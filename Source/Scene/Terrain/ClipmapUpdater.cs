using System;
using System.Drawing;
using OpenGlobe.Core;
using OpenGlobe.Renderer;
using System.Collections.Generic;

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

            _requestQueue.MessageReceived += TileLoadRequestReceived;
            _requestQueue.StartInAnotherThread();
        }

        public void Dispose()
        {
            _requestQueue.Dispose();
            _doneQueue.Dispose();
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

        public void ApplyNewData(Context context, ClipmapLevel[] levels)
        {
            // TODO: it would be nice if the MessageQueue gave us a way to do this directly without anonymous delegate trickery.
            List<RasterTerrainTile> tiles = new List<RasterTerrainTile>();
            EventHandler<MessageQueueEventArgs> handler = delegate(object sender, MessageQueueEventArgs e)
            {
                RasterTerrainTile tile = (RasterTerrainTile)e.Message;
                tiles.Add(tile);
                tile.IsLoading = false;
            };
            _doneQueue.MessageReceived += handler;
            _doneQueue.ProcessQueue();
            _doneQueue.MessageReceived -= handler;

            foreach (RasterTerrainTile tile in tiles)
            {
                int levelIndex = Array.FindIndex(levels, potentialLevel => potentialLevel.Terrain == tile.Level);
                if (levelIndex >= 0)
                {
                    ApplyNewTile(context, levels, levelIndex, tile);
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

            // Pad the tile extent by one post on all sides.
            // Normals in this larger extent could be affected by this tile's data.
            ClipmapUpdate thisTile = new ClipmapUpdate(
                level,
                tile.West - 1,
                tile.South - 1,
                tile.East + 1,
                tile.North + 1);

            ClipmapUpdate intersection = IntersectUpdates(entireLevel, thisTile);
            if (intersection.Width > 0 && intersection.Height > 0)
            {
                Update(context, intersection);

                // Recurse on child tiles if they're NOT loaded.  Unloaded children will use data from this tile.
                int childLevel = levelIndex + 1;
                if (childLevel < levels.Length)
                {
                    RasterTerrainTile southwest = tile.SouthwestChild;
                    if (southwest.Status != RasterTerrainTileStatus.Loaded)
                        ApplyNewTile(context, levels, levelIndex + 1, southwest);
                    RasterTerrainTile southeast = tile.SoutheastChild;
                    if (southeast.Status != RasterTerrainTileStatus.Loaded)
                        ApplyNewTile(context, levels, levelIndex + 1, southeast);
                    RasterTerrainTile northwest = tile.NorthwestChild;
                    if (northwest.Status != RasterTerrainTileStatus.Loaded)
                        ApplyNewTile(context, levels, levelIndex + 1, northwest);
                    RasterTerrainTile northeast = tile.NortheastChild;
                    if (northeast.Status != RasterTerrainTileStatus.Loaded)
                        ApplyNewTile(context, levels, levelIndex + 1, northeast);
                }
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
                // In order to compute normals, we need one extra post around the perimeter of the update region.
                ClipmapUpdate withBuffer = update.AddBuffer();

                RasterTerrainTileRegion[] tileRegions = level.Terrain.GetTilesInExtent(withBuffer.West, withBuffer.South, withBuffer.East, withBuffer.North);

                float[] posts = new float[withBuffer.Width * withBuffer.Height];

                foreach (RasterTerrainTileRegion region in tileRegions)
                {
                    if (region.Tile.Status == RasterTerrainTileStatus.Loaded)
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
            if (!tile.IsLoading)
            {
                tile.IsLoading = true;
                _requestQueue.Post(tile);
            }
        }

        private void UpsampleTileData(Context context, ClipmapLevel level, ClipmapUpdate update, RasterTerrainTileRegion region, float[] posts)
        {
            int west = region.Tile.West + region.West;
            int south = region.Tile.South + region.South;
            int east = region.Tile.West + region.East;
            int north = region.Tile.South + region.North;

            int tileWest = west - update.West;
            int tileSouth = south - update.South;
            int tileEast = east - update.West;
            int tileNorth = north - update.North;

            int stride = update.Width;
            int startIndex = tileSouth * stride + tileWest;

            int tileLongitudePosts = level.Terrain.LongitudePosts / level.Terrain.LongitudeTiles;
            int tileLatitudePosts = level.Terrain.LatitudePosts / level.Terrain.LatitudeTiles;

            int longitudePosts = tileEast - tileWest + 1;

            int writeIndex = startIndex;
            for (int j = tileSouth; j <= tileNorth; ++j)
            {
                int row = (tileLatitudePosts - j - 1) * (tileLongitudePosts + 1);
                for (int i = tileWest; i <= tileEast; ++i)
                {
                    posts[writeIndex] = 0.0f;
                    ++writeIndex;
                }
                writeIndex += stride - longitudePosts;
            }
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

        /// <summary>
        /// Invoked in the <see cref="_requestQueue"/> thread when a tile load request is received.
        /// </summary>
        private void TileLoadRequestReceived(object sender, MessageQueueEventArgs e)
        {
            RasterTerrainTile tile = (RasterTerrainTile)e.Message;
            tile.Load();
            _doneQueue.Post(tile);
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

        private MessageQueue _requestQueue = new MessageQueue();
        private MessageQueue _doneQueue = new MessageQueue();
    }
}
