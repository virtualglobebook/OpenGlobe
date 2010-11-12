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
        public PlaneClipmapTerrain(Context context, RasterTerrainSource terrainSource, int clipmapPosts, EsriRestImagery imagery)
        {
            _terrainSource = terrainSource;
            _clipmapPosts = clipmapPosts;
            _clipmapSegments = _clipmapPosts - 1;
            _imagery = imagery;

            int clipmapLevels = _terrainSource.Levels.Count;
            _clipmapLevels = new Level[clipmapLevels];

            for (int i = 0; i < _clipmapLevels.Length; ++i)
            {
                RasterTerrainLevel terrainLevel = _terrainSource.Levels[i];
                _clipmapLevels[i] = new Level();
                _clipmapLevels[i].Terrain = terrainLevel;
                _clipmapLevels[i].TerrainTexture = Device.CreateTexture2DRectangle(new Texture2DDescription(_clipmapPosts, _clipmapPosts, TextureFormat.Red32f));

                // Aim for roughly one imagery texel per geometry texel.
                // Find the first imagery level that meets our resolution needs.
                double longitudeResRequired = terrainLevel.PostDeltaLongitude;
                double latitudeResRequired = terrainLevel.PostDeltaLatitude;
                EsriRestImageryLevel imageryLevel = null;
                for (int j = 0; j < _imagery.Levels.Count; ++j)
                {
                    imageryLevel = _imagery.Levels[j];
                    if (imageryLevel.PostDeltaLongitude <= longitudeResRequired &&
                        imageryLevel.PostDeltaLatitude <= latitudeResRequired)
                    {
                        break;
                    }
                }

                _clipmapLevels[i].Imagery = imageryLevel;
                _clipmapLevels[i].ImageryWidth = (int)Math.Ceiling(_clipmapPosts * terrainLevel.PostDeltaLongitude / imageryLevel.PostDeltaLongitude);
                _clipmapLevels[i].ImageryHeight = (int)Math.Ceiling(_clipmapPosts * terrainLevel.PostDeltaLatitude / imageryLevel.PostDeltaLatitude);
                _clipmapLevels[i].ImageryTexture = Device.CreateTexture2DRectangle(new Texture2DDescription(_clipmapLevels[i].ImageryWidth, _clipmapLevels[i].ImageryHeight, TextureFormat.RedGreenBlue8, false));
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

            _patchOriginInClippedLevel = (Uniform<Vector2S>)_shaderProgram.Uniforms["u_patchOriginInClippedLevel"];
            _levelScaleFactor = (Uniform<Vector2S>)_shaderProgram.Uniforms["u_levelScaleFactor"];
            _levelZeroWorldScaleFactor = (Uniform<Vector2S>)_shaderProgram.Uniforms["u_levelZeroWorldScaleFactor"];
            _levelOffsetFromWorldOrigin = (Uniform<Vector2S>)_shaderProgram.Uniforms["u_levelOffsetFromWorldOrigin"];
            _heightExaggeration = (Uniform<float>)_shaderProgram.Uniforms["u_heightExaggeration"];
            _viewPosInClippedLevel = (Uniform<Vector2S>)_shaderProgram.Uniforms["u_viewPosInClippedLevel"];
            _fineLevelOriginInCoarse = (Uniform<Vector2S>)_shaderProgram.Uniforms["u_fineLevelOriginInCoarse"];
            _unblendedRegionSize = (Uniform<Vector2S>)_shaderProgram.Uniforms["u_unblendedRegionSize"];
            _oneOverBlendedRegionSize = (Uniform<Vector2S>)_shaderProgram.Uniforms["u_oneOverBlendedRegionSize"];
            _sunPositionRelativeToViewer = (Uniform<Vector3S>)_shaderProgram.Uniforms["u_sunPositionRelativeToViewer"];

            _renderState = new RenderState();
            _renderState.FacetCulling.FrontFaceWindingOrder = fieldBlockMesh.FrontFaceWindingOrder;
            _primitiveType = fieldBlockMesh.PrimitiveType;

            float oneOverBlendedRegionSize = (float)(10.0 / _clipmapPosts);
            _oneOverBlendedRegionSize.Value = new Vector2S(oneOverBlendedRegionSize, oneOverBlendedRegionSize);

            float unblendedRegionSize = (float)(_clipmapSegments / 2 - _clipmapPosts / 10.0 - 1);
            _unblendedRegionSize.Value = new Vector2S(unblendedRegionSize, unblendedRegionSize);

            HeightExaggeration = 0.00001f;
        }

        public bool Wireframe
        {
            get { return _wireframe; }
            set { _wireframe = value; }
        }

        public float HeightExaggeration
        {
            get { return _heightExaggeration.Value; }
            set { _heightExaggeration.Value = value; }
        }

        public void PreRender(Context context, SceneState sceneState)
        {
            Vector3D clipmapCenter = sceneState.Camera.Eye;
            //Geodetic2D center = Ellipsoid.ScaledWgs84.ToGeodetic2D(sceneState.Camera.Target / Ellipsoid.Wgs84.MaximumRadius);
            Geodetic2D center = new Geodetic2D(clipmapCenter.X, clipmapCenter.Y);
            double centerLongitude = center.Longitude; //Trig.ToDegrees(center.Longitude);
            double centerLatitude = center.Latitude; //Trig.ToDegrees(center.Latitude);

            Level level = _clipmapLevels[_clipmapLevels.Length - 1];
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

            level.CurrentOrigin.TerrainWest = west;
            level.CurrentOrigin.TerrainSouth = south;
            level.OffsetStripOnEast = true;
            level.OffsetStripOnNorth = true;

            for (int i = _clipmapLevels.Length - 2; i >= 0; --i)
            {
                level = _clipmapLevels[i];
                Level finerLevel = _clipmapLevels[i + 1];

                level.CurrentOrigin.TerrainWest = finerLevel.CurrentOrigin.TerrainWest / 2 - _fieldBlockSegments;
                level.OffsetStripOnEast = (level.CurrentOrigin.TerrainWest % 2) == 0;
                if (!level.OffsetStripOnEast)
                {
                    --level.CurrentOrigin.TerrainWest;
                }

                level.CurrentOrigin.TerrainSouth = finerLevel.CurrentOrigin.TerrainSouth / 2 - _fieldBlockSegments;
                level.OffsetStripOnNorth = (level.CurrentOrigin.TerrainSouth % 2) == 0;
                if (!level.OffsetStripOnNorth)
                {
                    --level.CurrentOrigin.TerrainSouth;
                }
            }

            for (int i = _clipmapLevels.Length - 1; i >= 0; --i)
            {
                Level thisLevel = _clipmapLevels[i];
                Level coarserLevel = _clipmapLevels[i > 0 ? i - 1 : 0];

                PreRenderLevel(thisLevel, coarserLevel, context, sceneState);
            }
        }

        private void PreRenderLevel(Level level, Level coarserLevel, Context context, SceneState sceneState)
        {
            int west = level.CurrentOrigin.TerrainWest;
            int south = level.CurrentOrigin.TerrainSouth;
            int east = west + _clipmapPosts - 1;
            int north = south + _clipmapPosts - 1;

            float[] posts = new float[_clipmapPosts * _clipmapPosts];
            level.Terrain.GetPosts(west, south, east, north, posts, 0, _clipmapPosts);

            /*Geodetic3D eye = Ellipsoid.ScaledWgs84.ToGeodetic3D(sceneState.Camera.Target);
            double heightAboveTerrain = eye.Height - posts[(_clipmapPosts / 2) * (_clipmapPosts + 1)] / Ellipsoid.Wgs84.MaximumRadius;

            double levelExtent = EstimateLevelExtent(level);
            if (level != coarserLevel && levelExtent < heightAboveTerrain)
            {
                // Do not render this level.
                //return false;
            }*/

            using (WritePixelBuffer pixelBuffer = Device.CreateWritePixelBuffer(PixelBufferHint.Stream, _clipmapPosts * _clipmapPosts * sizeof(float)))
            {
                pixelBuffer.CopyFromSystemMemory(posts);
                level.TerrainTexture.CopyFromBuffer(pixelBuffer, ImageFormat.Red, ImageDatatype.Float);
            }

            // Map the terrain posts indices to imagery post indices and offsets
            level.CurrentOrigin.ImageryWest = level.Imagery.LongitudeToIndex(level.Terrain.IndexToLongitude(level.CurrentOrigin.TerrainWest));
            level.CurrentOrigin.ImagerySouth = level.Imagery.LatitudeToIndex(level.Terrain.IndexToLatitude(level.CurrentOrigin.TerrainSouth));
            int imageryEastIndex = (int)level.CurrentOrigin.ImageryWest + level.ImageryWidth - 1;
            int imageryNorthIndex = (int)level.CurrentOrigin.ImagerySouth + level.ImageryHeight - 1;

            /*byte[] imagery = level.Imagery.GetImage((int)level.CurrentOrigin.ImageryWest, (int)level.CurrentOrigin.ImagerySouth, imageryEastIndex, imageryNorthIndex);

            using (WritePixelBuffer imageryPixelBuffer = Device.CreateWritePixelBuffer(PixelBufferHint.Stream, imagery.Length))
            {
                imageryPixelBuffer.CopyFromSystemMemory(imagery);
                level.ImageryTexture.CopyFromBuffer(imageryPixelBuffer, ImageFormat.BlueGreenRed, ImageDatatype.UnsignedByte);
            }*/
        }

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

            Vector3D clipmapCenter = sceneState.Camera.Eye;

            // Scale the camera eye and target positions to render to the scaled ellipsoid instead of the
            // true WGS84 ellipsoid, without affecting the view.  This avoids some precision problems.
            Vector3D previousTarget = sceneState.Camera.Target;
            Vector3D previousEye = sceneState.Camera.Eye;
            double previousNearPlane = sceneState.Camera.PerspectiveNearPlaneDistance;
            double previousFarPlane = sceneState.Camera.PerspectiveFarPlaneDistance;

            _sunPositionRelativeToViewer.Value = (sceneState.SunPosition - clipmapCenter).ToVector3S();

            Vector3D toSubtract = new Vector3D(clipmapCenter.X, clipmapCenter.Y, 0.0);
            sceneState.Camera.Target -= toSubtract;
            sceneState.Camera.Eye -= toSubtract;
            //sceneState.Camera.Target /= Ellipsoid.Wgs84.MaximumRadius;
            //sceneState.Camera.Eye /= Ellipsoid.Wgs84.MaximumRadius;
            //sceneState.Camera.PerspectiveNearPlaneDistance /= Ellipsoid.Wgs84.MaximumRadius;
            //sceneState.Camera.PerspectiveFarPlaneDistance /= Ellipsoid.Wgs84.MaximumRadius;

            _levelZeroWorldScaleFactor.Value = new Vector2S((float)_clipmapLevels[0].Terrain.PostDeltaLongitude, (float)_clipmapLevels[0].Terrain.PostDeltaLatitude);

            int maxLevel = _clipmapLevels.Length - 1;

            int longitudeIndex = (int)_clipmapLevels[0].Terrain.LongitudeToIndex(clipmapCenter.X);
            int latitudeIndex = (int)_clipmapLevels[0].Terrain.LatitudeToIndex(clipmapCenter.Y);

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
                Level thisLevel = _clipmapLevels[i];
                Level coarserLevel = _clipmapLevels[i > 0 ? i - 1 : 0];

                rendered = RenderLevel(i, thisLevel, coarserLevel, !rendered, center, context, sceneState);
            }

            sceneState.Camera.Target = previousTarget;
            sceneState.Camera.Eye = previousEye;
            sceneState.Camera.PerspectiveNearPlaneDistance = previousNearPlane;
            sceneState.Camera.PerspectiveFarPlaneDistance = previousFarPlane;
        }

        private bool RenderLevel(int levelIndex, Level level, Level coarserLevel, bool fillRing, Vector2D center, Context context, SceneState sceneState)
        {
            context.TextureUnits[0].Texture = level.TerrainTexture;
            context.TextureUnits[0].TextureSampler = Device.TextureSamplers.LinearClamp;
            context.TextureUnits[1].Texture = coarserLevel.TerrainTexture;
            context.TextureUnits[1].TextureSampler = Device.TextureSamplers.LinearClamp;

            int west = level.CurrentOrigin.TerrainWest;
            int south = level.CurrentOrigin.TerrainSouth;
            int east = west + _clipmapPosts - 1;
            int north = south + _clipmapPosts - 1;

            double imageryWestTerrainPostOffset = level.CurrentOrigin.ImageryWest - (int)level.CurrentOrigin.ImageryWest;
            double imagerySouthTerrainPostOffset = level.CurrentOrigin.ImagerySouth - (int)level.CurrentOrigin.ImagerySouth;

            float levelScaleFactor = (float)Math.Pow(2.0, -levelIndex);
            _levelScaleFactor.Value = new Vector2S(levelScaleFactor, levelScaleFactor);

            double originLongitude = level.Terrain.IndexToLongitude(level.CurrentOrigin.TerrainWest);
            double originLatitude = level.Terrain.IndexToLatitude(level.CurrentOrigin.TerrainSouth);
            _levelOffsetFromWorldOrigin.Value = new Vector2S((float)(originLongitude - center.X),
                                                             (float)(originLatitude - center.Y));

            int coarserWest = coarserLevel.CurrentOrigin.TerrainWest;
            int coarserSouth = coarserLevel.CurrentOrigin.TerrainSouth;
            _fineLevelOriginInCoarse.Value = new Vector2S(west / 2 - coarserWest,
                                                          south / 2 - coarserSouth);

            _viewPosInClippedLevel.Value = new Vector2S((float)(level.Terrain.LongitudeToIndex(center.X) - level.CurrentOrigin.TerrainWest),
                                                        (float)(level.Terrain.LatitudeToIndex(center.Y) - level.CurrentOrigin.TerrainSouth));

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

        private void DrawBlock(VertexArray block, Level level, Level coarserLevel, int overallWest, int overallSouth, int blockWest, int blockSouth, Context context, SceneState sceneState)
        {
            int textureWest = blockWest - overallWest;
            int textureSouth = blockSouth - overallSouth;

            _patchOriginInClippedLevel.Value = new Vector2S(textureWest, textureSouth);
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
            VertexAttributeDoubleVector2 positionsAttribute = new VertexAttributeDoubleVector2("position", numberOfPositions);
            IList<Vector2D> positions = positionsAttribute.Values;
            mesh.Attributes.Add(positionsAttribute);

            int numberOfIndices = (_clipmapSegments / 2) * 3 * 4;
            IndicesUnsignedShort indices = new IndicesUnsignedShort(numberOfIndices);
            mesh.Indices = indices;

            for (int i = 0; i < _clipmapPosts; ++i)
            {
                positions.Add(new Vector2D(0.0, i));
            }

            for (int i = 1; i < _clipmapPosts; ++i)
            {
                positions.Add(new Vector2D(i, _clipmapSegments));
            }

            for (int i = _clipmapSegments - 1; i >= 0; --i)
            {
                positions.Add(new Vector2D(_clipmapSegments, i));
            }

            for (int i = _clipmapSegments - 1; i > 0; --i)
            {
                positions.Add(new Vector2D(i, 0.0));
            }

            for (int i = 0; i < numberOfIndices; i += 2)
            {
                indices.AddTriangle(new TriangleIndicesUnsignedShort((ushort)i, (ushort)(i + 1), (ushort)(i + 2)));
            }

            return mesh;
        }

        private double EstimateLevelExtent(Level level)
        {
            int east = level.CurrentOrigin.TerrainWest + _clipmapSegments;
            int north = level.CurrentOrigin.TerrainSouth + _clipmapSegments;

            Geodetic2D southwest = new Geodetic2D(
                                    Trig.ToRadians(level.Terrain.IndexToLongitude(level.CurrentOrigin.TerrainWest)),
                                    Trig.ToRadians(level.Terrain.IndexToLatitude(level.CurrentOrigin.TerrainSouth)));
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
            public double ImageryWest;
            public double ImagerySouth;
        }

        private class Level
        {
            public Level() { }
            public Level(Level existingInstance)
            {
                Terrain = existingInstance.Terrain;
                TerrainTexture = existingInstance.TerrainTexture;
                Imagery = existingInstance.Imagery;
                ImageryTexture = existingInstance.ImageryTexture;
                ImageryWidth = existingInstance.ImageryWidth;
                ImageryHeight = existingInstance.ImageryHeight;
                CurrentOrigin.TerrainWest = existingInstance.CurrentOrigin.TerrainWest;
                CurrentOrigin.TerrainSouth = existingInstance.CurrentOrigin.TerrainSouth;
                OffsetStripOnNorth = existingInstance.OffsetStripOnNorth;
                OffsetStripOnEast = existingInstance.OffsetStripOnEast;
            }

            public RasterTerrainLevel Terrain;
            public Texture2D TerrainTexture;
            public EsriRestImageryLevel Imagery;
            public Texture2D ImageryTexture;
            public int ImageryWidth;
            public int ImageryHeight;

            public bool OffsetStripOnNorth;
            public bool OffsetStripOnEast;

            public IndexOrigin CurrentOrigin = new IndexOrigin();
            public IndexOrigin DesiredOrigin = new IndexOrigin();
        }

        private RasterTerrainSource _terrainSource;
        private int _clipmapPosts;
        private int _clipmapSegments;
        private Level[] _clipmapLevels;

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

        private Uniform<Vector2S> _patchOriginInClippedLevel;
        private Uniform<Vector2S> _levelScaleFactor;
        private Uniform<Vector2S> _levelZeroWorldScaleFactor;
        private Uniform<Vector2S> _levelOffsetFromWorldOrigin;
        private Uniform<float> _heightExaggeration;
        private Uniform<Vector2S> _fineLevelOriginInCoarse;
        private Uniform<Vector2S> _viewPosInClippedLevel;
        private Uniform<Vector2S> _unblendedRegionSize;
        private Uniform<Vector2S> _oneOverBlendedRegionSize;
        private Uniform<Vector3S> _sunPositionRelativeToViewer;

        private EsriRestImagery _imagery;
        private bool _wireframe;
    }
}
