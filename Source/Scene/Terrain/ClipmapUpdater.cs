using System;
using System.Drawing;
using OpenGlobe.Core;
using OpenGlobe.Renderer;
using System.Collections.Generic;

namespace OpenGlobe.Scene.Terrain
{
    internal class ClipmapUpdater : IDisposable
    {
        public ClipmapUpdater(GraphicsWindow window, Context context)
        {
            // TODO: We shouldn't have to pass in 'window'.  It's only need so that we can restore its context to being the
            // current one in the thread after we create a new context for the background loader thread.
            // Probably need some minor OpenTK work here.

            ShaderVertexAttributeCollection vertexAttributes = new ShaderVertexAttributeCollection();
            vertexAttributes.Add(new ShaderVertexAttribute("position", VertexLocations.Position, ShaderVertexAttributeType.FloatVector2, 1));

            Mesh unitQuad = RectangleTessellator.Compute(new RectangleD(new Vector2D(0.0, 0.0), new Vector2D(1.0, 1.0)), 1, 1);
            _unitQuad = context.CreateVertexArray(unitQuad, vertexAttributes, BufferHint.StaticDraw);
            _unitQuadPrimitiveType = unitQuad.PrimitiveType;
            _sceneState = new SceneState();
            _frameBuffer = context.CreateFrameBuffer();

            _updateShader = Device.CreateShaderProgram(
                EmbeddedResources.GetText("OpenGlobe.Scene.Terrain.ClipmapTerrain.ClipmapUpdateVS.glsl"),
                EmbeddedResources.GetText("OpenGlobe.Scene.Terrain.ClipmapTerrain.ClipmapUpdateFS.glsl"));
            _updateHeightOutput = _updateShader.FragmentOutputs["heightOutput"];
            _updateDrawState = new DrawState(new RenderState(), _updateShader, _unitQuad);
            _updateDrawState.RenderState.FacetCulling.FrontFaceWindingOrder = unitQuad.FrontFaceWindingOrder;
            _updateDrawState.RenderState.DepthTest.Enabled = false;
            _updateDestinationOffset = (Uniform<Vector2F>)_updateShader.Uniforms["u_destinationOffset"];
            _updateUpdateSize = (Uniform<Vector2F>)_updateShader.Uniforms["u_updateSize"];
            _updateSourceOrigin = (Uniform<Vector2F>)_updateShader.Uniforms["u_sourceOrigin"];

            _upsampleShader = Device.CreateShaderProgram(
                EmbeddedResources.GetText("OpenGlobe.Scene.Terrain.ClipmapTerrain.ClipmapUpsampleVS.glsl"),
                EmbeddedResources.GetText("OpenGlobe.Scene.Terrain.ClipmapTerrain.ClipmapUpsampleFS.glsl"));
            _upsampleHeightOutput = _upsampleShader.FragmentOutputs["heightOutput"];
            _upsampleDrawState = new DrawState(new RenderState(), _upsampleShader, _unitQuad);
            _upsampleDrawState.RenderState.FacetCulling.FrontFaceWindingOrder = unitQuad.FrontFaceWindingOrder;
            _upsampleDrawState.RenderState.DepthTest.Enabled = false;
            _upsampleOrigin = (Uniform<Vector2F>)_upsampleShader.Uniforms["u_updateOrigin"];
            _upsampleSize = (Uniform<Vector2F>)_upsampleShader.Uniforms["u_updateSize"];

            _computeNormalsShader = Device.CreateShaderProgram(
                EmbeddedResources.GetText("OpenGlobe.Scene.Terrain.ClipmapTerrain.ClipmapComputeNormalsVS.glsl"),
                EmbeddedResources.GetText("OpenGlobe.Scene.Terrain.ClipmapTerrain.ClipmapComputeNormalsFS.glsl"));
            _normalOutput = _computeNormalsShader.FragmentOutputs["normalOutput"];
            _computeNormalsDrawState = new DrawState(new RenderState(), _computeNormalsShader, _unitQuad);
            _computeNormalsDrawState.RenderState.FacetCulling.FrontFaceWindingOrder = unitQuad.FrontFaceWindingOrder;
            _computeNormalsDrawState.RenderState.DepthTest.Enabled = false;
            _computeNormalsOrigin = (Uniform<Vector2F>)_computeNormalsShader.Uniforms["u_origin"];
            _computeNormalsUpdateSize = (Uniform<Vector2F>)_computeNormalsShader.Uniforms["u_updateSize"];
            _computeNormalsOneOverHeightMapSize = (Uniform<Vector2F>)_computeNormalsShader.Uniforms["u_oneOverHeightMapSize"];
            _heightExaggeration = (Uniform<float>)_computeNormalsShader.Uniforms["u_heightExaggeration"];
            _postDelta = (Uniform<float>)_computeNormalsShader.Uniforms["u_postDelta"];

            HeightExaggeration = 1.0f;

            _workerWindow = Device.CreateWindow(1, 1);
            window.MakeCurrent();

            _requestQueue.MessageReceived += TileLoadRequestReceived;
            _requestQueue.Post(x => _workerWindow.MakeCurrent(), null);
            _requestQueue.StartInAnotherThread();
        }

        public void Dispose()
        {
            _requestQueue.Dispose();
            _doneQueue.Dispose();

            _computeNormalsShader.Dispose();
            _upsampleShader.Dispose();
            _updateShader.Dispose();

            _frameBuffer.Dispose();
            _unitQuad.Dispose();
        }

        public float HeightExaggeration
        {
            get { return _heightExaggeration.Value; }
            set { _heightExaggeration.Value = value; }
        }

        public void ApplyNewData(Context context, ClipmapLevel[] levels)
        {
            // TODO: it would be nice if the MessageQueue gave us a way to do this directly without anonymous delegate trickery.
            List<LoadedTile> tiles = new List<LoadedTile>();
            EventHandler<MessageQueueEventArgs> handler = delegate(object sender, MessageQueueEventArgs e)
            {
                LoadedTile tile = (LoadedTile)e.Message;
                _loadedTiles[tile.Tile.Identifier] = tile.Texture;
                tiles.Add(tile);
            };
            _doneQueue.MessageReceived += handler;
            _doneQueue.ProcessQueue();
            _doneQueue.MessageReceived -= handler;

            foreach (LoadedTile tile in tiles)
            {
                int levelIndex = Array.FindIndex(levels, potentialLevel => potentialLevel.Terrain == tile.Tile.Level);
                if (levelIndex >= 0)
                {
                    ApplyNewTile(context, levels, levelIndex, tile.Tile);
                }
            }
        }

        public void ApplyNewTile(Context context, ClipmapLevel[] levels, int levelIndex, RasterTerrainTile tile)
        {
            ClipmapLevel level = levels[levelIndex];
            ClipmapUpdate entireLevel = new ClipmapUpdate(
                level,
                level.NextExtent.West,
                level.NextExtent.South,
                level.NextExtent.East,
                level.NextExtent.North);

            ClipmapUpdate thisTile = new ClipmapUpdate(
                level,
                tile.West,
                tile.South,
                tile.East,
                tile.North);

            ClipmapUpdate intersection = IntersectUpdates(entireLevel, thisTile);

            if (intersection.Width > 0 && intersection.Height > 0)
            {
                Update(context, intersection);

                // Recurse on child tiles if they're NOT loaded.  Unloaded children will use data from this tile.
                int childLevel = levelIndex + 1;
                if (childLevel < levels.Length)
                {
                    ApplyIfNotLoaded(context, levels, childLevel, tile.SouthwestChild);
                    ApplyIfNotLoaded(context, levels, childLevel, tile.SoutheastChild);
                    ApplyIfNotLoaded(context, levels, childLevel, tile.NorthwestChild);
                    ApplyIfNotLoaded(context, levels, childLevel, tile.NortheastChild);
                }
            }
        }

        private void ApplyIfNotLoaded(Context context, ClipmapLevel[] levels, int levelIndex, RasterTerrainTile tile)
        {
            Texture2D texture;
            if (_loadedTiles.TryGetValue(tile.Identifier, out texture) && texture != null)
            {
                ApplyNewTile(context, levels, levelIndex, tile);
            }
        }

        private ClipmapUpdate IntersectUpdates(ClipmapUpdate first, ClipmapUpdate second)
        {
            int west = Math.Max(first.West, second.West);
            int south = Math.Max(first.South, second.South);
            int east = Math.Min(first.East, second.East);
            int north = Math.Min(first.North, second.North);
            return new ClipmapUpdate(first.Level, west, south, east, north);
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
                RasterTerrainTileRegion[] tileRegions = level.Terrain.GetTilesInExtent(update.West, update.South, update.East, update.North);
                foreach (RasterTerrainTileRegion region in tileRegions)
                {
                    // TODO: Remove this
                    _loadedTiles[region.Tile.Identifier] = CreateTextureFromTile(region.Tile);

                    Texture2D tileTexture;
                    bool loadingOrLoaded = _loadedTiles.TryGetValue(region.Tile.Identifier, out tileTexture);
                    if (loadingOrLoaded && tileTexture != null)
                    {
                        RenderTileToLevelHeightTexture(context, update, region, tileTexture);
                    }
                    else
                    {
                        if (!loadingOrLoaded)
                        {
                            RequestTileLoad(region.Tile);
                        }
                        UpsampleTileData(context, update, region);
                    }
                }

                UpdateNormals(context, update);
            }
        }

        private void RenderTileToLevelHeightTexture(Context context, ClipmapUpdate update, RasterTerrainTileRegion region, Texture2D texture)
        {
            ClipmapLevel level = update.Level;

            context.TextureUnits[0].Texture = texture;
            context.TextureUnits[0].TextureSampler = Device.TextureSamplers.NearestClamp;
            
            _frameBuffer.ColorAttachments[_updateHeightOutput] = level.HeightTexture;

            int clipmapSize = level.NextExtent.East - level.NextExtent.West + 1;
            int destWest = (level.OriginInTextures.X + (region.Tile.West + region.West - level.NextExtent.West)) % clipmapSize;
            int destSouth = (level.OriginInTextures.Y + (region.Tile.South + region.South - level.NextExtent.South)) % clipmapSize;

            int width = region.East - region.West + 1;
            int height = region.North - region.South + 1;

            _updateSourceOrigin.Value = new Vector2F(region.West, region.South);
            _updateUpdateSize.Value = new Vector2F(width, height);
            _updateDestinationOffset.Value = new Vector2F(destWest, destSouth);

            // Save the current state of the context
            Rectangle oldViewport = context.Viewport;
            FrameBuffer oldFrameBuffer = context.FrameBuffer;

            // Update the context and draw
            context.Viewport = new Rectangle(0, 0, level.HeightTexture.Description.Width, level.HeightTexture.Description.Height);
            context.FrameBuffer = _frameBuffer;
            context.Draw(_unitQuadPrimitiveType, _updateDrawState, _sceneState);

            // Restore the context to its original state
            context.FrameBuffer = oldFrameBuffer;
            context.Viewport = oldViewport;
        }

        private void UpsampleTileData(Context context, ClipmapUpdate update, RasterTerrainTileRegion region)
        {
        }

        private void UpdateNormals(Context context, ClipmapUpdate update)
        {
            // TODO: There are artifacts in the normal because normals on the edge are computed based on the texture wrap.  We don't care about the normals
            // on the edge, so this is ok so far.  However, when the viewer moves, those normals move away from the edge, and then we do care about them.
            // So we need to update them when they move away from the edge.  The AddBuffer call does this, sorta kinda maybe in theory, but
            // something still isn't working quite right.
            update = update.AddBuffer();

            ClipmapLevel level = update.Level;

            context.TextureUnits[0].Texture = update.Level.HeightTexture;
            context.TextureUnits[0].TextureSampler = Device.TextureSamplers.NearestClamp;

            _frameBuffer.ColorAttachments[_normalOutput] = level.NormalTexture;

            int clipmapSize = level.NextExtent.East - level.NextExtent.West + 1;
            int west = (level.OriginInTextures.X + (update.West - level.NextExtent.West)) % clipmapSize;
            int south = (level.OriginInTextures.Y + (update.South - level.NextExtent.South)) % clipmapSize;

            _computeNormalsUpdateSize.Value = new Vector2F(update.Width, update.Height);
            _computeNormalsOrigin.Value = new Vector2F(west, south);
            _computeNormalsOneOverHeightMapSize.Value = new Vector2F(1.0f / update.Level.HeightTexture.Description.Width, 1.0f / update.Level.HeightTexture.Description.Height);
            _postDelta.Value = (float)update.Level.Terrain.PostDeltaLongitude;

            // Save the current state of the context
            Rectangle oldViewport = context.Viewport;
            FrameBuffer oldFrameBuffer = context.FrameBuffer;

            // Update the context and draw
            context.Viewport = new Rectangle(0, 0, level.NormalTexture.Description.Width, level.NormalTexture.Description.Height);
            context.FrameBuffer = _frameBuffer;
            context.Draw(_unitQuadPrimitiveType, _computeNormalsDrawState, _sceneState);

            // Restore the context to its original state
            context.FrameBuffer = oldFrameBuffer;
            context.Viewport = oldViewport;
        }

        private void RequestTileLoad(RasterTerrainTile tile)
        {
            _loadedTiles[tile.Identifier] = null;
            _requestQueue.Post(tile);
        }

        private Texture2D CreateTextureFromTile(RasterTerrainTile tile)
        {
            int width = tile.East - tile.West + 1;
            int height = tile.North - tile.South + 1;

            Texture2DDescription description = new Texture2DDescription(width, height, TextureFormat.Red16f, false);
            Texture2D texture = Device.CreateTexture2DRectangle(description);

            using (WritePixelBuffer wpb = Device.CreateWritePixelBuffer(PixelBufferHint.Stream, width * height * sizeof(float)))
            {
                float[] posts = new float[width * height];
                tile.GetPosts(0, 0, width - 1, height - 1, posts, 0, width);

                wpb.CopyFromSystemMemory(posts);
                texture.CopyFromBuffer(wpb, ImageFormat.Red, ImageDatatype.Float);
            }

            return texture;
        }

        /// <summary>
        /// Invoked in the <see cref="_requestQueue"/> thread when a tile load request is received.
        /// </summary>
        private void TileLoadRequestReceived(object sender, MessageQueueEventArgs e)
        {
            RasterTerrainTile tile = (RasterTerrainTile)e.Message;
            Texture2D texture = CreateTextureFromTile(tile);
            
            Fence fence = Device.CreateFence();
            fence.ClientWait();

            LoadedTile message = new LoadedTile();
            message.Tile = tile;
            message.Texture = texture;

            _doneQueue.Post(message);
        }

        private class LoadedTile
        {
            public RasterTerrainTile Tile;
            public Texture2D Texture;
        }

        private VertexArray _unitQuad;
        private PrimitiveType _unitQuadPrimitiveType;
        private SceneState _sceneState;
        private FrameBuffer _frameBuffer;

        private ShaderProgram _updateShader;
        private int _updateHeightOutput;
        private DrawState _updateDrawState;
        private Uniform<Vector2F> _updateDestinationOffset;
        private Uniform<Vector2F> _updateUpdateSize;
        private Uniform<Vector2F> _updateSourceOrigin;

        private ShaderProgram _upsampleShader;
        private int _upsampleHeightOutput;
        private DrawState _upsampleDrawState;
        private Uniform<Vector2F> _upsampleOrigin;
        private Uniform<Vector2F> _upsampleSize;

        private ShaderProgram _computeNormalsShader;
        private int _normalOutput;
        private DrawState _computeNormalsDrawState;
        private Uniform<Vector2F> _computeNormalsOrigin;
        private Uniform<Vector2F> _computeNormalsUpdateSize;
        private Uniform<float> _heightExaggeration;
        private Uniform<float> _postDelta;

        private Dictionary<RasterTerrainTileIdentifier, Texture2D> _loadedTiles = new Dictionary<RasterTerrainTileIdentifier, Texture2D>();

        private GraphicsWindow _workerWindow;
        private MessageQueue _requestQueue = new MessageQueue();
        private MessageQueue _doneQueue = new MessageQueue();
        private Uniform<Vector2F> _computeNormalsOneOverHeightMapSize;
    }
}
