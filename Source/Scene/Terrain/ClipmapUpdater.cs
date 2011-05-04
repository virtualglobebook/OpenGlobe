//#define SingleThreaded
using System;
using System.Drawing;
using OpenGlobe.Core;
using OpenGlobe.Renderer;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using OpenGlobe.Scene;

namespace OpenGlobe.Scene
{
    internal class ClipmapUpdater : IDisposable
    {
        public ClipmapUpdater(Context context, ClipmapLevel[] clipmapLevels)
        {
            ShaderVertexAttributeCollection vertexAttributes = new ShaderVertexAttributeCollection();
            vertexAttributes.Add(new ShaderVertexAttribute("position", VertexLocations.Position, ShaderVertexAttributeType.FloatVector2, 1));

            Mesh unitQuad = RectangleTessellator.Compute(new RectangleD(new Vector2D(0.0, 0.0), new Vector2D(1.0, 1.0)), 1, 1);
            _unitQuad = context.CreateVertexArray(unitQuad, vertexAttributes, BufferHint.StaticDraw);
            _unitQuadPrimitiveType = unitQuad.PrimitiveType;
            _sceneState = new SceneState();
            _framebuffer = context.CreateFramebuffer();

            _updateShader = Device.CreateShaderProgram(
                EmbeddedResources.GetText("OpenGlobe.Scene.Terrain.ClipmapTerrain.ClipmapUpdateVS.glsl"),
                EmbeddedResources.GetText("OpenGlobe.Scene.Terrain.ClipmapTerrain.ClipmapUpdateFS.glsl"));
            _updateTexelOutput = _updateShader.FragmentOutputs["texelOutput"];
            _updateDrawState = new DrawState(new RenderState(), _updateShader, _unitQuad);
            _updateDrawState.RenderState.FacetCulling.FrontFaceWindingOrder = unitQuad.FrontFaceWindingOrder;
            _updateDrawState.RenderState.DepthTest.Enabled = false;
            _updateDestinationOffset = (Uniform<Vector2F>)_updateShader.Uniforms["u_destinationOffset"];
            _updateUpdateSize = (Uniform<Vector2F>)_updateShader.Uniforms["u_updateSize"];
            _updateSourceOrigin = (Uniform<Vector2F>)_updateShader.Uniforms["u_sourceOrigin"];

            _upsampleShader = Device.CreateShaderProgram(
                EmbeddedResources.GetText("OpenGlobe.Scene.Terrain.ClipmapTerrain.ClipmapUpsampleVS.glsl"),
                EmbeddedResources.GetText("OpenGlobe.Scene.Terrain.ClipmapTerrain.ClipmapUpsampleFS.glsl"));
            _upsampleTexelOutput = _upsampleShader.FragmentOutputs["texelOutput"];
            _upsampleDrawState = new DrawState(new RenderState(), _upsampleShader, _unitQuad);
            _upsampleDrawState.RenderState.FacetCulling.FrontFaceWindingOrder = unitQuad.FrontFaceWindingOrder;
            _upsampleDrawState.RenderState.DepthTest.Enabled = false;
            _upsampleSourceOrigin = (Uniform<Vector2F>)_upsampleShader.Uniforms["u_sourceOrigin"];
            _upsampleUpdateSize = (Uniform<Vector2F>)_upsampleShader.Uniforms["u_updateSize"];
            _upsampleDestinationOffset = (Uniform<Vector2F>)_upsampleShader.Uniforms["u_destinationOffset"];
            _upsampleOneOverTextureSize = (Uniform<Vector2F>)_upsampleShader.Uniforms["u_oneOverTextureSize"];

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

            ClipmapLevel levelZero = clipmapLevels[0];
            InitializeRequestThreads(context, _terrain, levelZero, levelZero.Terrain);
            InitializeRequestThreads(context, _imagery, levelZero, levelZero.Imagery);
        }

        private void InitializeRequestThreads(Context context, RasterDataDetails details, ClipmapLevel clipmapLevelZero, RasterLevel rasterLevelZero)
        {
            details.WorkerWindow = Device.CreateWindow(1, 1);
            context.MakeCurrent();

#if !SingleThreaded
            Thread requestThread = new Thread(RequestThreadEntryPoint);
            requestThread.IsBackground = true;
            requestThread.Start(details);
#endif

            // Preload the entire world at level 0
            RasterTileRegion[] regions = rasterLevelZero.GetTilesInExtent(0, 0, rasterLevelZero.LongitudePosts - 1, rasterLevelZero.LatitudePosts - 1);
            foreach (RasterTileRegion region in regions)
            {
                RequestTileLoad(details, clipmapLevelZero, region.Tile);
            }
        }

        public void Dispose()
        {
            _terrain.DoneQueue.Dispose();
            _imagery.DoneQueue.Dispose();

            _computeNormalsShader.Dispose();
            _upsampleShader.Dispose();
            _updateShader.Dispose();

            _framebuffer.Dispose();
            _unitQuad.Dispose();
        }

        public float HeightExaggeration
        {
            get { return _heightExaggeration.Value; }
            set { _heightExaggeration.Value = value; }
        }

        public void ApplyNewData(Context context)
        {
            ApplyNewData(context, _terrain);
            ApplyNewData(context, _imagery);
        }

        private void ApplyNewData(Context context, RasterDataDetails details)
        {
            // This is the start of a new frame, so the next request goes at the end
            details.RequestInsertionPoint = null;

#if SingleThreaded
            details.RequestQueue.ProcessQueue();
#endif

            List<TileLoadRequest> tiles = new List<TileLoadRequest>();
            EventHandler<MessageQueueEventArgs> handler = delegate(object sender, MessageQueueEventArgs e)
            {
                TileLoadRequest tile = (TileLoadRequest)e.Message;
                details.LoadedTiles[tile.Tile.Identifier] = tile.Texture;
                details.LoadingTiles.Remove(tile.Tile.Identifier);
                tiles.Add(tile);
            };
            details.DoneQueue.MessageReceived += handler;
            details.DoneQueue.ProcessQueue();
            details.DoneQueue.MessageReceived -= handler;

            foreach (TileLoadRequest tile in tiles)
            {
                ApplyNewTile(context, details, tile.Level, tile.Tile);
            }
        }

        private void ApplyNewTile(Context context, RasterDataDetails details, ClipmapLevel level, RasterTile tile)
        {
            ClipmapLevel.Extent nextExtent = details.Type == RasterType.Terrain ? level.NextExtent : level.NextImageryExtent;
            RasterLevel rasterLevel = details.Type == RasterType.Terrain ? level.Terrain : level.Imagery;

            ClipmapUpdate entireLevel = new ClipmapUpdate(
                level,
                nextExtent.West,
                nextExtent.South,
                nextExtent.East,
                nextExtent.North);

            ClipmapUpdate thisTile = new ClipmapUpdate(
                level,
                tile.West - 1,
                tile.South - 1,
                tile.East + 1,
                tile.North + 1);

            ClipmapUpdate intersection = IntersectUpdates(entireLevel, thisTile);

            if (intersection.Width > 0 && intersection.Height > 0)
            {
                Update(context, intersection, level, details, rasterLevel);

                // Recurse on child tiles if they're NOT loaded.  Unloaded children will use data from this tile.
                ClipmapLevel finer = level.FinerLevel;
                if (finer != null)
                {
                    ApplyIfNotLoaded(context, details, finer, tile.SouthwestChild);
                    ApplyIfNotLoaded(context, details, finer, tile.SoutheastChild);
                    ApplyIfNotLoaded(context, details, finer, tile.NorthwestChild);
                    ApplyIfNotLoaded(context, details, finer, tile.NortheastChild);
                }
            }
        }

        private void ApplyIfNotLoaded(Context context, RasterDataDetails details, ClipmapLevel level, RasterTile tile)
        {
            Texture2D texture;
            if (!details.LoadedTiles.TryGetValue(tile.Identifier, out texture) || texture == null)
            {
                ApplyNewTile(context, details, level, tile);
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

        public void RequestTileResidency(Context context, ClipmapLevel level)
        {
            RequestTileResidency(context, level, _terrain, level.Terrain, level.NextExtent);
            RequestTileResidency(context, level, _imagery, level.Imagery, level.NextImageryExtent);
        }

        private void RequestTileResidency(Context context, ClipmapLevel level, RasterDataDetails details, RasterLevel rasterLevel, ClipmapLevel.Extent nextExtent)
        {
            RasterTileRegion[] tileRegions = rasterLevel.GetTilesInExtent(nextExtent.West, nextExtent.South, nextExtent.East, nextExtent.North);
            foreach (RasterTileRegion region in tileRegions)
            {
                if (!details.LoadedTiles.ContainsKey(region.Tile.Identifier))
                {
                    RequestTileLoad(details, level, region.Tile);
                }
            }
        }

        public void UpdateTerrain(Context context, ClipmapUpdate update)
        {
            ClipmapLevel level = update.Level;
            Update(context, update, level, _terrain, level.Terrain);
        }

        public void UpdateImagery(Context context, ClipmapUpdate update)
        {
            ClipmapLevel level = update.Level;
            Update(context, update, level, _imagery, level.Imagery);
        }

        private void Update(Context context, ClipmapUpdate update, ClipmapLevel level, RasterDataDetails details, RasterLevel rasterLevel)
        {
            ClipmapUpdate[] updates = SplitUpdateToAvoidWrapping(update, details);
            foreach (ClipmapUpdate nonWrappingUpdate in updates)
            {
                RasterTileRegion[] tileRegions = rasterLevel.GetTilesInExtent(nonWrappingUpdate.West, nonWrappingUpdate.South, nonWrappingUpdate.East, nonWrappingUpdate.North);
                foreach (RasterTileRegion region in tileRegions)
                {
                    Texture2D tileTexture;
                    bool loaded = details.LoadedTiles.TryGetValue(region.Tile.Identifier, out tileTexture);
                    if (loaded)
                    {
                        RenderTileToLevelTexture(context, level, details, region, tileTexture);
                    }
                    else
                    {
                        UpsampleTileData(context, level, details, region);
                    }
                }
            }

            if (details.Type == RasterType.Terrain)
            {
                // Normals at edges are incorrect, so include a one-post buffer around the update region
                // when updating normals in order to update normals that were previously at the edge.
                ClipmapUpdate updateWithBuffer = update.AddBufferWithinLevelNextExtent();
                ClipmapUpdate[] normalUpdates = SplitUpdateToAvoidWrapping(updateWithBuffer, details);
                foreach (ClipmapUpdate normalUpdate in normalUpdates)
                {
                    UpdateNormals(context, normalUpdate);
                }
            }
        }

        private ClipmapUpdate[] SplitUpdateToAvoidWrapping(ClipmapUpdate update, RasterDataDetails details)
        {
            ClipmapLevel level = update.Level;
            Vector2I origin = details.Type == RasterType.Terrain ? level.OriginInTextures : level.OriginInImagery;
            ClipmapLevel.Extent extent = details.Type == RasterType.Terrain ? level.NextExtent : level.NextImageryExtent;

            int clipmapSizeX = extent.East - extent.West + 1;
            int clipmapSizeY = extent.North - extent.South + 1;

            int west = (origin.X + (update.West - extent.West)) % clipmapSizeX;
            int east = (origin.X + (update.East - extent.West)) % clipmapSizeX;
            int south = (origin.Y + (update.South - extent.South)) % clipmapSizeY;
            int north = (origin.Y + (update.North - extent.South)) % clipmapSizeY;

            if (east < west && north < south)
            {
                // Horizontal AND vertical wrap
                ClipmapUpdate bottomLeftUpdate = new ClipmapUpdate(
                    level,
                    update.West,
                    update.South,
                    extent.West + (clipmapSizeX - origin.X - 1),
                    extent.South + (clipmapSizeY - origin.Y - 1));

                ClipmapUpdate bottomRightUpdate = new ClipmapUpdate(
                    level,
                    extent.West + clipmapSizeX - origin.X,
                    update.South,
                    update.East,
                    extent.South + (clipmapSizeY - origin.Y - 1));

                ClipmapUpdate topLeftUpdate = new ClipmapUpdate(
                    level,
                    update.West,
                    extent.South + clipmapSizeY - origin.Y,
                    extent.West + (clipmapSizeX - origin.X - 1),
                    update.North);

                ClipmapUpdate topRightUpdate = new ClipmapUpdate(
                    level,
                    extent.West + clipmapSizeX - origin.X,
                    extent.South + clipmapSizeY - origin.Y,
                    update.East,
                    update.North);

                ClipmapUpdate[] result = new ClipmapUpdate[4];
                result[0] = bottomLeftUpdate;
                result[1] = bottomRightUpdate;
                result[2] = topLeftUpdate;
                result[3] = topRightUpdate;
                return result;
            }
            else if (east < west)
            {
                // Horizontal wrap
                ClipmapUpdate leftUpdate = new ClipmapUpdate(
                    level,
                    update.West,
                    update.South,
                    extent.West + (clipmapSizeX - origin.X - 1),
                    update.North);

                ClipmapUpdate rightUpdate = new ClipmapUpdate(
                    level,
                    extent.West + clipmapSizeX - origin.X,
                    update.South,
                    update.East,
                    update.North);

                ClipmapUpdate[] result = new ClipmapUpdate[2];
                result[0] = leftUpdate;
                result[1] = rightUpdate;
                return result;
            }
            else if (north < south)
            {
                // Vertical wrap
                ClipmapUpdate bottomUpdate = new ClipmapUpdate(
                    level,
                    update.West,
                    update.South,
                    update.East,
                    extent.South + (clipmapSizeY - origin.Y - 1));

                ClipmapUpdate topUpdate = new ClipmapUpdate(
                    level,
                    update.West,
                    extent.South + clipmapSizeY - origin.Y,
                    update.East,
                    update.North);

                ClipmapUpdate[] result = new ClipmapUpdate[2];
                result[0] = bottomUpdate;
                result[1] = topUpdate;
                return result;
            }
            else
            {
                // No wrap
                ClipmapUpdate[] result = new ClipmapUpdate[1];
                result[0] = update;
                return result;
            }
        }

        private void RenderTileToLevelTexture(Context context, ClipmapLevel level, RasterDataDetails details, RasterTileRegion region, Texture2D tileTexture)
        {
            Texture2D levelTexture;
            Vector2I originInTextures;
            ClipmapLevel.Extent nextExtent;

            if (details.Type == RasterType.Terrain)
            {
                levelTexture = level.HeightTexture;
                originInTextures = level.OriginInTextures;
                nextExtent = level.NextExtent;
            }
            else
            {
                levelTexture = level.ImageryTexture;
                originInTextures = level.OriginInImagery;
                nextExtent = level.NextImageryExtent;
            }

            context.TextureUnits[0].Texture = tileTexture;
            context.TextureUnits[0].TextureSampler = Device.TextureSamplers.NearestClamp;

            _framebuffer.ColorAttachments[_updateTexelOutput] = levelTexture;

            int clipmapSize = nextExtent.East - nextExtent.West + 1;
            int destWest = (originInTextures.X + (region.Tile.West + region.West - nextExtent.West)) % clipmapSize;
            int destSouth = (originInTextures.Y + (region.Tile.South + region.South - nextExtent.South)) % clipmapSize;

            int width = region.East - region.West + 1;
            int height = region.North - region.South + 1;

            _updateSourceOrigin.Value = new Vector2F(region.West, region.South);
            _updateUpdateSize.Value = new Vector2F(width, height);
            _updateDestinationOffset.Value = new Vector2F(destWest, destSouth);

            // Save the current state of the context
            Rectangle oldViewport = context.Viewport;
            Framebuffer oldFramebuffer = context.Framebuffer;

            // Update the context and draw
            context.Viewport = new Rectangle(0, 0, levelTexture.Description.Width, levelTexture.Description.Height);
            context.Framebuffer = _framebuffer;
            context.Draw(_unitQuadPrimitiveType, _updateDrawState, _sceneState);

            // Restore the context to its original state
            context.Framebuffer = oldFramebuffer;
            context.Viewport = oldViewport;
        }

        private void UpsampleTileData(Context context, ClipmapLevel level, RasterDataDetails details, RasterTileRegion region)
        {
            ClipmapLevel coarserLevel = level.CoarserLevel;

            if (coarserLevel == null)
                return;

            Texture2D levelTexture;
            Texture2D coarserLevelTexture;
            Vector2I originInTextures;
            Vector2I coarserOriginInTextures;
            ClipmapLevel.Extent nextExtent;
            ClipmapLevel.Extent coarserNextExtent;

            if (details.Type == RasterType.Terrain)
            {
                levelTexture = level.HeightTexture;
                coarserLevelTexture = coarserLevel.HeightTexture;
                originInTextures = level.OriginInTextures;
                coarserOriginInTextures = coarserLevel.OriginInTextures;
                nextExtent = level.NextExtent;
                coarserNextExtent = coarserLevel.NextExtent;
            }
            else
            {
                levelTexture = level.ImageryTexture;
                coarserLevelTexture = coarserLevel.ImageryTexture;
                originInTextures = level.OriginInImagery;
                coarserOriginInTextures = coarserLevel.OriginInImagery;
                nextExtent = level.NextImageryExtent;
                coarserNextExtent = coarserLevel.NextImageryExtent;
            }

            context.TextureUnits[0].Texture = coarserLevelTexture;
            context.TextureUnits[0].TextureSampler = Device.TextureSamplers.LinearRepeat;

            _framebuffer.ColorAttachments[_upsampleTexelOutput] = levelTexture;

            int fineClipmapSize = nextExtent.East - nextExtent.West + 1;
            int destWest = (originInTextures.X + (region.Tile.West + region.West - nextExtent.West)) % fineClipmapSize;
            int destSouth = (originInTextures.Y + (region.Tile.South + region.South - nextExtent.South)) % fineClipmapSize;

            int coarseClipmapSize = coarserNextExtent.East - coarserNextExtent.West + 1;
            double sourceWest = (coarserOriginInTextures.X + ((region.Tile.West + region.West) / 2.0 - coarserNextExtent.West)) % coarseClipmapSize;
            double sourceSouth = (coarserOriginInTextures.Y + ((region.Tile.South + region.South) / 2.0 - coarserNextExtent.South)) % coarseClipmapSize;

            int width = region.East - region.West + 1;
            int height = region.North - region.South + 1;

            _upsampleSourceOrigin.Value = new Vector2F((float)sourceWest, (float)sourceSouth);
            _upsampleUpdateSize.Value = new Vector2F(width, height);
            _upsampleDestinationOffset.Value = new Vector2F(destWest, destSouth);
            _upsampleOneOverTextureSize.Value = new Vector2F(1.0f / coarserLevelTexture.Description.Width, 1.0f / coarserLevelTexture.Description.Height);

            // Save the current state of the context
            Rectangle oldViewport = context.Viewport;
            Framebuffer oldFramebuffer = context.Framebuffer;

            // Update the context and draw
            context.Viewport = new Rectangle(0, 0, levelTexture.Description.Width, levelTexture.Description.Height);
            context.Framebuffer = _framebuffer;
            context.Draw(_unitQuadPrimitiveType, _upsampleDrawState, _sceneState);

            // Restore the context to its original state
            context.Framebuffer = oldFramebuffer;
            context.Viewport = oldViewport;
        }

        private void UpdateNormals(Context context, ClipmapUpdate update)
        {
            ClipmapLevel level = update.Level;

            context.TextureUnits[0].Texture = update.Level.HeightTexture;
            context.TextureUnits[0].TextureSampler = Device.TextureSamplers.NearestRepeat;

            _framebuffer.ColorAttachments[_normalOutput] = level.NormalTexture;

            int clipmapSize = level.NextExtent.East - level.NextExtent.West + 1;
            int west = (level.OriginInTextures.X + (update.West - level.NextExtent.West)) % clipmapSize;
            int south = (level.OriginInTextures.Y + (update.South - level.NextExtent.South)) % clipmapSize;

            _computeNormalsUpdateSize.Value = new Vector2F(update.Width, update.Height);
            _computeNormalsOrigin.Value = new Vector2F(west, south);
            _computeNormalsOneOverHeightMapSize.Value = new Vector2F(1.0f / update.Level.HeightTexture.Description.Width, 1.0f / update.Level.HeightTexture.Description.Height);
            _postDelta.Value = (float)update.Level.Terrain.PostDeltaLongitude;

            // Save the current state of the context
            Rectangle oldViewport = context.Viewport;
            Framebuffer oldFramebuffer = context.Framebuffer;

            // Update the context and draw
            context.Viewport = new Rectangle(0, 0, level.NormalTexture.Description.Width, level.NormalTexture.Description.Height);
            context.Framebuffer = _framebuffer;
            context.Draw(_unitQuadPrimitiveType, _computeNormalsDrawState, _sceneState);

            // Restore the context to its original state
            context.Framebuffer = oldFramebuffer;
            context.Viewport = oldViewport;
        }

        private void RequestTileLoad(RasterDataDetails details,  ClipmapLevel level, RasterTile tile)
        {
            LinkedListNode<TileLoadRequest> requestNode;
            bool exists = details.LoadingTiles.TryGetValue(tile.Identifier, out requestNode);

            if (!exists)
            {
                // Create a new request.
                TileLoadRequest request = new TileLoadRequest();
                request.Level = level;
                request.Tile = tile;
                requestNode = new LinkedListNode<TileLoadRequest>(request);
            }

            lock (details.RequestList)
            {
                if (exists)
                {
                    // Remove the existing request from the queue so we can re-insert it
                    // in its new location.
                    if (requestNode.List == null)
                    {
                        // Request was in the queue at one point, but it's not anymore.
                        // That means it's been loaded,  so we don't need to do anything.
                        return;
                    }
                    details.RequestList.Remove(requestNode);
                }

                if (details.RequestInsertionPoint == null || details.RequestInsertionPoint.List == null)
                {
                    details.RequestList.AddLast(requestNode);
                }
                else
                {
                    details.RequestList.AddBefore(details.RequestInsertionPoint, requestNode);
                }
                details.RequestInsertionPoint = requestNode;

                // If the request list has too many entries, delete from the beginning
                const int MaxRequests = 500;
                while (details.RequestList.Count > MaxRequests)
                {
                    LinkedListNode<TileLoadRequest> nodeToRemove = details.RequestList.First;
                    details.RequestList.RemoveFirst();
                    details.LoadingTiles.Remove(nodeToRemove.Value.Tile.Identifier);
                }

                Monitor.Pulse(details.RequestList);
            }

            details.LoadingTiles[tile.Identifier] = requestNode;
        }

        private void RequestThreadEntryPoint(object state)
        {
            RasterDataDetails details = (RasterDataDetails)state;

            details.WorkerWindow.Context.MakeCurrent();

            while (true)
            {
                TileLoadRequest request = null;
                lock (details.RequestList)
                {
                    LinkedListNode<TileLoadRequest> lastNode = details.RequestList.Last;
                    if (lastNode != null)
                    {
                        request = lastNode.Value;
                        details.RequestList.RemoveLast();
                    }
                    else
                    {
                        Monitor.Wait(details.RequestList);

                        lastNode = details.RequestList.Last;
                        if (lastNode != null)
                        {
                            request = lastNode.Value;
                            details.RequestList.RemoveLast();
                        }
                    }
                }

                if (request != null)
                {
                    RasterTile tile = request.Tile;
                    request.Texture = tile.LoadTexture();

                    Fence fence = Device.CreateFence();
                    fence.ClientWait();

                    details.DoneQueue.Post(request);
                }
            }
        }

        private enum RasterType
        {
            Terrain,
            Imagery
        }

        private class TileLoadRequest
        {
            public ClipmapLevel Level;
            public RasterTile Tile;
            public Texture2D Texture;
        }

        private class RasterDataDetails
        {
            public RasterDataDetails(RasterType type)
            {
                Type = type;
            }

            public RasterType Type;
            public GraphicsWindow WorkerWindow;
            public Dictionary<RasterTileIdentifier, LinkedListNode<TileLoadRequest>> LoadingTiles = new Dictionary<RasterTileIdentifier, LinkedListNode<TileLoadRequest>>();
            public Dictionary<RasterTileIdentifier, Texture2D> LoadedTiles = new Dictionary<RasterTileIdentifier, Texture2D>();
            public LinkedList<TileLoadRequest> RequestList = new LinkedList<TileLoadRequest>();
            public LinkedListNode<TileLoadRequest> RequestInsertionPoint;
            public MessageQueue DoneQueue = new MessageQueue();
        }

        private VertexArray _unitQuad;
        private PrimitiveType _unitQuadPrimitiveType;
        private SceneState _sceneState;
        private Framebuffer _framebuffer;

        private ShaderProgram _updateShader;
        private int _updateTexelOutput;
        private DrawState _updateDrawState;
        private Uniform<Vector2F> _updateDestinationOffset;
        private Uniform<Vector2F> _updateUpdateSize;
        private Uniform<Vector2F> _updateSourceOrigin;

        private ShaderProgram _upsampleShader;
        private int _upsampleTexelOutput;
        private DrawState _upsampleDrawState;
        private Uniform<Vector2F> _upsampleSourceOrigin;
        private Uniform<Vector2F> _upsampleUpdateSize;
        private Uniform<Vector2F> _upsampleDestinationOffset;
        private Uniform<Vector2F> _upsampleOneOverTextureSize;

        private ShaderProgram _computeNormalsShader;
        private int _normalOutput;
        private DrawState _computeNormalsDrawState;
        private Uniform<Vector2F> _computeNormalsOrigin;
        private Uniform<Vector2F> _computeNormalsUpdateSize;
        private Uniform<Vector2F> _computeNormalsOneOverHeightMapSize;
        private Uniform<float> _heightExaggeration;
        private Uniform<float> _postDelta;

        private RasterDataDetails _terrain = new RasterDataDetails(RasterType.Terrain);
        private RasterDataDetails _imagery = new RasterDataDetails(RasterType.Imagery);
    }
}
