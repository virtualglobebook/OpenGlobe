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
            _imageryWorkerWindow = Device.CreateWindow(1, 1);
            context.MakeCurrent();

#if !SingleThreaded
            Thread requestThread = new Thread(RequestThreadEntryPoint);
            requestThread.IsBackground = true;
            requestThread.Start();

            Thread imageryRequestThread = new Thread(ImageryRequestThreadEntryPoint);
            imageryRequestThread.IsBackground = true;
            imageryRequestThread.Start();
#endif

            // Preload the entire world at level 0
            ClipmapLevel levelZero = clipmapLevels[0];
            RasterTileRegion[] regions = levelZero.Terrain.GetTilesInExtent(0, 0, levelZero.Terrain.LongitudePosts - 1, levelZero.Terrain.LatitudePosts - 1);
            foreach (RasterTileRegion region in regions)
            {
                RequestTileLoad(levelZero, region.Tile);
            }

            RasterTileRegion[] imageryRegions = levelZero.Imagery.GetTilesInExtent(0, 0, levelZero.Imagery.LongitudePosts - 1, levelZero.Imagery.LatitudePosts - 1);
            foreach (RasterTileRegion region in imageryRegions)
            {
                RequestImageryTileLoad(levelZero, region.Tile);
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
            _imageryRequestInsertionPoint = null;

#if SingleThreaded
            _requestQueue.ProcessQueue();
#endif
            // TODO: it would be nice if the MessageQueue gave us a way to do this directly without anonymous delegate trickery.
            List<TileLoadRequest> tiles = new List<TileLoadRequest>();
            List<ImageryTileLoadRequest> imageryTiles = new List<ImageryTileLoadRequest>();
            EventHandler<MessageQueueEventArgs> handler = delegate(object sender, MessageQueueEventArgs e)
            {
                TileLoadRequest tile = e.Message as TileLoadRequest;
                if (tile != null)
                {
                    _loadedTiles[tile.Tile.Identifier] = tile.Texture;
                    _loadingTiles.Remove(tile.Tile.Identifier);
                    tiles.Add(tile);
                }

                ImageryTileLoadRequest imageryTile = e.Message as ImageryTileLoadRequest;
                if (imageryTile != null)
                {
                    _loadedImageryTiles[imageryTile.Tile.Identifier] = imageryTile.Texture;
                    _loadingImageryTiles.Remove(imageryTile.Tile.Identifier);
                    imageryTiles.Add(imageryTile);
                }
            };
            _doneQueue.MessageReceived += handler;
            _doneQueue.ProcessQueue();
            _doneQueue.MessageReceived -= handler;

            foreach (TileLoadRequest tile in tiles)
            {
                ApplyNewTile(context, tile.Level, tile.Tile);
            }

            foreach (ImageryTileLoadRequest tile in imageryTiles)
            {
                ApplyNewImageryTile(context, tile.Level, tile.Tile);
            }

            //if (tiles.Count > 0)
            //    foreach (ClipmapLevel level in levels)
            //    {
            //        VerifyHeights(level);
            //    }
        }

        public void ApplyNewTile(Context context, ClipmapLevel level, RasterTile tile)
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

        private void ApplyIfNotLoaded(Context context, ClipmapLevel level, RasterTile tile)
        {
            Texture2D texture;
            if (!_loadedTiles.TryGetValue(tile.Identifier, out texture) || texture == null)
            {
                ApplyNewTile(context, level, tile);
            }
        }

        public void ApplyNewImageryTile(Context context, ClipmapLevel level, RasterTile tile)
        {
            ClipmapUpdate entireLevel = new ClipmapUpdate(
                level,
                level.NextImageryExtent.West,
                level.NextImageryExtent.South,
                level.NextImageryExtent.East,
                level.NextImageryExtent.North);

            ClipmapUpdate thisTile = new ClipmapUpdate(
                level,
                tile.West - 1,
                tile.South - 1,
                tile.East + 1,
                tile.North + 1);

            ClipmapUpdate intersection = IntersectUpdates(entireLevel, thisTile);

            if (intersection.Width > 0 && intersection.Height > 0)
            {
                UpdateImagery(context, intersection);

                // Recurse on child tiles if they're NOT loaded.  Unloaded children will use data from this tile.
                ClipmapLevel finer = level.FinerLevel;
                if (finer != null)
                {
                    ApplyImageryIfNotLoaded(context, finer, tile.SouthwestChild);
                    ApplyImageryIfNotLoaded(context, finer, tile.SoutheastChild);
                    ApplyImageryIfNotLoaded(context, finer, tile.NorthwestChild);
                    ApplyImageryIfNotLoaded(context, finer, tile.NortheastChild);
                }
            }
        }

        private void ApplyImageryIfNotLoaded(Context context, ClipmapLevel level, RasterTile tile)
        {
            Texture2D texture;
            if (!_loadedImageryTiles.TryGetValue(tile.Identifier, out texture) || texture == null)
            {
                ApplyNewImageryTile(context, level, tile);
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
            RasterTileRegion[] tileRegions = level.Terrain.GetTilesInExtent(level.NextExtent.West, level.NextExtent.South, level.NextExtent.East, level.NextExtent.North);
            foreach (RasterTileRegion region in tileRegions)
            {
                if (!_loadedTiles.ContainsKey(region.Tile.Identifier))
                {
                    RequestTileLoad(level, region.Tile);
                }
            }

            RasterTileRegion[] imageryTileRegions = level.Imagery.GetTilesInExtent(level.NextImageryExtent.West, level.NextImageryExtent.South, level.NextImageryExtent.East, level.NextImageryExtent.North);
            foreach (RasterTileRegion region in imageryTileRegions)
            {
                if (!_loadedImageryTiles.ContainsKey(region.Tile.Identifier))
                {
                    RequestImageryTileLoad(level, region.Tile);
                }
            }
        }

        public void Update(Context context, ClipmapUpdate update)
        {
            ClipmapLevel level = update.Level;

            ClipmapUpdate[] updates = SplitUpdateToAvoidWrappingTerrain(update);
            foreach (ClipmapUpdate nonWrappingUpdate in updates)
            {
                RasterTileRegion[] tileRegions = level.Terrain.GetTilesInExtent(nonWrappingUpdate.West, nonWrappingUpdate.South, nonWrappingUpdate.East, nonWrappingUpdate.North);
                foreach (RasterTileRegion region in tileRegions)
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
            ClipmapUpdate[] normalUpdates = SplitUpdateToAvoidWrappingTerrain(updateWithBuffer);
            foreach (ClipmapUpdate normalUpdate in normalUpdates)
            {
                UpdateNormals(context, normalUpdate);
            }
        }

        public void UpdateImagery(Context context, ClipmapUpdate update)
        {
            ClipmapLevel level = update.Level;

            ClipmapUpdate[] updates = SplitUpdateToAvoidWrappingImagery(update);
            foreach (ClipmapUpdate nonWrappingUpdate in updates)
            {
                RasterTileRegion[] tileRegions = level.Imagery.GetTilesInExtent(nonWrappingUpdate.West, nonWrappingUpdate.South, nonWrappingUpdate.East, nonWrappingUpdate.North);
                foreach (RasterTileRegion region in tileRegions)
                {
                    Texture2D tileTexture;
                    bool loaded = _loadedImageryTiles.TryGetValue(region.Tile.Identifier, out tileTexture);
                    if (loaded)
                    {
                        RenderImageryTileToLevelTexture(context, level, region, tileTexture);
                    }
                    else
                    {
                        UpsampleImageryTileData(context, level, region);
                    }
                }
            }
        }

        private ClipmapUpdate[] SplitUpdateToAvoidWrappingTerrain(ClipmapUpdate update)
        {
            ClipmapLevel level = update.Level;
            return SplitUpdateToAvoidWrapping(update, level.OriginInTextures, level.NextExtent);
        }

        private ClipmapUpdate[] SplitUpdateToAvoidWrappingImagery(ClipmapUpdate update)
        {
            ClipmapLevel level = update.Level;
            return SplitUpdateToAvoidWrapping(update, level.OriginInImagery, level.NextImageryExtent);
        }

        private ClipmapUpdate[] SplitUpdateToAvoidWrapping(ClipmapUpdate update, Vector2I origin, ClipmapLevel.Extent extent)
        {
            ClipmapLevel level = update.Level;
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

        private void RenderImageryTileToLevelTexture(Context context, ClipmapLevel level, RasterTileRegion region, Texture2D texture)
        {
            context.TextureUnits[0].Texture = texture;
            context.TextureUnits[0].TextureSampler = Device.TextureSamplers.NearestClamp;

            _framebuffer.ColorAttachments[_updateHeightOutput] = level.ImageryTexture;

            int clipmapSize = level.NextImageryExtent.East - level.NextImageryExtent.West + 1;
            int destWest = (level.OriginInImagery.X + (region.Tile.West + region.West - level.NextImageryExtent.West)) % clipmapSize;
            int destSouth = (level.OriginInImagery.Y + (region.Tile.South + region.South - level.NextImageryExtent.South)) % clipmapSize;

            int width = region.East - region.West + 1;
            int height = region.North - region.South + 1;

            _updateSourceOrigin.Value = new Vector2F(region.West, region.South);
            _updateUpdateSize.Value = new Vector2F(width, height);
            _updateDestinationOffset.Value = new Vector2F(destWest, destSouth);

            // Save the current state of the context
            Rectangle oldViewport = context.Viewport;
            Framebuffer oldFramebuffer = context.Framebuffer;

            // Update the context and draw
            context.Viewport = new Rectangle(0, 0, level.ImageryTexture.Description.Width, level.ImageryTexture.Description.Height);
            context.Framebuffer = _framebuffer;
            context.Draw(_unitQuadPrimitiveType, _updateDrawState, _sceneState);

            // Restore the context to its original state
            context.Framebuffer = oldFramebuffer;
            context.Viewport = oldViewport;
        }

        private void RenderTileToLevelHeightTexture(Context context, ClipmapLevel level, RasterTileRegion region, Texture2D texture)
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

        private void UpsampleTileData(Context context, ClipmapLevel level, RasterTileRegion region)
        {
            ClipmapLevel coarserLevel = level.CoarserLevel;

            if (coarserLevel == null)
                return;

            context.TextureUnits[0].Texture = coarserLevel.HeightTexture;
            context.TextureUnits[0].TextureSampler = Device.TextureSamplers.LinearRepeat;

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

        private void UpsampleImageryTileData(Context context, ClipmapLevel level, RasterTileRegion region)
        {
            ClipmapLevel coarserLevel = level.CoarserLevel;

            if (coarserLevel == null)
                return;

            context.TextureUnits[0].Texture = coarserLevel.ImageryTexture;
            context.TextureUnits[0].TextureSampler = Device.TextureSamplers.LinearRepeat;

            _framebuffer.ColorAttachments[_upsampleHeightOutput] = level.ImageryTexture;

            int fineClipmapSize = level.NextImageryExtent.East - level.NextImageryExtent.West + 1;
            int destWest = (level.OriginInImagery.X + (region.Tile.West + region.West - level.NextImageryExtent.West)) % fineClipmapSize;
            int destSouth = (level.OriginInImagery.Y + (region.Tile.South + region.South - level.NextImageryExtent.South)) % fineClipmapSize;

            int coarseClipmapSize = coarserLevel.NextImageryExtent.East - coarserLevel.NextImageryExtent.West + 1;
            double sourceWest = (coarserLevel.OriginInImagery.X + ((region.Tile.West + region.West) / 2.0 - coarserLevel.NextImageryExtent.West)) % coarseClipmapSize;
            double sourceSouth = (coarserLevel.OriginInImagery.Y + ((region.Tile.South + region.South) / 2.0 - coarserLevel.NextImageryExtent.South)) % coarseClipmapSize;

            int width = region.East - region.West + 1;
            int height = region.North - region.South + 1;

            _upsampleSourceOrigin.Value = new Vector2F((float)sourceWest, (float)sourceSouth);
            _upsampleUpdateSize.Value = new Vector2F(width, height);
            _upsampleDestinationOffset.Value = new Vector2F(destWest, destSouth);
            _upsampleOneOverHeightMapSize.Value = new Vector2F(1.0f / coarserLevel.ImageryTexture.Description.Width, 1.0f / coarserLevel.ImageryTexture.Description.Height);

            // Save the current state of the context
            Rectangle oldViewport = context.Viewport;
            Framebuffer oldFramebuffer = context.Framebuffer;

            // Update the context and draw
            context.Viewport = new Rectangle(0, 0, level.ImageryTexture.Description.Width, level.ImageryTexture.Description.Height);
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

        private void RequestTileLoad(ClipmapLevel level, RasterTile tile)
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

        private void RequestImageryTileLoad(ClipmapLevel level, RasterTile tile)
        {
            LinkedListNode<ImageryTileLoadRequest> requestNode;
            bool exists = _loadingImageryTiles.TryGetValue(tile.Identifier, out requestNode);

            if (!exists)
            {
                // Create a new request.
                ImageryTileLoadRequest request = new ImageryTileLoadRequest();
                request.Level = level;
                request.Tile = tile;
                requestNode = new LinkedListNode<ImageryTileLoadRequest>(request);
            }

            lock (_imageryRequestList)
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
                    _imageryRequestList.Remove(requestNode);
                }

                if (_imageryRequestInsertionPoint == null || _imageryRequestInsertionPoint.List == null)
                {
                    _imageryRequestList.AddLast(requestNode);
                }
                else
                {
                    _imageryRequestList.AddBefore(_imageryRequestInsertionPoint, requestNode);
                }
                _imageryRequestInsertionPoint = requestNode;

                // If the request list has too many entries, delete from the beginning
                const int MaxRequests = 500;
                while (_imageryRequestList.Count > MaxRequests)
                {
                    LinkedListNode<ImageryTileLoadRequest> nodeToRemove = _imageryRequestList.First;
                    _imageryRequestList.RemoveFirst();
                    _loadingImageryTiles.Remove(nodeToRemove.Value.Tile.Identifier);
                }

                Monitor.Pulse(_imageryRequestList);
            }

            _loadingImageryTiles[tile.Identifier] = requestNode;
        }

        private Texture2D CreateImageryTextureFromTile(RasterTile tile)
        {
            return tile.LoadTexture();
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
                    RasterTile tile = request.Tile;
                    request.Texture = tile.LoadTexture();

                    Fence fence = Device.CreateFence();
                    fence.ClientWait();

                    _doneQueue.Post(request);
                }
            }
        }

        private void ImageryRequestThreadEntryPoint()
        {
            _imageryWorkerWindow.Context.MakeCurrent();

            while (true)
            {
                ImageryTileLoadRequest request = null;
                lock (_imageryRequestList)
                {
                    LinkedListNode<ImageryTileLoadRequest> lastNode = _imageryRequestList.Last;
                    if (lastNode != null)
                    {
                        request = lastNode.Value;
                        _imageryRequestList.RemoveLast();
                    }
                    else
                    {
                        Monitor.Wait(_imageryRequestList);

                        lastNode = _imageryRequestList.Last;
                        if (lastNode != null)
                        {
                            request = lastNode.Value;
                            _imageryRequestList.RemoveLast();
                        }
                    }
                }

                if (request != null)
                {
                    RasterTile tile = request.Tile;
                    request.Texture = CreateImageryTextureFromTile(tile);

                    Fence fence = Device.CreateFence();
                    fence.ClientWait();

                    _doneQueue.Post(request);
                }
            }
        }

        private class TileLoadRequest
        {
            public ClipmapLevel Level;
            public RasterTile Tile;
            public Texture2D Texture;
        }

        private class ImageryTileLoadRequest
        {
            public ClipmapLevel Level;
            public RasterTile Tile;
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

        private Dictionary<RasterTileIdentifier, LinkedListNode<TileLoadRequest>> _loadingTiles = new Dictionary<RasterTileIdentifier, LinkedListNode<TileLoadRequest>>();
        private Dictionary<RasterTileIdentifier, Texture2D> _loadedTiles = new Dictionary<RasterTileIdentifier, Texture2D>();

        private Dictionary<RasterTileIdentifier, LinkedListNode<ImageryTileLoadRequest>> _loadingImageryTiles = new Dictionary<RasterTileIdentifier, LinkedListNode<ImageryTileLoadRequest>>();
        private Dictionary<RasterTileIdentifier, Texture2D> _loadedImageryTiles = new Dictionary<RasterTileIdentifier, Texture2D>();

        private GraphicsWindow _workerWindow;
        private GraphicsWindow _imageryWorkerWindow;
        private MessageQueue _doneQueue = new MessageQueue();

        private LinkedList<TileLoadRequest> _requestList = new LinkedList<TileLoadRequest>();
        private LinkedListNode<TileLoadRequest> _requestInsertionPoint;

        private LinkedList<ImageryTileLoadRequest> _imageryRequestList = new LinkedList<ImageryTileLoadRequest>();
        private LinkedListNode<ImageryTileLoadRequest> _imageryRequestInsertionPoint;

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
