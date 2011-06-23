#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using OpenGlobe.Core;
using OpenGlobe.Renderer;

namespace OpenGlobe.Scene
{
    public class GlobeClipmapTerrain : IRenderable, IDisposable
    {
        public GlobeClipmapTerrain(Context context, RasterSource terrainSource, EsriRestImagery imagery, Ellipsoid ellipsoid, int clipmapPosts)
        {
            _terrainSource = terrainSource;
            _ellipsoid = ellipsoid;

            _clipmapPosts = clipmapPosts;
            _clipmapSegments = _clipmapPosts - 1;

            int clipmapLevels = _terrainSource.Levels.Count;
            _clipmapLevels = new ClipmapLevel[clipmapLevels];

            for (int i = 0; i < _clipmapLevels.Length; ++i)
            {
                _clipmapLevels[i] = new ClipmapLevel();
            }

            for (int i = 0; i < _clipmapLevels.Length; ++i)
            {
                RasterLevel terrainLevel = _terrainSource.Levels[i];
                _clipmapLevels[i].Terrain = terrainLevel;
                _clipmapLevels[i].HeightTexture = Device.CreateTexture2D(new Texture2DDescription(_clipmapPosts, _clipmapPosts, TextureFormat.Red32f));
                _clipmapLevels[i].NormalTexture = Device.CreateTexture2D(new Texture2DDescription(_clipmapPosts, _clipmapPosts, TextureFormat.RedGreenBlue32f));
                _clipmapLevels[i].CoarserLevel = i == 0 ? null : _clipmapLevels[i - 1];
                _clipmapLevels[i].FinerLevel = i == _clipmapLevels.Length - 1 ? null : _clipmapLevels[i + 1];

                // Aim for roughly one imagery texel per geometry texel.
                // Find the first imagery level that meets our resolution needs.
                double longitudeResRequired = terrainLevel.PostDeltaLongitude;
                double latitudeResRequired = terrainLevel.PostDeltaLatitude;
                RasterLevel imageryLevel = null;
                for (int j = 0; j < imagery.Levels.Count; ++j)
                {
                    imageryLevel = imagery.Levels[j];
                    if (imageryLevel.PostDeltaLongitude <= longitudeResRequired &&
                        imageryLevel.PostDeltaLatitude <= latitudeResRequired)
                    {
                        break;
                    }
                }

                _clipmapLevels[i].Imagery = imageryLevel;
                _clipmapLevels[i].ImageryWidth = (int)Math.Ceiling(_clipmapPosts * terrainLevel.PostDeltaLongitude / imageryLevel.PostDeltaLongitude);
                _clipmapLevels[i].ImageryHeight = (int)Math.Ceiling(_clipmapPosts * terrainLevel.PostDeltaLatitude / imageryLevel.PostDeltaLatitude);
                _clipmapLevels[i].ImageryTexture = Device.CreateTexture2D(new Texture2DDescription(_clipmapLevels[i].ImageryWidth, _clipmapLevels[i].ImageryHeight, TextureFormat.RedGreenBlue8, false));
            }

            _shaderProgram = Device.CreateShaderProgram(
                EmbeddedResources.GetText("OpenGlobe.Scene.Terrain.ClipmapTerrain.GlobeClipmapVS.glsl"),
                EmbeddedResources.GetText("OpenGlobe.Scene.Terrain.ClipmapTerrain.GlobeClipmapFS.glsl"));

            _fillPatchPosts = (clipmapPosts + 1) / 4; // M
            _fillPatchSegments = _fillPatchPosts - 1;

            // Create the MxM block used to fill the ring and the field.
            Mesh fieldBlockMesh = RectangleTessellator.Compute(
                new RectangleD(new Vector2D(0.0, 0.0), new Vector2D(_fillPatchSegments, _fillPatchSegments)),
                _fillPatchSegments, _fillPatchSegments);
            _fillPatch = context.CreateVertexArray(fieldBlockMesh, _shaderProgram.VertexAttributes, BufferHint.StaticDraw);

            // Create the Mx3 block used to fill the space between the MxM blocks in the ring
            Mesh ringFixupHorizontalMesh = RectangleTessellator.Compute(
                new RectangleD(new Vector2D(0.0, 0.0), new Vector2D(_fillPatchSegments, 2.0)),
                _fillPatchSegments, 2);
            _horizontalFixupPatch = context.CreateVertexArray(ringFixupHorizontalMesh, _shaderProgram.VertexAttributes, BufferHint.StaticDraw);

            // Create the 3xM block used to fill the space between the MxM blocks in the ring
            Mesh ringFixupVerticalMesh = RectangleTessellator.Compute(
                new RectangleD(new Vector2D(0.0, 0.0), new Vector2D(2.0, _fillPatchSegments)),
                2, _fillPatchSegments);
            _verticalFixupPatch = context.CreateVertexArray(ringFixupVerticalMesh, _shaderProgram.VertexAttributes, BufferHint.StaticDraw);

            Mesh offsetStripHorizontalMesh = RectangleTessellator.Compute(
                new RectangleD(new Vector2D(0.0, 0.0), new Vector2D(2 * _fillPatchPosts, 1.0)),
                2 * _fillPatchPosts, 1);
            _horizontalOffsetPatch = context.CreateVertexArray(offsetStripHorizontalMesh, _shaderProgram.VertexAttributes, BufferHint.StaticDraw);

            Mesh offsetStripVerticalMesh = RectangleTessellator.Compute(
                new RectangleD(new Vector2D(0.0, 0.0), new Vector2D(1.0, 2 * _fillPatchPosts - 1)),
                1, 2 * _fillPatchPosts - 1);
            _verticalOffsetPatch = context.CreateVertexArray(offsetStripVerticalMesh, _shaderProgram.VertexAttributes, BufferHint.StaticDraw);

            Mesh centerMesh = RectangleTessellator.Compute(new RectangleD(new Vector2D(0.0, 0.0), new Vector2D(2.0, 2.0)), 2, 2);
            _centerPatch = context.CreateVertexArray(centerMesh, _shaderProgram.VertexAttributes, BufferHint.StaticDraw);

            Mesh degenerateTriangleMesh = CreateDegenerateTriangleMesh();
            _degenerateTrianglePatch = context.CreateVertexArray(degenerateTriangleMesh, _shaderProgram.VertexAttributes, BufferHint.StaticDraw);

            _patchOriginInClippedLevel = (Uniform<Vector2F>)_shaderProgram.Uniforms["u_patchOriginInClippedLevel"];
            _levelScaleFactor = (Uniform<Vector2F>)_shaderProgram.Uniforms["u_levelScaleFactor"];
            _levelZeroWorldScaleFactor = (Uniform<Vector2F>)_shaderProgram.Uniforms["u_levelZeroWorldScaleFactor"];
            _levelOffsetFromWorldOrigin = (Uniform<Vector2F>)_shaderProgram.Uniforms["u_levelOffsetFromWorldOrigin"];
            _heightExaggeration = (Uniform<float>)_shaderProgram.Uniforms["u_heightExaggeration"];
            _viewPosInClippedLevel = (Uniform<Vector2F>)_shaderProgram.Uniforms["u_viewPosInClippedLevel"];
            _fineLevelOriginInCoarse = (Uniform<Vector2F>)_shaderProgram.Uniforms["u_fineLevelOriginInCoarse"];
            _unblendedRegionSize = (Uniform<Vector2F>)_shaderProgram.Uniforms["u_unblendedRegionSize"];
            _oneOverBlendedRegionSize = (Uniform<Vector2F>)_shaderProgram.Uniforms["u_oneOverBlendedRegionSize"];
            _fineTextureOrigin = (Uniform<Vector2F>)_shaderProgram.Uniforms["u_fineTextureOrigin"];
            _showBlendRegions = (Uniform<bool>)_shaderProgram.Uniforms["u_showBlendRegions"];
            _useBlendRegions = (Uniform<bool>)_shaderProgram.Uniforms["u_useBlendRegions"];
            _oneOverClipmapSize = (Uniform<float>)_shaderProgram.Uniforms["u_oneOverClipmapSize"];
            _color = (Uniform<Vector3F>)_shaderProgram.Uniforms["u_color"];
            _blendRegionColor = (Uniform<Vector3F>)_shaderProgram.Uniforms["u_blendRegionColor"];
            _terrainToImageryResolutionRatio = (Uniform<Vector2F>)_shaderProgram.Uniforms["u_terrainToImageryResolutionRatio"];
            _terrainOffsetInImagery = (Uniform<Vector2F>)_shaderProgram.Uniforms["u_terrainOffsetInImagery"];
            _oneOverImagerySize = (Uniform<Vector2F>)_shaderProgram.Uniforms["u_oneOverImagerySize"];
            _showImagery = (Uniform<bool>)_shaderProgram.Uniforms["u_showImagery"];
            _lighting = (Uniform<bool>)_shaderProgram.Uniforms["u_shade"];

            ((Uniform<Vector3F>)_shaderProgram.Uniforms["u_globeRadiiSquared"]).Value =
                ellipsoid.RadiiSquared.ToVector3F();
            
            _renderState = new RenderState();
            _renderState.FacetCulling.FrontFaceWindingOrder = fieldBlockMesh.FrontFaceWindingOrder;
            _primitiveType = fieldBlockMesh.PrimitiveType;

            float oneOverBlendedRegionSize = (float)(10.0 / _clipmapPosts);
            _oneOverBlendedRegionSize.Value = new Vector2F(oneOverBlendedRegionSize, oneOverBlendedRegionSize);

            float unblendedRegionSize = (float)(_clipmapSegments / 2 - _clipmapPosts / 10.0 - 1);
            _unblendedRegionSize.Value = new Vector2F(unblendedRegionSize, unblendedRegionSize);

            _useBlendRegions.Value = true;
            _showImagery.Value = true;

            _oneOverClipmapSize.Value = 1.0f / clipmapPosts;

            _updater = new ClipmapUpdater(context, _clipmapLevels);

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

        public bool ShowImagery
        {
            get { return _showImagery.Value; }
            set { _showImagery.Value = value; }
        }

        public bool Lighting
        {
            get { return _lighting.Value; }
            set { _lighting.Value = value; }
        }

        public float HeightExaggeration
        {
            get { return _heightExaggeration.Value; }
            set
            {
                _heightExaggeration.Value = value;
                _updater.HeightExaggeration = 0.00001f; // value;
            }
        }

        public bool LodUpdateEnabled
        {
            get { return _lodUpdateEnabled; }
            set { _lodUpdateEnabled = value; }
        }

        public bool ColorClipmapLevels
        {
            get { return _colorClipmapLevels; }
            set { _colorClipmapLevels = value; }
        }

        public Ellipsoid Ellipsoid
        {
            get { return _ellipsoid; }
            set
            {
                _ellipsoid = value;
                ((Uniform<Vector3F>)_shaderProgram.Uniforms["u_globeRadiiSquared"]).Value =
                    _ellipsoid.RadiiSquared.ToVector3F();
            }
        }

        public void PreRender(Context context, SceneState sceneState)
        {
            if (!_lodUpdateEnabled)
                return;

            _clipmapCenter = _ellipsoid.ToGeodetic3D(sceneState.Camera.Eye);

            //Geodetic2D center = Ellipsoid.ScaledWgs84.ToGeodetic2D(sceneState.Camera.Target / Ellipsoid.Wgs84.MaximumRadius);
            Geodetic2D center = new Geodetic2D(_clipmapCenter.Longitude, _clipmapCenter.Latitude);
            double centerLongitude = Trig.ToDegrees(center.Longitude);
            double centerLatitude = Trig.ToDegrees(center.Latitude);

            ClipmapLevel level = _clipmapLevels[_clipmapLevels.Length - 1];
            double longitudeIndex = level.Terrain.LongitudeToIndex(centerLongitude);
            double latitudeIndex = level.Terrain.LatitudeToIndex(centerLatitude);
            double imageryLongitudeIndex = level.Imagery.LongitudeToIndex(centerLongitude);
            double imageryLatitudeIndex = level.Imagery.LatitudeToIndex(centerLatitude);

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

            int imageryWest = (int)(imageryLongitudeIndex - level.ImageryWidth / 2);
            int imagerySouth = (int)(imageryLatitudeIndex - level.ImageryHeight / 2);

            level.NextExtent.West = west;
            level.NextExtent.East = west + _clipmapSegments;
            level.NextExtent.South = south;
            level.NextExtent.North = south + _clipmapSegments;

            level.NextImageryExtent.West = imageryWest;
            level.NextImageryExtent.East = imageryWest + level.ImageryWidth;
            level.NextImageryExtent.South = imagerySouth;
            level.NextImageryExtent.North = imagerySouth + level.ImageryHeight;

            UpdateOriginInTextures(level);
            UpdateImageryOriginInTextures(level);

            for (int i = _clipmapLevels.Length - 2; i >= 0; --i)
            {
                level = _clipmapLevels[i];
                ClipmapLevel finerLevel = _clipmapLevels[i + 1];

                // Terrain
                level.NextExtent.West = finerLevel.NextExtent.West / 2 - _fillPatchSegments;
                level.OffsetStripOnEast = (level.NextExtent.West % 2) == 0;
                if (!level.OffsetStripOnEast)
                {
                    --level.NextExtent.West;
                }
                level.NextExtent.East = level.NextExtent.West + _clipmapSegments;

                level.NextExtent.South = finerLevel.NextExtent.South / 2 - _fillPatchSegments;
                level.OffsetStripOnNorth = (level.NextExtent.South % 2) == 0;
                if (!level.OffsetStripOnNorth)
                {
                    --level.NextExtent.South;
                }
                level.NextExtent.North = level.NextExtent.South + _clipmapSegments;

                // Imagery
                imageryWest = (int)level.Imagery.LongitudeToIndex(level.Terrain.IndexToLongitude(level.NextExtent.West));
                imagerySouth = (int)level.Imagery.LatitudeToIndex(level.Terrain.IndexToLatitude(level.NextExtent.South));

                level.NextImageryExtent.West = imageryWest;
                level.NextImageryExtent.East = imageryWest + level.ImageryWidth - 1;
                level.NextImageryExtent.South = imagerySouth;
                level.NextImageryExtent.North = imagerySouth + level.ImageryHeight - 1;

                UpdateOriginInTextures(level);
                UpdateImageryOriginInTextures(level);
            }

            _updater.ApplyNewData(context);

            for (int i = 0; i <_clipmapLevels.Length; ++i)
            {
                ClipmapLevel thisLevel = _clipmapLevels[i];
                ClipmapLevel coarserLevel = _clipmapLevels[i > 0 ? i - 1 : 0];

                _updater.RequestTileResidency(context, thisLevel);
                UpdateTerrain(thisLevel, coarserLevel, context, sceneState);
                UpdateImagery(thisLevel, coarserLevel, context, sceneState);
            }
        }

        private void UpdateOriginInTextures(ClipmapLevel level)
        {
            int deltaX = level.NextExtent.West - level.CurrentExtent.West;
            int deltaY = level.NextExtent.South - level.CurrentExtent.South;
            if (deltaX == 0 && deltaY == 0)
                return;

            if (level.CurrentExtent.West > level.CurrentExtent.East ||  // initial update
                Math.Abs(deltaX) >= _clipmapPosts || Math.Abs(deltaY) >= _clipmapPosts)      // complete update
            {
                level.OriginInTextures = new Vector2I(0, 0);
            }
            else
            {
                int newOriginX = (level.OriginInTextures.X + deltaX) % _clipmapPosts;
                if (newOriginX < 0)
                    newOriginX += _clipmapPosts;
                int newOriginY = (level.OriginInTextures.Y + deltaY) % _clipmapPosts;
                if (newOriginY < 0)
                    newOriginY += _clipmapPosts;
                level.OriginInTextures = new Vector2I(newOriginX, newOriginY);
            }
        }

        private void UpdateImageryOriginInTextures(ClipmapLevel level)
        {
            int deltaX = level.NextImageryExtent.West - level.CurrentImageryExtent.West;
            int deltaY = level.NextImageryExtent.South - level.CurrentImageryExtent.South;
            if (deltaX == 0 && deltaY == 0)
                return;

            if (level.CurrentImageryExtent.West > level.CurrentImageryExtent.East ||  // initial update
                Math.Abs(deltaX) >= level.ImageryWidth || Math.Abs(deltaY) >= level.ImageryHeight)      // complete update
            {
                level.OriginInImagery = new Vector2I(0, 0);
            }
            else
            {
                int newOriginX = (level.OriginInImagery.X + deltaX) % level.ImageryWidth;
                if (newOriginX < 0)
                    newOriginX += level.ImageryWidth;
                int newOriginY = (level.OriginInImagery.Y + deltaY) % level.ImageryHeight;
                if (newOriginY < 0)
                    newOriginY += level.ImageryHeight;
                level.OriginInImagery = new Vector2I(newOriginX, newOriginY);
            }
        }

        private void UpdateTerrain(ClipmapLevel level, ClipmapLevel coarserLevel, Context context, SceneState sceneState)
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

            if (level.CurrentExtent.West > level.CurrentExtent.East || // initial update
                width >= _clipmapPosts || height >= _clipmapPosts) // complete update
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
            }

            if (height > 0)
            {
                ClipmapUpdate horizontalUpdate = new ClipmapUpdate(
                    level,
                    level.NextExtent.West,
                    minLatitude,
                    level.NextExtent.East,
                    maxLatitude);
                _updater.UpdateTerrain(context, horizontalUpdate);
            }

            if (width > 0)
            {
                ClipmapUpdate verticalUpdate = new ClipmapUpdate(
                    level,
                    minLongitude,
                    level.NextExtent.South,
                    maxLongitude,
                    level.NextExtent.North);
                _updater.UpdateTerrain(context, verticalUpdate);
            }

            level.CurrentExtent.West = level.NextExtent.West;
            level.CurrentExtent.South = level.NextExtent.South;
            level.CurrentExtent.East = level.NextExtent.East;
            level.CurrentExtent.North = level.NextExtent.North;
        }

        private void UpdateImagery(ClipmapLevel level, ClipmapLevel coarserLevel, Context context, SceneState sceneState)
        {
            int deltaX = level.NextImageryExtent.West - level.CurrentImageryExtent.West;
            int deltaY = level.NextImageryExtent.South - level.CurrentImageryExtent.South;
            if (deltaX == 0 && deltaY == 0)
                return;

            int minLongitude = deltaX > 0 ? level.CurrentImageryExtent.East + 1 : level.NextImageryExtent.West;
            int maxLongitude = deltaX > 0 ? level.NextImageryExtent.East : level.CurrentImageryExtent.West - 1;
            int minLatitude = deltaY > 0 ? level.CurrentImageryExtent.North + 1 : level.NextImageryExtent.South;
            int maxLatitude = deltaY > 0 ? level.NextImageryExtent.North : level.CurrentImageryExtent.South - 1;

            int width = maxLongitude - minLongitude + 1;
            int height = maxLatitude - minLatitude + 1;

            if (level.CurrentImageryExtent.West > level.CurrentImageryExtent.East || // initial update
                width >= level.ImageryWidth || height >= level.ImageryHeight) // complete update
            {
                // Initial or complete update.
                width = level.ImageryWidth;
                height = level.ImageryHeight;
                deltaX = level.ImageryWidth;
                deltaY = level.ImageryHeight;
                minLongitude = level.NextImageryExtent.West;
                maxLongitude = level.NextImageryExtent.East;
                minLatitude = level.NextImageryExtent.South;
                maxLatitude = level.NextImageryExtent.North;
            }

            if (height > 0)
            {
                ClipmapUpdate horizontalUpdate = new ClipmapUpdate(
                    level,
                    level.NextImageryExtent.West,
                    minLatitude,
                    level.NextImageryExtent.East,
                    maxLatitude);
                _updater.UpdateImagery(context, horizontalUpdate);
            }

            if (width > 0)
            {
                ClipmapUpdate verticalUpdate = new ClipmapUpdate(
                    level,
                    minLongitude,
                    level.NextImageryExtent.South,
                    maxLongitude,
                    level.NextImageryExtent.North);
                _updater.UpdateImagery(context, verticalUpdate);
            }


            level.CurrentImageryExtent.West = level.NextImageryExtent.West;
            level.CurrentImageryExtent.South = level.NextImageryExtent.South;
            level.CurrentImageryExtent.East = level.NextImageryExtent.East;
            level.CurrentImageryExtent.North = level.NextImageryExtent.North;
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

            Vector3D previousTarget = sceneState.Camera.Target;
            Vector3D previousEye = sceneState.Camera.Eye;
            Vector3D previousSun = sceneState.SunPosition;

            _levelZeroWorldScaleFactor.Value = new Vector2F((float)_clipmapLevels[0].Terrain.PostDeltaLongitude, (float)_clipmapLevels[0].Terrain.PostDeltaLatitude);

            int maxLevel = _clipmapLevels.Length - 1;

            int longitudeIndex = (int)_clipmapLevels[0].Terrain.LongitudeToIndex(_clipmapCenter.Longitude);
            int latitudeIndex = (int)_clipmapLevels[0].Terrain.LatitudeToIndex(_clipmapCenter.Latitude);

            Vector2D center = new Vector2D(0.0, 0.0);

            bool rendered = false;
            for (int i = maxLevel; i >= 0; --i)
            {
                ClipmapLevel thisLevel = _clipmapLevels[i];
                ClipmapLevel coarserLevel = _clipmapLevels[i > 0 ? i - 1 : 0];

                rendered = RenderLevel(i, thisLevel, coarserLevel, !rendered, center, context, sceneState);
            }

            sceneState.Camera.Target = previousTarget;
            sceneState.Camera.Eye = previousEye;
            sceneState.SunPosition = previousSun;
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
            context.TextureUnits[4].Texture = level.ImageryTexture;
            context.TextureUnits[4].TextureSampler = Device.TextureSamplers.LinearRepeat;

            if (_colorClipmapLevels)
            {
                _color.Value = _colors[levelIndex];
                _blendRegionColor.Value = levelIndex == 0 ? _colors[levelIndex] : _colors[levelIndex - 1];
            }
            else
            {
                _color.Value = new Vector3F(0.0f, 1.0f, 0.0f);
                _blendRegionColor.Value = new Vector3F(0.0f, 0.0f, 1.0f);
            }

            int west = level.CurrentExtent.West;
            int south = level.CurrentExtent.South;
            int east = level.CurrentExtent.East;
            int north = level.CurrentExtent.North;

            float levelScaleFactor = (float)Math.Pow(2.0, -levelIndex);
            _levelScaleFactor.Value = new Vector2F(levelScaleFactor, levelScaleFactor);

            _levelOffsetFromWorldOrigin.Value = new Vector2F((float)((double)level.CurrentExtent.West - level.Terrain.LongitudeToIndex(0.0)),
                                                             (float)((double)level.CurrentExtent.South - level.Terrain.LatitudeToIndex(0.0)));

            int coarserWest = coarserLevel.CurrentExtent.West;
            int coarserSouth = coarserLevel.CurrentExtent.South;
            _fineLevelOriginInCoarse.Value = coarserLevel.OriginInTextures.ToVector2F() +
                                             new Vector2F(west / 2 - coarserWest + 0.5f,
                                                          south / 2 - coarserSouth + 0.5f);

            _viewPosInClippedLevel.Value = new Vector2F((float)(level.Terrain.LongitudeToIndex(Trig.ToDegrees(_clipmapCenter.Longitude)) - level.CurrentExtent.West),
                                                        (float)(level.Terrain.LatitudeToIndex(Trig.ToDegrees(_clipmapCenter.Latitude)) - level.CurrentExtent.South));

            _fineTextureOrigin.Value = level.OriginInTextures.ToVector2F() + new Vector2F(0.5f, 0.5f);

            _useBlendRegions.Value = _blendRegionsEnabled && level != coarserLevel;

            _terrainToImageryResolutionRatio.Value = new Vector2F((float)(level.Terrain.PostDeltaLongitude / level.Imagery.PostDeltaLongitude),
                                                                  (float)(level.Terrain.PostDeltaLatitude / level.Imagery.PostDeltaLatitude));

            double terrainWest = level.Terrain.IndexToLongitude(level.CurrentExtent.West);
            double terrainSouth = level.Terrain.IndexToLatitude(level.CurrentExtent.South);
            double imageryWestIndex = level.Imagery.LongitudeToIndex(terrainWest);
            double imagerySouthIndex = level.Imagery.LatitudeToIndex(terrainSouth);

            Vector2D terrainOffsetInImagery = level.OriginInImagery.ToVector2D() +
                                              new Vector2D(imageryWestIndex - level.CurrentImageryExtent.West,
                                                           imagerySouthIndex - level.CurrentImageryExtent.South);
            _terrainOffsetInImagery.Value = terrainOffsetInImagery.ToVector2F();
            _oneOverImagerySize.Value = new Vector2F((float)(1.0 / level.ImageryWidth), (float)(1.0 / level.ImageryHeight));

            DrawBlock(_fillPatch, level, coarserLevel, west, south, west, south, context, sceneState);
            DrawBlock(_fillPatch, level, coarserLevel, west, south, west + _fillPatchSegments, south, context, sceneState);
            DrawBlock(_fillPatch, level, coarserLevel, west, south, east - 2 * _fillPatchSegments, south, context, sceneState);
            DrawBlock(_fillPatch, level, coarserLevel, west, south, east - _fillPatchSegments, south, context, sceneState);

            DrawBlock(_fillPatch, level, coarserLevel, west, south, west, south + _fillPatchSegments, context, sceneState);
            DrawBlock(_fillPatch, level, coarserLevel, west, south, east - _fillPatchSegments, south + _fillPatchSegments, context, sceneState);

            DrawBlock(_fillPatch, level, coarserLevel, west, south, west, north - 2 * _fillPatchSegments, context, sceneState);
            DrawBlock(_fillPatch, level, coarserLevel, west, south, east - _fillPatchSegments, north - 2 * _fillPatchSegments, context, sceneState);

            DrawBlock(_fillPatch, level, coarserLevel, west, south, west, north - _fillPatchSegments, context, sceneState);
            DrawBlock(_fillPatch, level, coarserLevel, west, south, west + _fillPatchSegments, north - _fillPatchSegments, context, sceneState);
            DrawBlock(_fillPatch, level, coarserLevel, west, south, east - 2 * _fillPatchSegments, north - _fillPatchSegments, context, sceneState);
            DrawBlock(_fillPatch, level, coarserLevel, west, south, east - _fillPatchSegments, north - _fillPatchSegments, context, sceneState);

            DrawBlock(_horizontalFixupPatch, level, coarserLevel, west, south, west, south + 2 * _fillPatchSegments, context, sceneState);
            DrawBlock(_horizontalFixupPatch, level, coarserLevel, west, south, east - _fillPatchSegments, south + 2 * _fillPatchSegments, context, sceneState);

            DrawBlock(_verticalFixupPatch, level, coarserLevel, west, south, west + 2 * _fillPatchSegments, south, context, sceneState);
            DrawBlock(_verticalFixupPatch, level, coarserLevel, west, south, west + 2 * _fillPatchSegments, north - _fillPatchSegments, context, sceneState);

            DrawBlock(_degenerateTrianglePatch, level, coarserLevel, west, south, west, south, context, sceneState);

            // Fill the center of the highest-detail ring
            if (fillRing)
            {
                DrawBlock(_fillPatch, level, coarserLevel, west, south, west + _fillPatchSegments, south + _fillPatchSegments, context, sceneState);
                DrawBlock(_fillPatch, level, coarserLevel, west, south, west + 2 * _fillPatchPosts, south + _fillPatchSegments, context, sceneState);
                DrawBlock(_fillPatch, level, coarserLevel, west, south, west + _fillPatchSegments, south + 2 * _fillPatchPosts, context, sceneState);
                DrawBlock(_fillPatch, level, coarserLevel, west, south, west + 2 * _fillPatchPosts, south + 2 * _fillPatchPosts, context, sceneState);

                DrawBlock(_horizontalFixupPatch, level, coarserLevel, west, south, west + _fillPatchSegments, south + 2 * _fillPatchSegments, context, sceneState);
                DrawBlock(_horizontalFixupPatch, level, coarserLevel, west, south, west + 2 * _fillPatchPosts, south + 2 * _fillPatchSegments, context, sceneState);

                DrawBlock(_verticalFixupPatch, level, coarserLevel, west, south, west + 2 * _fillPatchSegments, south + _fillPatchSegments, context, sceneState);
                DrawBlock(_verticalFixupPatch, level, coarserLevel, west, south, west + 2 * _fillPatchSegments, south + 2 * _fillPatchPosts, context, sceneState);

                DrawBlock(_centerPatch, level, coarserLevel, west, south, west + 2 * _fillPatchSegments, south + 2 * _fillPatchSegments, context, sceneState);
            }
            else
            {
                int offset = level.OffsetStripOnNorth
                                ? north - _fillPatchPosts
                                : south + _fillPatchSegments;
                DrawBlock(_horizontalOffsetPatch, level, coarserLevel, west, south, west + _fillPatchSegments, offset, context, sceneState);

                int southOffset = level.OffsetStripOnNorth ? 0 : 1;
                offset = level.OffsetStripOnEast
                                ? east - _fillPatchPosts
                                : west + _fillPatchSegments;
                DrawBlock(_verticalOffsetPatch, level, coarserLevel, west, south, offset, south + _fillPatchSegments + southOffset, context, sceneState);
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

        private static Vector3F[] CreateColors()
        {
            int i = 0;
            Vector3F[] colors = new Vector3F[20];
            colors[i++] = new Vector3F(1.0f, 0.42f, 0.0f);
            colors[i++] = new Vector3F(0.0f, 0.58f, 1.0f);
            colors[i++] = new Vector3F(0.0f, 0.5f, 0.05f);
            colors[i++] = new Vector3F(0.7f, 0.0f, 1.0f);
            colors[i++] = new Vector3F(0.0f, 0.78f, 0.78f);
            colors[i++] = new Vector3F(1.0f, 0.85f, 0.0f);

            Random random = new Random();
            for (; i < colors.Length; ++i)
            {
                colors[i] = new Vector3F((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble());
            }
            return colors;
        }

        public void Dispose()
        {
            foreach (ClipmapLevel level in _clipmapLevels)
            {
                level.Dispose();
            }
            _shaderProgram.Dispose();
            _fillPatch.Dispose();
            _horizontalFixupPatch.Dispose();
            _verticalFixupPatch.Dispose();
            _horizontalOffsetPatch.Dispose();
            _verticalOffsetPatch.Dispose();
            _centerPatch.Dispose();
            _degenerateTrianglePatch.Dispose();
            _updater.Dispose();
        }

        private RasterSource _terrainSource;
        private int _clipmapPosts;
        private int _clipmapSegments;
        private ClipmapLevel[] _clipmapLevels;

        private ShaderProgram _shaderProgram;
        private RenderState _renderState;
        private PrimitiveType _primitiveType;

        private int _fillPatchPosts;
        private int _fillPatchSegments;

        private VertexArray _fillPatch;
        private VertexArray _horizontalFixupPatch;
        private VertexArray _verticalFixupPatch;
        private VertexArray _horizontalOffsetPatch;
        private VertexArray _verticalOffsetPatch;
        private VertexArray _centerPatch;
        private VertexArray _degenerateTrianglePatch;

        private Uniform<Vector2F> _patchOriginInClippedLevel;
        private Uniform<Vector2F> _levelScaleFactor;
        private Uniform<Vector2F> _levelZeroWorldScaleFactor;
        private Uniform<Vector2F> _levelOffsetFromWorldOrigin;
        private Uniform<float> _heightExaggeration;
        private Uniform<Vector2F> _fineLevelOriginInCoarse;
        private Uniform<Vector2F> _viewPosInClippedLevel;
        private Uniform<Vector2F> _unblendedRegionSize;
        private Uniform<Vector2F> _oneOverBlendedRegionSize;
        private Uniform<Vector2F> _fineTextureOrigin;
        private Uniform<bool> _showBlendRegions;
        private Uniform<bool> _useBlendRegions;
        private Uniform<float> _oneOverClipmapSize;
        private Uniform<Vector3F> _color;
        private Uniform<Vector3F> _blendRegionColor;
        private Uniform<Vector2F> _terrainToImageryResolutionRatio;
        private Uniform<Vector2F> _terrainOffsetInImagery;
        private Uniform<Vector2F> _oneOverImagerySize;
        private Uniform<bool> _showImagery;
        private Uniform<bool> _lighting;
        
        private bool _wireframe;
        private bool _blendRegionsEnabled = true;
        private bool _lodUpdateEnabled = true;
        private bool _colorClipmapLevels;

        private ClipmapUpdater _updater;
        private Geodetic3D _clipmapCenter;

        private static readonly Vector3F[] _colors = CreateColors();
        private Ellipsoid _ellipsoid;
    }
}
