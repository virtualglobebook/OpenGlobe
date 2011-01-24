//#define SingleThreaded
using System;
using System.Drawing;
using OpenGlobe.Core;
using OpenGlobe.Renderer;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

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
            _updateHeightOutput = _updateShader.FragmentOutputs["heightOutput"];
            _updateDrawState = new DrawState(new RenderState(), _updateShader, _unitQuad);
            _updateDrawState.RenderState.FacetCulling.FrontFaceWindingOrder = unitQuad.FrontFaceWindingOrder;
            _updateDrawState.RenderState.DepthTest.Enabled = false;
            ((Uniform<int>)_updateShader.Uniforms["u_texture"]).Value = 0;
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
            _upsampleSourceOrigin = (Uniform<Vector2F>)_upsampleShader.Uniforms["u_sourceOrigin"];
            _upsampleUpdateSize = (Uniform<Vector2F>)_upsampleShader.Uniforms["u_updateSize"];
            _upsampleDestinationOffset = (Uniform<Vector2F>)_upsampleShader.Uniforms["u_destinationOffset"];
            _upsampleOneOverHeightMapSize = (Uniform<Vector2F>)_upsampleShader.Uniforms["u_oneOverHeightMapSize"];

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
            context.MakeCurrent();

#if !SingleThreaded
            Thread requestThread = new Thread(RequestThreadEntryPoint);
            requestThread.IsBackground = true;
            requestThread.Start();
#endif

            // Preload the entire world at level 0
            ClipmapLevel levelZero = clipmapLevels[0];
            RasterTerrainTileRegion[] regions = levelZero.Terrain.GetTilesInExtent(0, 0, levelZero.Terrain.LongitudePosts - 1, levelZero.Terrain.LatitudePosts - 1);
            foreach (RasterTerrainTileRegion region in regions)
            {
                RequestTileLoad(levelZero, region.Tile);
            }
        }

        public void Dispose()
        {
            _doneQueue.Dispose();

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
            // This is the start of a new frame, so the next request goes at the end
            _requestInsertionPoint = null;

#if SingleThreaded
            _requestQueue.ProcessQueue();
#endif
            // TODO: it would be nice if the MessageQueue gave us a way to do this directly without anonymous delegate trickery.
            List<TileLoadRequest> tiles = new List<TileLoadRequest>();
            EventHandler<MessageQueueEventArgs> handler = delegate(object sender, MessageQueueEventArgs e)
            {
                TileLoadRequest tile = (TileLoadRequest)e.Message;
                _loadedTiles[tile.Tile.Identifier] = tile.Texture;
                _loadingTiles.Remove(tile.Tile.Identifier);
                tiles.Add(tile);
            };
            _doneQueue.MessageReceived += handler;
            _doneQueue.ProcessQueue();
            _doneQueue.MessageReceived -= handler;

            foreach (TileLoadRequest tile in tiles)
            {
                ApplyNewTile(context, tile.Level, tile.Tile);
            }

            //if (tiles.Count > 0)
            //    foreach (ClipmapLevel level in levels)
            //    {
            //        VerifyHeights(level);
            //    }
        }

        public void ApplyNewTile(Context context, ClipmapLevel level, RasterTerrainTile tile)
        {
            ClipmapUpdate entireLevel = new ClipmapUpdate(
                level,
                level.NextExtent.West,
                level.NextExtent.South,
                level.NextExtent.East,
                level.NextExtent.North);

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
                ClipmapLevel finer = level.FinerLevel;
                if (finer != null)
                {
                    ApplyIfNotLoaded(context, finer, tile.SouthwestChild);
                    ApplyIfNotLoaded(context, finer, tile.SoutheastChild);
                    ApplyIfNotLoaded(context, finer, tile.NorthwestChild);
                    ApplyIfNotLoaded(context, finer, tile.NortheastChild);
                }
            }
        }

        private void ApplyIfNotLoaded(Context context, ClipmapLevel level, RasterTerrainTile tile)
        {
            Texture2D texture;
            if (!_loadedTiles.TryGetValue(tile.Identifier, out texture) || texture == null)
            {
                ApplyNewTile(context, level, tile);
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
            RasterTerrainTileRegion[] tileRegions = level.Terrain.GetTilesInExtent(level.NextExtent.West, level.NextExtent.South, level.NextExtent.East, level.NextExtent.North);
            foreach (RasterTerrainTileRegion region in tileRegions)
            {
                if (!_loadedTiles.ContainsKey(region.Tile.Identifier))
                {
                    RequestTileLoad(level, region.Tile);
                }
            }
        }

        public void Update(Context context, ClipmapUpdate update)
        {
            ClipmapLevel level = update.Level;

            ClipmapUpdate[] updates = SplitUpdateToAvoidWrapping(update);
            foreach (ClipmapUpdate nonWrappingUpdate in updates)
            {
                RasterTerrainTileRegion[] tileRegions = level.Terrain.GetTilesInExtent(nonWrappingUpdate.West, nonWrappingUpdate.South, nonWrappingUpdate.East, nonWrappingUpdate.North);
                foreach (RasterTerrainTileRegion region in tileRegions)
                {
                    Texture2D tileTexture;
                    bool loaded = _loadedTiles.TryGetValue(region.Tile.Identifier, out tileTexture);
                    if (loaded)
                    {
                        RenderTileToLevelHeightTexture(context, level, region, tileTexture);
                    }
                    else
                    {
                        UpsampleTileData(context, level, region);
                    }
                }
            }

            // Normals at edges are incorrect, so include a one-post buffer around the update region
            // when updating normals in order to update normals that were previously at the edge.
            ClipmapUpdate updateWithBuffer = update.AddBufferWithinLevelNextExtent();
            ClipmapUpdate[] normalUpdates = SplitUpdateToAvoidWrapping(updateWithBuffer);
            foreach (ClipmapUpdate normalUpdate in normalUpdates)
            {
                UpdateNormals(context, normalUpdate);
            }
        }

        private ClipmapUpdate[] SplitUpdateToAvoidWrapping(ClipmapUpdate update)
        {
            ClipmapLevel level = update.Level;
            int clipmapSize = level.NextExtent.East - level.NextExtent.West + 1;

            int west = (level.OriginInTextures.X + (update.West - level.NextExtent.West)) % clipmapSize;
            int east = (level.OriginInTextures.X + (update.East - level.NextExtent.West)) % clipmapSize;
            int south = (level.OriginInTextures.Y + (update.South - level.NextExtent.South)) % clipmapSize;
            int north = (level.OriginInTextures.Y + (update.North - level.NextExtent.South)) % clipmapSize;

            if (east < west && north < south)
            {
                // Horizontal AND vertical wrap
                ClipmapUpdate bottomLeftUpdate = new ClipmapUpdate(
                    level,
                    update.West,
                    update.South,
                    level.NextExtent.West + (clipmapSize - level.OriginInTextures.X - 1),
                    level.NextExtent.South + (clipmapSize - level.OriginInTextures.Y - 1));

                ClipmapUpdate bottomRightUpdate = new ClipmapUpdate(
                    level,
                    level.NextExtent.West + clipmapSize - level.OriginInTextures.X,
                    update.South,
                    update.East,
                    level.NextExtent.South + (clipmapSize - level.OriginInTextures.Y - 1));

                ClipmapUpdate topLeftUpdate = new ClipmapUpdate(
                    level,
                    update.West,
                    level.NextExtent.South + clipmapSize - level.OriginInTextures.Y,
                    level.NextExtent.West + (clipmapSize - level.OriginInTextures.X - 1),
                    update.North);

                ClipmapUpdate topRightUpdate = new ClipmapUpdate(
                    level,
                    level.NextExtent.West + clipmapSize - level.OriginInTextures.X,
                    level.NextExtent.South + clipmapSize - level.OriginInTextures.Y,
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
                    level.NextExtent.West + (clipmapSize - level.OriginInTextures.X - 1),
                    update.North);

                ClipmapUpdate rightUpdate = new ClipmapUpdate(
                    level,
                    level.NextExtent.West + clipmapSize - level.OriginInTextures.X,
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
                    level.NextExtent.South + (clipmapSize - level.OriginInTextures.Y - 1));

                ClipmapUpdate topUpdate = new ClipmapUpdate(
                    level,
                    update.West,
                    level.NextExtent.South + clipmapSize - level.OriginInTextures.Y,
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

        private void RenderTileToLevelHeightTexture(Context context, ClipmapLevel level, RasterTerrainTileRegion region, Texture2D texture)
        {
            context.TextureUnits[0].Texture = texture;
            context.TextureUnits[0].TextureSampler = Device.TextureSamplers.NearestClamp;

            _framebuffer.ColorAttachments[_updateHeightOutput] = level.HeightTexture;

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
            Framebuffer oldFramebuffer = context.Framebuffer;

            // Update the context and draw
            context.Viewport = new Rectangle(0, 0, level.HeightTexture.Description.Width, level.HeightTexture.Description.Height);
            context.Framebuffer = _framebuffer;
            context.Draw(_unitQuadPrimitiveType, _updateDrawState, _sceneState);

            // Restore the context to its original state
            context.Framebuffer = oldFramebuffer;
            context.Viewport = oldViewport;
        }

        private void UpsampleTileData(Context context, ClipmapLevel level, RasterTerrainTileRegion region)
        {
            ClipmapLevel coarserLevel = level.CoarserLevel;

            if (coarserLevel == null)
                return;

            context.TextureUnits[0].Texture = coarserLevel.HeightTexture;
            context.TextureUnits[0].TextureSampler = Device.TextureSamplers.LinearRepeat; // TODO: change to NearestRepeat

            _framebuffer.ColorAttachments[_upsampleHeightOutput] = level.HeightTexture;

            int fineClipmapSize = level.NextExtent.East - level.NextExtent.West + 1;
            int destWest = (level.OriginInTextures.X + (region.Tile.West + region.West - level.NextExtent.West)) % fineClipmapSize;
            int destSouth = (level.OriginInTextures.Y + (region.Tile.South + region.South - level.NextExtent.South)) % fineClipmapSize;

            int coarseClipmapSize = coarserLevel.NextExtent.East - coarserLevel.NextExtent.West + 1;
            double sourceWest = (coarserLevel.OriginInTextures.X + ((region.Tile.West + region.West) / 2.0 - coarserLevel.NextExtent.West)) % coarseClipmapSize;
            double sourceSouth = (coarserLevel.OriginInTextures.Y + ((region.Tile.South + region.South) / 2.0 - coarserLevel.NextExtent.South)) % coarseClipmapSize;

            int width = region.East - region.West + 1;
            int height = region.North - region.South + 1;

            _upsampleSourceOrigin.Value = new Vector2F((float)sourceWest, (float)sourceSouth);
            _upsampleUpdateSize.Value = new Vector2F(width, height);
            _upsampleDestinationOffset.Value = new Vector2F(destWest, destSouth);
            _upsampleOneOverHeightMapSize.Value = new Vector2F(1.0f / coarserLevel.HeightTexture.Description.Width, 1.0f / coarserLevel.HeightTexture.Description.Height);

            // Save the current state of the context
            Rectangle oldViewport = context.Viewport;
            Framebuffer oldFramebuffer = context.Framebuffer;

            // Update the context and draw
            context.Viewport = new Rectangle(0, 0, level.HeightTexture.Description.Width, level.HeightTexture.Description.Height);
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

        private void RequestTileLoad(ClipmapLevel level, RasterTerrainTile tile)
        {
            LinkedListNode<TileLoadRequest> requestNode;
            bool exists = _loadingTiles.TryGetValue(tile.Identifier, out requestNode);

            if (!exists)
            {
                // Create a new request.
                TileLoadRequest request = new TileLoadRequest();
                request.Level = level;
                request.Tile = tile;
                requestNode = new LinkedListNode<TileLoadRequest>(request);
            }

            lock (_requestList)
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
                    _requestList.Remove(requestNode);
                }

                if (_requestInsertionPoint == null || _requestInsertionPoint.List == null)
                {
                    _requestList.AddLast(requestNode);
                }
                else
                {
                    _requestList.AddBefore(_requestInsertionPoint, requestNode);
                }
                _requestInsertionPoint = requestNode;

                // If the request list has too many entries, delete from the beginning
                const int MaxRequests = 500;
                while (_requestList.Count > MaxRequests)
                {
                    LinkedListNode<TileLoadRequest> nodeToRemove = _requestList.First;
                    _requestList.RemoveFirst();
                    _loadingTiles.Remove(nodeToRemove.Value.Tile.Identifier);
                }

                Monitor.Pulse(_requestList);
            }

            _loadingTiles[tile.Identifier] = requestNode;
        }

        private Texture2D CreateTextureFromTile(RasterTerrainTile tile)
        {
            int width = tile.East - tile.West + 1;
            int height = tile.North - tile.South + 1;

            Texture2DDescription description = new Texture2DDescription(width, height, TextureFormat.Red32f, false);
            Texture2D texture = Device.CreateTexture2DRectangle(description);

            float[] posts = new float[width * height];
            tile.GetPosts(0, 0, width - 1, height - 1, posts, 0, width);

            using (WritePixelBuffer wpb = Device.CreateWritePixelBuffer(PixelBufferHint.Stream, width * height * sizeof(float)))
            {
                wpb.CopyFromSystemMemory(posts);
                texture.CopyFromBuffer(wpb, ImageFormat.Red, ImageDatatype.Float);
            }

            return texture;
        }

        private List<TileLoadRequest> _currentRequests = new List<TileLoadRequest>();

        /// <summary>
        /// Invoked in the <see cref="_requestQueue"/> thread when a tile load request is received.
        /// </summary>
        private void TileLoadRequestReceived(object sender, MessageQueueEventArgs e)
        {
            TileLoadRequest request = (TileLoadRequest)e.Message;
            _currentRequests.Add(request);
        }

        private void RequestThreadEntryPoint()
        {
            _workerWindow.Context.MakeCurrent();

            while (true)
            {
                TileLoadRequest request = null;
                lock (_requestList)
                {
                    LinkedListNode<TileLoadRequest> lastNode = _requestList.Last;
                    if (lastNode != null)
                    {
                        request = lastNode.Value;
                        _requestList.RemoveLast();
                    }
                    else
                    {
                        Monitor.Wait(_requestList);

                        lastNode = _requestList.Last;
                        if (lastNode != null)
                        {
                            request = lastNode.Value;
                            _requestList.RemoveLast();
                        }
                    }
                }

                if (request != null)
                {
                    RasterTerrainTile tile = request.Tile;
                    request.Texture = CreateTextureFromTile(tile);

                    Fence fence = Device.CreateFence();
                    fence.ClientWait();

                    _doneQueue.Post(request);
                }
            }
        }

        public void VerifyHeights(ClipmapLevel level)
        {
            ReadPixelBuffer rpb = level.HeightTexture.CopyToBuffer(ImageFormat.Red, ImageDatatype.Float);
            float[] postsFromTexture = rpb.CopyToSystemMemory<float>();

            ReadPixelBuffer rpbNormals = level.NormalTexture.CopyToBuffer(ImageFormat.RedGreenBlue, ImageDatatype.Float);
            Vector3F[] normalsFromTexture = rpbNormals.CopyToSystemMemory<Vector3F>();

            int clipmapPosts = level.NextExtent.East - level.NextExtent.West + 1;

            float[] realPosts = new float[clipmapPosts * clipmapPosts];
            level.Terrain.GetPosts(level.NextExtent.West, level.NextExtent.South, level.NextExtent.East, level.NextExtent.North, realPosts, 0, clipmapPosts);

            float heightExaggeration = HeightExaggeration;
            float postDelta = (float)level.Terrain.PostDeltaLongitude;

            for (int j = 0; j < clipmapPosts; ++j)
            {
                int y = (j + level.OriginInTextures.Y) % clipmapPosts;
                for (int i = 0; i < clipmapPosts; ++i)
                {
                    int x = (i + level.OriginInTextures.X) % clipmapPosts;

                    float realHeight = realPosts[j * clipmapPosts + i];
                    float heightFromTexture = postsFromTexture[y * clipmapPosts + x];

                    if (realHeight != heightFromTexture)
                        throw new Exception("bad");

                    if (i != 0 && i != clipmapPosts - 1 &&
                        j != 0 && j != clipmapPosts - 1)
                    {
                        int top = (j + 1) * clipmapPosts + i;
                        float topHeight = realPosts[top] * heightExaggeration;
                        int bottom = (j - 1) * clipmapPosts + i;
                        float bottomHeight = realPosts[bottom] * heightExaggeration;
                        int right = j * clipmapPosts + i + 1;
                        float rightHeight = realPosts[right] * heightExaggeration;
                        int left = j * clipmapPosts + i - 1;
                        float leftHeight = realPosts[left] * heightExaggeration;

                        Vector3F realNormal = new Vector3F(leftHeight - rightHeight, bottomHeight - topHeight, 2.0f * postDelta).Normalize();

                        Vector3F normalFromTexture = normalsFromTexture[y * clipmapPosts + x].Normalize();

                        if (!realNormal.EqualsEpsilon(normalFromTexture, 1e-5f))
                            throw new Exception("normal is bad.");
                    }
                }
            }
        }

        private class TileLoadRequest
        {
            public ClipmapLevel Level;
            public RasterTerrainTile Tile;
            public Texture2D Texture;
        }

        private VertexArray _unitQuad;
        private PrimitiveType _unitQuadPrimitiveType;
        private SceneState _sceneState;
        private Framebuffer _framebuffer;

        private ShaderProgram _updateShader;
        private int _updateHeightOutput;
        private DrawState _updateDrawState;
        private Uniform<Vector2F> _updateDestinationOffset;
        private Uniform<Vector2F> _updateUpdateSize;
        private Uniform<Vector2F> _updateSourceOrigin;

        private ShaderProgram _upsampleShader;
        private int _upsampleHeightOutput;
        private DrawState _upsampleDrawState;
        private Uniform<Vector2F> _upsampleSourceOrigin;
        private Uniform<Vector2F> _upsampleUpdateSize;
        private Uniform<Vector2F> _upsampleDestinationOffset;
        private Uniform<Vector2F> _upsampleOneOverHeightMapSize;

        private ShaderProgram _computeNormalsShader;
        private int _normalOutput;
        private DrawState _computeNormalsDrawState;
        private Uniform<Vector2F> _computeNormalsOrigin;
        private Uniform<Vector2F> _computeNormalsUpdateSize;
        private Uniform<Vector2F> _computeNormalsOneOverHeightMapSize;
        private Uniform<float> _heightExaggeration;
        private Uniform<float> _postDelta;

        private Dictionary<RasterTerrainTileIdentifier, LinkedListNode<TileLoadRequest>> _loadingTiles = new Dictionary<RasterTerrainTileIdentifier, LinkedListNode<TileLoadRequest>>();
        private Dictionary<RasterTerrainTileIdentifier, Texture2D> _loadedTiles = new Dictionary<RasterTerrainTileIdentifier, Texture2D>();

        private GraphicsWindow _workerWindow;
        private MessageQueue _doneQueue = new MessageQueue();

        private LinkedList<TileLoadRequest> _requestList = new LinkedList<TileLoadRequest>();
        private LinkedListNode<TileLoadRequest> _requestInsertionPoint;

        private class LastViewerPosition
        {
            public LastViewerPosition(double longitude, double latitude)
            {
                _longitude = longitude;
                _latitude = latitude;
            }

            public double Longitude
            {
                get { return _longitude; }
            }

            public double Latitude
            {
                get { return _latitude; }
            }

            private double _longitude;
            private double _latitude;
        }
    }
}
