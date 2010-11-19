#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using OpenGlobe.Core;
using OpenGlobe.Renderer;
using OpenGlobe.Terrain;

namespace OpenGlobe.Scene.Terrain
{
    public class PlaneClipmapTerrain : IRenderable, IDisposable
    {
        public PlaneClipmapTerrain(Context context, RasterTerrainSource terrainSource, int clipmapPosts)
        {
            _terrainSource = terrainSource;
            _clipmapPosts = clipmapPosts;
            _clipmapSegments = _clipmapPosts - 1;

            int clipmapLevels = _terrainSource.Levels.Count;
            _clipmapLevels = new ClipmapLevel[clipmapLevels];

            for (int i = 0; i < _clipmapLevels.Length; ++i)
            {
                RasterTerrainLevel terrainLevel = _terrainSource.Levels[i];
                _clipmapLevels[i] = new ClipmapLevel();
                _clipmapLevels[i].Terrain = terrainLevel;
                _clipmapLevels[i].HeightTexture = Device.CreateTexture2D(new Texture2DDescription(_clipmapPosts, _clipmapPosts, TextureFormat.Red32f));
                _clipmapLevels[i].NormalTexture = Device.CreateTexture2D(new Texture2DDescription(_clipmapPosts, _clipmapPosts, TextureFormat.RedGreenBlue32f));
            }

            _shaderProgram = Device.CreateShaderProgram(
                EmbeddedResources.GetText("OpenGlobe.Scene.Terrain.ClipmapTerrain.PlaneClipmapVS.glsl"),
                EmbeddedResources.GetText("OpenGlobe.Scene.Terrain.ClipmapTerrain.PlaneClipmapFS.glsl"));

            _fieldBlockPosts = (clipmapPosts + 1) / 4; // M
            _fieldBlockSegments = _fieldBlockPosts - 1;

            // Create the MxM block used to fill the ring and the field.
            Mesh fieldBlockMesh = RectangleTessellator.Compute(
                new RectangleD(new Vector2D(0.0, 0.0), new Vector2D(_fieldBlockSegments, _fieldBlockSegments)),
                _fieldBlockSegments, _fieldBlockSegments);
            _fieldBlock = context.CreateVertexArray(fieldBlockMesh, _shaderProgram.VertexAttributes, BufferHint.StaticDraw);

            // Create the Mx3 block used to fill the space between the MxM blocks in the ring
            Mesh ringFixupHorizontalMesh = RectangleTessellator.Compute(
                new RectangleD(new Vector2D(0.0, 0.0), new Vector2D(_fieldBlockSegments, 2.0)),
                _fieldBlockSegments, 2);
            _ringFixupHorizontal = context.CreateVertexArray(ringFixupHorizontalMesh, _shaderProgram.VertexAttributes, BufferHint.StaticDraw);

            // Create the 3xM block used to fill the space between the MxM blocks in the ring
            Mesh ringFixupVerticalMesh = RectangleTessellator.Compute(
                new RectangleD(new Vector2D(0.0, 0.0), new Vector2D(2.0, _fieldBlockSegments)),
                2, _fieldBlockSegments);
            _ringFixupVertical = context.CreateVertexArray(ringFixupVerticalMesh, _shaderProgram.VertexAttributes, BufferHint.StaticDraw);

            Mesh offsetStripHorizontalMesh = RectangleTessellator.Compute(
                new RectangleD(new Vector2D(0.0, 0.0), new Vector2D(2 * _fieldBlockPosts, 1.0)),
                2 * _fieldBlockPosts, 1);
            _offsetStripHorizontal = context.CreateVertexArray(offsetStripHorizontalMesh, _shaderProgram.VertexAttributes, BufferHint.StaticDraw);

            Mesh offsetStripVerticalMesh = RectangleTessellator.Compute(
                new RectangleD(new Vector2D(0.0, 0.0), new Vector2D(1.0, 2 * _fieldBlockPosts - 1)),
                1, 2 * _fieldBlockPosts - 1);
            _offsetStripVertical = context.CreateVertexArray(offsetStripVerticalMesh, _shaderProgram.VertexAttributes, BufferHint.StaticDraw);

            // These finest offset strips are not necessary... use the horizontal and vertical fixups instead.
            Mesh finestOffsetStripHorizontalMesh = RectangleTessellator.Compute(
                new RectangleD(new Vector2D(0.0, 0.0), new Vector2D(2 * _fieldBlockPosts, 2.0)),
                2 * _fieldBlockPosts, 2);
            _finestOffsetStripHorizontal = context.CreateVertexArray(finestOffsetStripHorizontalMesh, _shaderProgram.VertexAttributes, BufferHint.StaticDraw);

            Mesh finestOffsetStripVerticalMesh = RectangleTessellator.Compute(
                new RectangleD(new Vector2D(0.0, 0.0), new Vector2D(2.0, 2 * _fieldBlockPosts - 1)),
                2, 2 * _fieldBlockPosts - 1);
            _finestOffsetStripVertical = context.CreateVertexArray(finestOffsetStripVerticalMesh, _shaderProgram.VertexAttributes, BufferHint.StaticDraw);

            Mesh degenerateTriangleMesh = CreateDegenerateTriangleMesh();
            _degenerateTriangles = context.CreateVertexArray(degenerateTriangleMesh, _shaderProgram.VertexAttributes, BufferHint.StaticDraw);

            _patchOriginInClippedLevel = (Uniform<Vector2F>)_shaderProgram.Uniforms["u_patchOriginInClippedLevel"];
            _levelScaleFactor = (Uniform<Vector2F>)_shaderProgram.Uniforms["u_levelScaleFactor"];
            _levelZeroWorldScaleFactor = (Uniform<Vector2F>)_shaderProgram.Uniforms["u_levelZeroWorldScaleFactor"];
            _levelOffsetFromWorldOrigin = (Uniform<Vector2F>)_shaderProgram.Uniforms["u_levelOffsetFromWorldOrigin"];
            _heightExaggeration = (Uniform<float>)_shaderProgram.Uniforms["u_heightExaggeration"];
            _viewPosInClippedLevel = (Uniform<Vector2F>)_shaderProgram.Uniforms["u_viewPosInClippedLevel"];
            _fineLevelOriginInCoarse = (Uniform<Vector2F>)_shaderProgram.Uniforms["u_fineLevelOriginInCoarse"];
            _unblendedRegionSize = (Uniform<Vector2F>)_shaderProgram.Uniforms["u_unblendedRegionSize"];
            _oneOverBlendedRegionSize = (Uniform<Vector2F>)_shaderProgram.Uniforms["u_oneOverBlendedRegionSize"];
            _sunPositionRelativeToViewer = (Uniform<Vector3F>)_shaderProgram.Uniforms["u_sunPositionRelativeToViewer"];
            _fineTextureOrigin = (Uniform<Vector2F>)_shaderProgram.Uniforms["u_fineTextureOrigin"];
            _showBlendRegions = (Uniform<bool>)_shaderProgram.Uniforms["u_showBlendRegions"];
            _useBlendRegions = (Uniform<bool>)_shaderProgram.Uniforms["u_useBlendRegions"];
            _oneOverClipmapSize = (Uniform<float>)_shaderProgram.Uniforms["u_oneOverClipmapSize"];

            _renderState = new RenderState();
            _renderState.FacetCulling.FrontFaceWindingOrder = fieldBlockMesh.FrontFaceWindingOrder;
            _primitiveType = fieldBlockMesh.PrimitiveType;

            float oneOverBlendedRegionSize = (float)(10.0 / _clipmapPosts);
            _oneOverBlendedRegionSize.Value = new Vector2F(oneOverBlendedRegionSize, oneOverBlendedRegionSize);

            float unblendedRegionSize = (float)(_clipmapSegments / 2 - _clipmapPosts / 10.0 - 1);
            _unblendedRegionSize.Value = new Vector2F(unblendedRegionSize, unblendedRegionSize);

            _useBlendRegions.Value = true;

            _oneOverClipmapSize.Value = 1.0f / clipmapPosts;

            _updater = new ClipmapUpdater(context, _clipmapPosts + 2);

            HeightExaggeration = 0.00001f;
        }

        public bool Wireframe
        {
            get { return _wireframe; }
            set { _wireframe = value; }
        }

        public bool BlendRegionsEnabled
        {
            get { return _blendRegionsEnabled; }
            set { _blendRegionsEnabled = value; }
        }

        public bool ShowBlendRegions
        {
            get { return _showBlendRegions.Value; }
            set { _showBlendRegions.Value = value; }
        }

        public float HeightExaggeration
        {
            get { return _heightExaggeration.Value; }
            set
            {
                _heightExaggeration.Value = value;
                _updater.HeightExaggeration = value;
            }
        }

        public bool LodUpdateEnabled
        {
            get { return _lodUpdateEnabled; }
            set { _lodUpdateEnabled = value; }
        }

        public void PreRender(Context context, SceneState sceneState)
        {
            if (!_lodUpdateEnabled)
                return;

            _clipmapCenter = sceneState.Camera.Eye;

            //Geodetic2D center = Ellipsoid.ScaledWgs84.ToGeodetic2D(sceneState.Camera.Target / Ellipsoid.Wgs84.MaximumRadius);
            Geodetic2D center = new Geodetic2D(_clipmapCenter.X, _clipmapCenter.Y);
            double centerLongitude = center.Longitude; //Trig.ToDegrees(center.Longitude);
            double centerLatitude = center.Latitude; //Trig.ToDegrees(center.Latitude);

            ClipmapLevel level = _clipmapLevels[_clipmapLevels.Length - 1];
            double longitudeIndex = level.Terrain.LongitudeToIndex(centerLongitude);
            double latitudeIndex = level.Terrain.LatitudeToIndex(centerLatitude);

            int west = (int)(longitudeIndex - _clipmapPosts / 2);
            if ((west % 2) != 0)
            {
                ++west;
            }
            int south = (int)(latitudeIndex - _clipmapPosts / 2);
            if ((south % 2) != 0)
            {
                ++south;
            }

            level.NextExtent.West = west;
            level.NextExtent.East = west + _clipmapSegments;
            level.NextExtent.South = south;
            level.NextExtent.North = south + _clipmapSegments;
            level.OffsetStripOnEast = true;
            level.OffsetStripOnNorth = true;

            for (int i = _clipmapLevels.Length - 2; i >= 0; --i)
            {
                level = _clipmapLevels[i];
                ClipmapLevel finerLevel = _clipmapLevels[i + 1];

                level.NextExtent.West = finerLevel.NextExtent.West / 2 - _fieldBlockSegments;
                level.OffsetStripOnEast = (level.NextExtent.West % 2) == 0;
                if (!level.OffsetStripOnEast)
                {
                    --level.NextExtent.West;
                }
                level.NextExtent.East = level.NextExtent.West + _clipmapSegments;

                level.NextExtent.South = finerLevel.NextExtent.South / 2 - _fieldBlockSegments;
                level.OffsetStripOnNorth = (level.NextExtent.South % 2) == 0;
                if (!level.OffsetStripOnNorth)
                {
                    --level.NextExtent.South;
                }
                level.NextExtent.North = level.NextExtent.South + _clipmapSegments;
            }

            for (int i = _clipmapLevels.Length - 1; i >= 0; --i)
            {
                ClipmapLevel thisLevel = _clipmapLevels[i];
                ClipmapLevel coarserLevel = _clipmapLevels[i > 0 ? i - 1 : 0];

                PreRenderLevel(thisLevel, coarserLevel, context, sceneState);
            }
        }

        private void PreRenderLevel(ClipmapLevel level, ClipmapLevel coarserLevel, Context context, SceneState sceneState)
        {
            int deltaX = level.NextExtent.West - level.CurrentExtent.West;
            int deltaY = level.NextExtent.South - level.CurrentExtent.South;
            if (deltaX == 0 && deltaY == 0)
                return;

            int minLongitude = deltaX > 0 ? level.CurrentExtent.East + 1 : level.NextExtent.West;
            int maxLongitude = deltaX > 0 ? level.NextExtent.East : level.CurrentExtent.West - 1;
            int minLatitude = deltaY > 0 ? level.CurrentExtent.North + 1 : level.NextExtent.South;
            int maxLatitude = deltaY > 0 ? level.NextExtent.North : level.CurrentExtent.South - 1;

            int width = maxLongitude - minLongitude + 1;
            int height = maxLatitude - minLatitude + 1;

            int newOriginX = (level.OriginInTextures.X + deltaX) % _clipmapPosts;
            if (newOriginX < 0)
                newOriginX += _clipmapPosts;
            int newOriginY = (level.OriginInTextures.Y + deltaY) % _clipmapPosts;
            if (newOriginY < 0)
                newOriginY += _clipmapPosts;

            if (level.CurrentExtent.West > level.CurrentExtent.East || // initial update
                width >= _clipmapPosts || height >= _clipmapPosts)
            {
                // Initial or complete update.
                width = _clipmapPosts;
                height = _clipmapPosts;
                deltaX = _clipmapPosts;
                deltaY = _clipmapPosts;
                minLongitude = level.NextExtent.West;
                maxLongitude = level.NextExtent.East;
                minLatitude = level.NextExtent.South;
                maxLatitude = level.NextExtent.North;
                newOriginX = 0;
                newOriginY = 0;
            }

            level.OriginInTextures = new Vector2I(newOriginX, newOriginY);

            if (height > 0)
            {
                ClipmapUpdate horizontalUpdate = new ClipmapUpdate(
                    level,
                    level.NextExtent.West,
                    minLatitude,
                    level.NextExtent.East,
                    maxLatitude);
                _updater.Update(context, level, horizontalUpdate);
            }

            if (width > 0)
            {
                ClipmapUpdate verticalUpdate = new ClipmapUpdate(
                    level,
                    minLongitude,
                    level.NextExtent.South,
                    maxLongitude,
                    level.NextExtent.North);
                _updater.Update(context, level, verticalUpdate);
            }

            level.CurrentExtent.West = level.NextExtent.West;
            level.CurrentExtent.South = level.NextExtent.South;
            level.CurrentExtent.East = level.NextExtent.East;
            level.CurrentExtent.North = level.NextExtent.North;
        }

        //private void VerifyHeights(Level level)
        //{
        //    ReadPixelBuffer rpb = level.TerrainTexture.CopyToBuffer(ImageFormat.Red, ImageDatatype.Float);
        //    float[] postsFromTexture = rpb.CopyToSystemMemory<float>();

        //    ReadPixelBuffer rpbNormals = level.NormalTexture.CopyToBuffer(ImageFormat.RedGreenBlue, ImageDatatype.Float);
        //    Vector3F[] normalsFromTexture = rpbNormals.CopyToSystemMemory<Vector3F>();

        //    float[] realPosts = new float[_clipmapPosts * _clipmapPosts];
        //    level.Terrain.GetPosts(level.CurrentOrigin.TerrainWest, level.CurrentOrigin.TerrainSouth, level.CurrentOrigin.TerrainEast, level.CurrentOrigin.TerrainNorth, realPosts, 0, _clipmapPosts);

        //    float heightExaggeration = HeightExaggeration;
        //    float postDelta = (float)level.Terrain.PostDeltaLongitude;

        //    for (int j = 0; j < _clipmapPosts; ++j)
        //    {
        //        int y = (j + level.OriginInTexture.Y) % _clipmapPosts;
        //        for (int i = 0; i < _clipmapPosts; ++i)
        //        {
        //            int x = (i + level.OriginInTexture.X) % _clipmapPosts;

        //            float realHeight = realPosts[j * _clipmapPosts + i];
        //            float heightFromTexture = postsFromTexture[y * _clipmapPosts + x];

        //            if (realHeight != heightFromTexture)
        //                throw new Exception("bad");

        //            if (i != 0 && i != _clipmapPosts - 1 &&
        //                j != 0 && j != _clipmapPosts - 1)
        //            {
        //                int top = (j + 1) * _clipmapPosts + i;
        //                float topHeight = realPosts[top] * heightExaggeration;
        //                int bottom = (j - 1) * _clipmapPosts + i;
        //                float bottomHeight = realPosts[bottom] * heightExaggeration;
        //                int right = j * _clipmapPosts + i + 1;
        //                float rightHeight = realPosts[right] * heightExaggeration;
        //                int left = j * _clipmapPosts + i - 1;
        //                float leftHeight = realPosts[left] * heightExaggeration;

        //                Vector3F realNormal = new Vector3F(leftHeight - rightHeight, bottomHeight - topHeight, 2.0f * postDelta).Normalize();

        //                Vector3F normalFromTexture = normalsFromTexture[y * _clipmapPosts + x].Normalize();

        //                if (!realNormal.EqualsEpsilon(normalFromTexture, 1e-5f))
        //                    throw new Exception("normal is bad.");
        //            }
        //        }
        //    }
        //}

        public void Render(Context context, SceneState sceneState)
        {
            if (_wireframe)
            {
                _renderState.RasterizationMode = RasterizationMode.Line;
            }
            else
            {
                _renderState.RasterizationMode = RasterizationMode.Fill;
            }

            Vector3D previousTarget = sceneState.Camera.Target;
            Vector3D previousEye = sceneState.Camera.Eye;

            _sunPositionRelativeToViewer.Value = (sceneState.SunPosition - _clipmapCenter).ToVector3F();

            Vector3D toSubtract = new Vector3D(_clipmapCenter.X, _clipmapCenter.Y, 0.0);
            sceneState.Camera.Target -= toSubtract;
            sceneState.Camera.Eye -= toSubtract;

            _levelZeroWorldScaleFactor.Value = new Vector2F((float)_clipmapLevels[0].Terrain.PostDeltaLongitude, (float)_clipmapLevels[0].Terrain.PostDeltaLatitude);

            int maxLevel = _clipmapLevels.Length - 1;

            int longitudeIndex = (int)_clipmapLevels[0].Terrain.LongitudeToIndex(_clipmapCenter.X);
            int latitudeIndex = (int)_clipmapLevels[0].Terrain.LatitudeToIndex(_clipmapCenter.Y);

            float[] heightSample = new float[1];
            _clipmapLevels[0].Terrain.GetPosts(longitudeIndex, latitudeIndex, longitudeIndex, latitudeIndex, heightSample, 0, 1);

            /*while (maxLevel > 0)
            {
                double terrainHeight = heightSample[0] * _heightExaggeration.Value; // TODO: get the real terrain height
                double viewerHeight = clipmapCenter.Z;
                double h = viewerHeight - terrainHeight;
                double gridExtent = _clipmapLevels[maxLevel].Terrain.PostDeltaLongitude * _clipmapPosts;
                if (h <= 0.4 * gridExtent)
                {
                    break;
                }
                --maxLevel;
            }*/

            Vector2D center = toSubtract.XY;

            bool rendered = false;
            for (int i = maxLevel; i >= 0; --i)
            {
                ClipmapLevel thisLevel = _clipmapLevels[i];
                ClipmapLevel coarserLevel = _clipmapLevels[i > 0 ? i - 1 : 0];

                rendered = RenderLevel(i, thisLevel, coarserLevel, !rendered, center, context, sceneState);
            }

            sceneState.Camera.Target = previousTarget;
            sceneState.Camera.Eye = previousEye;
        }

        private bool RenderLevel(int levelIndex, ClipmapLevel level, ClipmapLevel coarserLevel, bool fillRing, Vector2D center, Context context, SceneState sceneState)
        {
            context.TextureUnits[0].Texture = level.HeightTexture;
            context.TextureUnits[0].TextureSampler = Device.TextureSamplers.NearestRepeat;
            context.TextureUnits[1].Texture = coarserLevel.HeightTexture;
            context.TextureUnits[1].TextureSampler = Device.TextureSamplers.LinearRepeat;
            context.TextureUnits[2].Texture = level.NormalTexture;
            context.TextureUnits[2].TextureSampler = Device.TextureSamplers.LinearRepeat;
            context.TextureUnits[3].Texture = coarserLevel.NormalTexture;
            context.TextureUnits[3].TextureSampler = Device.TextureSamplers.LinearRepeat;

            int west = level.CurrentExtent.West;
            int south = level.CurrentExtent.South;
            int east = level.CurrentExtent.East;
            int north = level.CurrentExtent.North;

            float levelScaleFactor = (float)Math.Pow(2.0, -levelIndex);
            _levelScaleFactor.Value = new Vector2F(levelScaleFactor, levelScaleFactor);

            double originLongitude = level.Terrain.IndexToLongitude(level.CurrentExtent.West);
            double originLatitude = level.Terrain.IndexToLatitude(level.CurrentExtent.South);
            _levelOffsetFromWorldOrigin.Value = new Vector2F((float)(originLongitude - center.X),
                                                             (float)(originLatitude - center.Y));

            int coarserWest = coarserLevel.CurrentExtent.West;
            int coarserSouth = coarserLevel.CurrentExtent.South;
            _fineLevelOriginInCoarse.Value = coarserLevel.OriginInTextures.ToVector2F() +
                                             new Vector2F(west / 2 - coarserWest + 0.5f,
                                                          south / 2 - coarserSouth + 0.5f);

            _viewPosInClippedLevel.Value = new Vector2F((float)(level.Terrain.LongitudeToIndex(center.X) - level.CurrentExtent.West),
                                                        (float)(level.Terrain.LatitudeToIndex(center.Y) - level.CurrentExtent.South));

            _fineTextureOrigin.Value = level.OriginInTextures.ToVector2F() + new Vector2F(0.5f, 0.5f);

            _useBlendRegions.Value = _blendRegionsEnabled && level != coarserLevel;

            DrawBlock(_fieldBlock, level, coarserLevel, west, south, west, south, context, sceneState);
            DrawBlock(_fieldBlock, level, coarserLevel, west, south, west + _fieldBlockSegments, south, context, sceneState);
            DrawBlock(_fieldBlock, level, coarserLevel, west, south, east - 2 * _fieldBlockSegments, south, context, sceneState);
            DrawBlock(_fieldBlock, level, coarserLevel, west, south, east - _fieldBlockSegments, south, context, sceneState);

            DrawBlock(_fieldBlock, level, coarserLevel, west, south, west, south + _fieldBlockSegments, context, sceneState);
            DrawBlock(_fieldBlock, level, coarserLevel, west, south, east - _fieldBlockSegments, south + _fieldBlockSegments, context, sceneState);

            DrawBlock(_fieldBlock, level, coarserLevel, west, south, west, north - 2 * _fieldBlockSegments, context, sceneState);
            DrawBlock(_fieldBlock, level, coarserLevel, west, south, east - _fieldBlockSegments, north - 2 * _fieldBlockSegments, context, sceneState);

            DrawBlock(_fieldBlock, level, coarserLevel, west, south, west, north - _fieldBlockSegments, context, sceneState);
            DrawBlock(_fieldBlock, level, coarserLevel, west, south, west + _fieldBlockSegments, north - _fieldBlockSegments, context, sceneState);
            DrawBlock(_fieldBlock, level, coarserLevel, west, south, east - 2 * _fieldBlockSegments, north - _fieldBlockSegments, context, sceneState);
            DrawBlock(_fieldBlock, level, coarserLevel, west, south, east - _fieldBlockSegments, north - _fieldBlockSegments, context, sceneState);

            DrawBlock(_ringFixupHorizontal, level, coarserLevel, west, south, west, south + 2 * _fieldBlockSegments, context, sceneState);
            DrawBlock(_ringFixupHorizontal, level, coarserLevel, west, south, east - _fieldBlockSegments, south + 2 * _fieldBlockSegments, context, sceneState);

            DrawBlock(_ringFixupVertical, level, coarserLevel, west, south, west + 2 * _fieldBlockSegments, south, context, sceneState);
            DrawBlock(_ringFixupVertical, level, coarserLevel, west, south, west + 2 * _fieldBlockSegments, north - _fieldBlockSegments, context, sceneState);

            DrawBlock(_degenerateTriangles, level, coarserLevel, west, south, west, south, context, sceneState);

            // Fill the center of the highest-detail ring
            if (fillRing)
            {
                int westOffset = level.OffsetStripOnEast ? 0 : 2;
                int southOffset = level.OffsetStripOnNorth ? 0 : 2;

                DrawBlock(_fieldBlock, level, coarserLevel, west, south, west + _fieldBlockSegments + westOffset, south + _fieldBlockSegments + southOffset, context, sceneState);
                DrawBlock(_fieldBlock, level, coarserLevel, west, south, west + 2 * _fieldBlockSegments + westOffset, south + _fieldBlockSegments + southOffset, context, sceneState);
                DrawBlock(_fieldBlock, level, coarserLevel, west, south, west + _fieldBlockSegments + westOffset, south + 2 * _fieldBlockSegments + southOffset, context, sceneState);
                DrawBlock(_fieldBlock, level, coarserLevel, west, south, west + 2 * _fieldBlockSegments + westOffset, south + 2 * _fieldBlockSegments + southOffset, context, sceneState);

                int offset = level.OffsetStripOnNorth
                                ? north - _fieldBlockPosts - 1
                                : south + _fieldBlockSegments;
                DrawBlock(_finestOffsetStripHorizontal, level, coarserLevel, west, south, west + _fieldBlockSegments, offset, context, sceneState);

                offset = level.OffsetStripOnEast
                                ? east - _fieldBlockPosts - 1
                                : west + _fieldBlockSegments;
                DrawBlock(_finestOffsetStripVertical, level, coarserLevel, west, south, offset, south + _fieldBlockSegments + southOffset, context, sceneState);
            }
            else
            {
                int offset = level.OffsetStripOnNorth
                                ? north - _fieldBlockPosts
                                : south + _fieldBlockSegments;
                DrawBlock(_offsetStripHorizontal, level, coarserLevel, west, south, west + _fieldBlockSegments, offset, context, sceneState);

                int southOffset = level.OffsetStripOnNorth ? 0 : 1;
                offset = level.OffsetStripOnEast
                                ? east - _fieldBlockPosts
                                : west + _fieldBlockSegments;
                DrawBlock(_offsetStripVertical, level, coarserLevel, west, south, offset, south + _fieldBlockSegments + southOffset, context, sceneState);
            }

            return true;
        }

        private void DrawBlock(VertexArray block, ClipmapLevel level, ClipmapLevel coarserLevel, int overallWest, int overallSouth, int blockWest, int blockSouth, Context context, SceneState sceneState)
        {
            int textureWest = blockWest - overallWest;
            int textureSouth = blockSouth - overallSouth;

            _patchOriginInClippedLevel.Value = new Vector2F(textureWest, textureSouth);
            DrawState drawState = new DrawState(_renderState, _shaderProgram, block);
            context.Draw(_primitiveType, drawState, sceneState);
        }

        public void Dispose()
        {
        }

        private Mesh CreateDegenerateTriangleMesh()
        {
            Mesh mesh = new Mesh();
            mesh.PrimitiveType = PrimitiveType.Triangles;
            mesh.FrontFaceWindingOrder = WindingOrder.Counterclockwise;

            int numberOfPositions = _clipmapSegments * 4;
            VertexAttributeFloatVector2 positionsAttribute = new VertexAttributeFloatVector2("position", numberOfPositions);
            IList<Vector2F> positions = positionsAttribute.Values;
            mesh.Attributes.Add(positionsAttribute);

            int numberOfIndices = (_clipmapSegments / 2) * 3 * 4;
            IndicesUnsignedShort indices = new IndicesUnsignedShort(numberOfIndices);
            mesh.Indices = indices;

            for (int i = 0; i < _clipmapPosts; ++i)
            {
                positions.Add(new Vector2F(0.0f, i));
            }

            for (int i = 1; i < _clipmapPosts; ++i)
            {
                positions.Add(new Vector2F(i, _clipmapSegments));
            }

            for (int i = _clipmapSegments - 1; i >= 0; --i)
            {
                positions.Add(new Vector2F(_clipmapSegments, i));
            }

            for (int i = _clipmapSegments - 1; i > 0; --i)
            {
                positions.Add(new Vector2F(i, 0.0f));
            }

            for (int i = 0; i < numberOfIndices; i += 2)
            {
                indices.AddTriangle(new TriangleIndicesUnsignedShort((ushort)i, (ushort)(i + 1), (ushort)(i + 2)));
            }

            return mesh;
        }

        private double EstimateLevelExtent(ClipmapLevel level)
        {
            int east = level.CurrentExtent.West + _clipmapSegments;
            int north = level.CurrentExtent.South + _clipmapSegments;

            Geodetic2D southwest = new Geodetic2D(
                                    Trig.ToRadians(level.Terrain.IndexToLongitude(level.CurrentExtent.West)),
                                    Trig.ToRadians(level.Terrain.IndexToLatitude(level.CurrentExtent.South)));
            Geodetic2D northeast = new Geodetic2D(
                                    Trig.ToRadians(level.Terrain.IndexToLongitude(east)),
                                    Trig.ToRadians(level.Terrain.IndexToLatitude(north)));

            Vector3D southwestCartesian = Ellipsoid.ScaledWgs84.ToVector3D(southwest);
            Vector3D northeastCartesian = Ellipsoid.ScaledWgs84.ToVector3D(northeast);

            return (northeastCartesian - southwestCartesian).Magnitude;
        }

        private class IndexOrigin
        {
            public int TerrainWest;
            public int TerrainSouth;
            public int TerrainEast;
            public int TerrainNorth;

            public void CopyTo(IndexOrigin other)
            {
                other.TerrainWest = TerrainWest;
                other.TerrainSouth = TerrainSouth;
                other.TerrainEast = TerrainEast;
                other.TerrainNorth = TerrainNorth;
            }
        }

        private RasterTerrainSource _terrainSource;
        private int _clipmapPosts;
        private int _clipmapSegments;
        private ClipmapLevel[] _clipmapLevels;

        private ShaderProgram _shaderProgram;
        private RenderState _renderState;
        private PrimitiveType _primitiveType;

        private int _fieldBlockPosts;
        private int _fieldBlockSegments;

        private VertexArray _fieldBlock;
        private VertexArray _ringFixupHorizontal;
        private VertexArray _ringFixupVertical;
        private VertexArray _offsetStripHorizontal;
        private VertexArray _offsetStripVertical;
        private VertexArray _finestOffsetStripHorizontal;
        private VertexArray _finestOffsetStripVertical;
        private VertexArray _degenerateTriangles;

        private Uniform<Vector2F> _patchOriginInClippedLevel;
        private Uniform<Vector2F> _levelScaleFactor;
        private Uniform<Vector2F> _levelZeroWorldScaleFactor;
        private Uniform<Vector2F> _levelOffsetFromWorldOrigin;
        private Uniform<float> _heightExaggeration;
        private Uniform<Vector2F> _fineLevelOriginInCoarse;
        private Uniform<Vector2F> _viewPosInClippedLevel;
        private Uniform<Vector2F> _unblendedRegionSize;
        private Uniform<Vector2F> _oneOverBlendedRegionSize;
        private Uniform<Vector3F> _sunPositionRelativeToViewer;
        private Uniform<Vector2F> _fineTextureOrigin;
        private Uniform<bool> _showBlendRegions;
        private Uniform<bool> _useBlendRegions;
        private Uniform<float> _oneOverClipmapSize;

        private bool _wireframe;
        private bool _blendRegionsEnabled = true;
        private bool _lodUpdateEnabled = true;

        private ClipmapUpdater _updater;
        private Vector3D _clipmapCenter;
    }
}
