using System;
using System.Drawing;
using OpenGlobe.Core;
using OpenGlobe.Renderer;

namespace OpenGlobe.Scene.Terrain
{
    internal class ClipmapUpdater : IDisposable
    {
        public ClipmapUpdater(Context context, int maxUpdate)
        {
            _maxUpdate = maxUpdate;

            ShaderProgram shaderProgram = Device.CreateShaderProgram(
                EmbeddedResources.GetText("OpenGlobe.Scene.Terrain.ClipmapTerrain.ClipmapUpdateVS.glsl"),
                EmbeddedResources.GetText("OpenGlobe.Scene.Terrain.ClipmapTerrain.ClipmapUpdateFS.glsl"));
            _heightOutput = shaderProgram.FragmentOutputs["heightOutput"];
            _normalOutput = shaderProgram.FragmentOutputs["normalOutput"];

            _heightExaggeration = (Uniform<float>)shaderProgram.Uniforms["u_heightExaggeration"];
            _postDelta = (Uniform<float>)shaderProgram.Uniforms["u_postDelta"];

            _updateOrigin = (Uniform<Vector2F>)shaderProgram.Uniforms["u_updateOrigin"];
            _updateSize = (Uniform<Vector2F>)shaderProgram.Uniforms["u_updateSize"];

            Mesh quad = RectangleTessellator.Compute(new RectangleD(new Vector2D(0.0, 0.0), new Vector2D(1.0, 1.0)), 1, 1);
            VertexArray quadVertexArray = context.CreateVertexArray(quad, shaderProgram.VertexAttributes, BufferHint.StaticDraw);
            _primitiveType = quad.PrimitiveType;

            _sourceTexture = Device.CreateTexture2DRectangle(new Texture2DDescription(_maxUpdate, _maxUpdate, TextureFormat.Red32f));

            _drawState = new DrawState(new RenderState(), shaderProgram, quadVertexArray);
            _drawState.RenderState.FacetCulling.FrontFaceWindingOrder = quad.FrontFaceWindingOrder;
            _drawState.RenderState.DepthTest.Enabled = false;
            _sceneState = new SceneState();

            _frameBuffer = context.CreateFrameBuffer();

            HeightExaggeration = 1.0f;
        }

        public void Dispose()
        {
            _frameBuffer.Dispose();
            _drawState.ShaderProgram.Dispose();
            _drawState.VertexArray.Dispose();
            _sourceTexture.Dispose();
        }

        public float HeightExaggeration
        {
            get { return _heightExaggeration.Value; }
            set { _heightExaggeration.Value = value; }
        }

        public void ApplyNewData(Context context, ClipmapLevel level)
        {
            // How do we compute normals at the edges of a new tile?
            // We'll need the adjacent tile as well.
            // And we should update the normals on the edge of that adjacent tile as well, in case
            // this tile wasn't available when those normals were computed, so they were computed from
            // less detailed heights.
            // A corner post might have two adjacent tiles.
        }

        public void Update(Context context, ClipmapUpdate update)
        {
            ClipmapLevel level = update.Level;
            int clipmapSize = level.NextExtent.East - level.NextExtent.West + 1;

            int west = (level.OriginInTextures.X + (update.West - level.NextExtent.West)) % clipmapSize;
            int east = (level.OriginInTextures.X + (update.East - level.NextExtent.West)) % clipmapSize;
            int south = (level.OriginInTextures.Y + (update.South - level.NextExtent.South)) % clipmapSize;
            int north = (level.OriginInTextures.Y + (update.North - level.NextExtent.South)) % clipmapSize;

            if (east < west)
            {
                // Horizontal wrap
                ClipmapUpdate leftUpdate = new ClipmapUpdate(
                    level,
                    update.West,
                    update.South,
                    level.NextExtent.West + (clipmapSize - level.OriginInTextures.X - 1),
                    update.North);
                Update(context, leftUpdate);

                ClipmapUpdate rightUpdate = new ClipmapUpdate(
                    level,
                    level.NextExtent.West + clipmapSize - level.OriginInTextures.X,
                    update.South,
                    update.East,
                    update.North);
                Update(context, rightUpdate);
            }
            else if (north < south)
            {
                // Vertical wrap
                ClipmapUpdate bottomUpdate = new ClipmapUpdate(
                    level,
                    update.West,
                    update.South,
                    update.East,
                    level.NextExtent.South + (clipmapSize - level.OriginInTextures.Y - 1));
                Update(context, bottomUpdate);

                ClipmapUpdate topUpdate = new ClipmapUpdate(
                    level,
                    update.West,
                    level.NextExtent.South + clipmapSize - level.OriginInTextures.Y,
                    update.East,
                    update.North);
                Update(context, topUpdate);
            }
            else
            {
                // In order to compute normals, we need one extra post around the perimeter of the update region.
                ClipmapUpdate withBuffer = update.AddBuffer();

                RasterTerrainTileRegion[] tileRegions = level.Terrain.GetTilesInExtent(withBuffer.West, withBuffer.South, withBuffer.East, withBuffer.North);

                float[] posts = new float[withBuffer.Width * withBuffer.Height];

                foreach (RasterTerrainTileRegion region in tileRegions)
                {
                    if (region.Tile.IsLoaded)
                    {
                        UpdateTile(context, level, withBuffer, region, posts);
                    }
                    else
                    {
                        RequestTileLoad(region.Tile);
                        UpsampleTileData(context, level, withBuffer, region, posts);
                    }
                }

                RenderToLevelTextures(context, update, withBuffer, posts);
            }
        }

        private void UpdateTile(Context context, ClipmapLevel level, ClipmapUpdate update, RasterTerrainTileRegion region, float[] posts)
        {
            int west = region.Tile.West + region.West;
            int south = region.Tile.South + region.South;

            int westOffset = west - update.West;
            int southOffset = south - update.South;

            int stride = update.Width;
            int startIndex = southOffset * stride + westOffset;

            region.Tile.GetPosts(region.West, region.South, region.East, region.North, posts, startIndex, stride);
        }

        private void RequestTileLoad(RasterTerrainTile tile)
        {
        }

        private void UpsampleTileData(Context context, ClipmapLevel level, ClipmapUpdate update, RasterTerrainTileRegion region, float[] posts)
        {
        }

        private void RenderToLevelTextures(Context context, ClipmapUpdate update, ClipmapUpdate withBuffer, float[] posts)
        {
            ClipmapLevel level = update.Level;

            // Copy the post data into the source texture.
            using (WritePixelBuffer pixelBuffer = Device.CreateWritePixelBuffer(PixelBufferHint.Stream, posts.Length * sizeof(float)))
            {
                pixelBuffer.CopyFromSystemMemory(posts);
                _sourceTexture.CopyFromBuffer(pixelBuffer, 0, 0, withBuffer.Width, withBuffer.Height, ImageFormat.Red, ImageDatatype.Float, 4);
            }

            context.TextureUnits[0].Texture = _sourceTexture;
            context.TextureUnits[0].TextureSampler = Device.TextureSamplers.NearestClamp;
            _postDelta.Value = (float)level.Terrain.PostDeltaLongitude;

            // Set the target location
            _frameBuffer.ColorAttachments[_heightOutput] = level.HeightTexture;
            _frameBuffer.ColorAttachments[_normalOutput] = level.NormalTexture;
            _updateSize.Value = new Vector2F(update.Width, update.Height);

            int clipmapSize = level.NextExtent.East - level.NextExtent.West + 1;
            int west = (level.OriginInTextures.X + (update.West - level.NextExtent.West)) % clipmapSize;
            int south = (level.OriginInTextures.Y + (update.South - level.NextExtent.South)) % clipmapSize;
            _updateOrigin.Value = new Vector2F(west, south);

            Rectangle oldViewport = context.Viewport;
            context.Viewport = new Rectangle(0, 0, level.HeightTexture.Description.Width, level.HeightTexture.Description.Height);

            // Render to the frame buffer
            FrameBuffer oldFrameBuffer = context.FrameBuffer;
            context.FrameBuffer = _frameBuffer;

            context.Draw(_primitiveType, _drawState, _sceneState);

            // Restore the context to its original state
            context.FrameBuffer = oldFrameBuffer;
            context.Viewport = oldViewport;
        }

        private float[] GetPostSubset(float[] posts, int sourceStride, int xMin, int yMin, int xMax, int yMax)
        {
            int width = xMax - xMin + 1;
            int height = yMax - yMin + 1;

            float[] result = new float[width * height];

            for (int j = 0; j < height; ++j)
            {
                for (int i = 0; i < width; ++i)
                {
                    result[j * width + i] = posts[(j + yMin) * sourceStride + i + xMin];
                }
            }
            
            return result;
        }

        private int _maxUpdate;
        private Texture2D _sourceTexture;
        private SceneState _sceneState;
        private DrawState _drawState;
        private FrameBuffer _frameBuffer;
        private PrimitiveType _primitiveType;
        private Uniform<float> _heightExaggeration;
        private Uniform<float> _postDelta;
        private Uniform<Vector2F> _updateOrigin;
        private Uniform<Vector2F> _updateSize;
        private int _heightOutput;
        private int _normalOutput;
    }
}
