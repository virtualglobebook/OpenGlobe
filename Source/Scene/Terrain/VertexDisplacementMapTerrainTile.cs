#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using MiniGlobe.Core;
using MiniGlobe.Core.Geometry;
using MiniGlobe.Core.Tessellation;
using MiniGlobe.Renderer;
using MiniGlobe.Scene;

namespace MiniGlobe.Terrain
{
    public enum TerrainNormalsAlgorithm
    {
        None,
        ThreeSamples,
        FourSamples,
        SobelFilter
    }

    public enum TerrainShadingAlgorithm
    {
        ColorMap,
        Solid,
        ByHeight,
        HeightContour,
        ColorRampByHeight,
        BlendRampByHeight,
        BySlope,
        SlopeContour,
        ColorRampBySlope,
        BlendRampBySlope,
        BlendMask
    }

    public sealed class VertexDisplacementMapTerrainTile : IDisposable
    {
        public VertexDisplacementMapTerrainTile(Context context, TerrainTile tile)
        {
            _context = context;

            //
            // Upload height map as a one channel floating point texture
            //
            WritePixelBuffer pixelBuffer = Device.CreateWritePixelBuffer(WritePixelBufferHint.StreamDraw,
                sizeof(float) * tile.Heights.Length);
            pixelBuffer.CopyFromSystemMemory(tile.Heights);

            _texture = Device.CreateTexture2DRectangle(new Texture2DDescription(
                tile.Size.X, tile.Size.Y, TextureFormat.Red32f));
            _texture.CopyFromBuffer(pixelBuffer, ImageFormat.Red, ImageDataType.Float);
            _texture.Filter = Texture2DFilter.NearestClampToEdge;
            
            ///////////////////////////////////////////////////////////////////

            _spTerrain = Device.CreateShaderProgram(
                EmbeddedResources.GetText("MiniGlobe.Scene.Terrain.VertexDisplacementMapTerrainTile.TerrainVS.glsl"),
                EmbeddedResources.GetText("MiniGlobe.Scene.Terrain.VertexDisplacementMapTerrainTile.TerrainFS.glsl"));
            _heightExaggerationUniform = _spTerrain.Uniforms["u_heightExaggeration"] as Uniform<float>;
            (_spTerrain.Uniforms["u_positionToTextureCoordinate"] as Uniform<Vector2S>).Value = new Vector2S(
                (float)(1.0 / (double)(tile.Size.X)), 
                (float)( 1.0 / (double)(tile.Size.Y)));
            (_spTerrain.Uniforms["u_positionToRepeatTextureCoordinate"] as Uniform<Vector2S>).Value = new Vector2S(
                (float)(4.0 / (double)tile.Size.X),
                (float)(4.0 / (double)tile.Size.Y));
            
            ///////////////////////////////////////////////////////////////////

            _spNormals = Device.CreateShaderProgram(
                EmbeddedResources.GetText("MiniGlobe.Scene.Terrain.VertexDisplacementMapTerrainTile.NormalsVS.glsl"),
                EmbeddedResources.GetText("MiniGlobe.Scene.Terrain.VertexDisplacementMapTerrainTile.NormalsGS.glsl"),
                EmbeddedResources.GetText("MiniGlobe.Scene.Terrain.VertexDisplacementMapTerrainTile.NormalsFS.glsl"));
            _heightExaggerationNormals = _spNormals.Uniforms["u_heightExaggeration"] as Uniform<float>;
            _fillDistanceNormals = _spNormals.Uniforms["u_fillDistance"] as Uniform<float>;
            (_spNormals.Uniforms["u_color"] as Uniform<Vector3S>).Value = Vector3S.Zero;

            ///////////////////////////////////////////////////////////////////

            _spWireframe = Device.CreateShaderProgram(
                EmbeddedResources.GetText("MiniGlobe.Scene.Terrain.VertexDisplacementMapTerrainTile.WireframeVS.glsl"),
                EmbeddedResources.GetText("MiniGlobe.Scene.Terrain.VertexDisplacementMapTerrainTile.WireframeGS.glsl"),
                EmbeddedResources.GetText("MiniGlobe.Scene.Terrain.VertexDisplacementMapTerrainTile.WireframeFS.glsl"));
            _lineWidthWireframe = _spWireframe.Uniforms["u_halfLineWidth"] as Uniform<float>;
            _heightExaggerationWireframe = _spWireframe.Uniforms["u_heightExaggeration"] as Uniform<float>;
            (_spWireframe.Uniforms["u_color"] as Uniform<Vector3S>).Value = Vector3S.Zero;
            
            ///////////////////////////////////////////////////////////////////

            Mesh mesh = RectangleTessellator.Compute(new RectangleD(new Vector2D(0.5, 0.5),
                new Vector2D((double)tile.Size.X - 0.5, (double)tile.Size.Y - 0.5)),
                tile.Size.X - 1, tile.Size.Y - 1);
            _va = _context.CreateVertexArray(mesh, _spWireframe.VertexAttributes, BufferHint.StaticDraw);
            _primitiveType = mesh.PrimitiveType;

            ///////////////////////////////////////////////////////////////////

            _rsTerrain = new RenderState();
            _rsTerrain.FacetCulling.FrontFaceWindingOrder = mesh.FrontFaceWindingOrder;

            _rsWireframe = new RenderState();
            _rsWireframe.Blending.Enabled = true;
            _rsWireframe.Blending.SourceRGBFactor = SourceBlendingFactor.SourceAlpha;
            _rsWireframe.Blending.SourceAlphaFactor = SourceBlendingFactor.SourceAlpha;
            _rsWireframe.Blending.DestinationRGBFactor = DestinationBlendingFactor.OneMinusSourceAlpha;
            _rsWireframe.Blending.DestinationAlphaFactor = DestinationBlendingFactor.OneMinusSourceAlpha;
            _rsWireframe.FacetCulling.FrontFaceWindingOrder = mesh.FrontFaceWindingOrder;
            _rsWireframe.DepthTest.Function = DepthTestFunction.LessThanOrEqual;

            _rsNormals = new RenderState();
            _rsNormals.FacetCulling.Enabled = false;
            _rsNormals.Blending.Enabled = true;
            _rsNormals.Blending.SourceRGBFactor = SourceBlendingFactor.SourceAlpha;
            _rsNormals.Blending.SourceAlphaFactor = SourceBlendingFactor.SourceAlpha;
            _rsNormals.Blending.DestinationRGBFactor = DestinationBlendingFactor.OneMinusSourceAlpha;
            _rsNormals.Blending.DestinationAlphaFactor = DestinationBlendingFactor.OneMinusSourceAlpha;

            ///////////////////////////////////////////////////////////////////

            _tileMinimumHeight = tile.MinimumHeight;
            _tileMaximumHeight = tile.MaximumHeight;

            _heightExaggeration = 1;
            _normalsAlgorithm = TerrainNormalsAlgorithm.ThreeSamples;
            _showTerrain = true;
            _dirty = true;
        }

        private void Update(SceneState sceneState)
        {
            if (_dirty)
            {
                (_spTerrain.Uniforms["u_normalAlgorithm"] as Uniform<int>).Value = (int)_normalsAlgorithm;
                (_spTerrain.Uniforms["u_shadingAlgorithm"] as Uniform<int>).Value = (int)_shadingAlgorithm;
                (_spTerrain.Uniforms["u_showTerrain"] as Uniform<bool>).Value = _showTerrain;
                (_spTerrain.Uniforms["u_showSilhouette"] as Uniform<bool>).Value = _showSilhouette;
                (_spNormals.Uniforms["u_normalAlgorithm"] as Uniform<int>).Value = (int)_normalsAlgorithm;

                _minimumHeight = _spTerrain.Uniforms["u_minimumHeight"] as Uniform<float>;
                _maximumHeight = _spTerrain.Uniforms["u_maximumHeight"] as Uniform<float>;

                _dirty = false;
            }

            _heightExaggerationUniform.Value = _heightExaggeration;
            _minimumHeight.Value = _tileMinimumHeight * _heightExaggeration;
            _maximumHeight.Value = _tileMaximumHeight * _heightExaggeration;

            _heightExaggerationWireframe.Value = _heightExaggeration;
            _lineWidthWireframe.Value = (float)(0.5 * 3.0 * sceneState.HighResolutionSnapScale);

            _heightExaggerationNormals.Value = _heightExaggeration;
            _fillDistanceNormals.Value = (float)(0.5 * 3.0 * sceneState.HighResolutionSnapScale);
        }

        public void Render(SceneState sceneState)
        {
            Verify.ThrowInvalidOperationIfNull(ColorMapTexture, "ColorMap");
            Verify.ThrowInvalidOperationIfNull(ColorRampHeightTexture, "ColorRampTexture");
            Verify.ThrowInvalidOperationIfNull(ColorRampSlopeTexture, "ColorRampSlopeTexture");
            Verify.ThrowInvalidOperationIfNull(BlendRampTexture, "BlendRampTexture");
            Verify.ThrowInvalidOperationIfNull(GrassTexture, "GrassTexture");
            Verify.ThrowInvalidOperationIfNull(StoneTexture, "StoneTexture");
            Verify.ThrowInvalidOperationIfNull(BlendMaskTexture, "BlendMaskTexture");
            
            if (ShowTerrain || ShowSilhouette || ShowWireframe || ShowNormals)
            {
                Update(sceneState);

                GrassTexture.Filter = Texture2DFilter.LinearRepeat;
                StoneTexture.Filter = Texture2DFilter.LinearRepeat;

                _context.TextureUnits[0].Texture2DRectangle = _texture;
                _context.TextureUnits[6].Texture2D = ColorMapTexture;
                _context.TextureUnits[1].Texture2D = ColorRampHeightTexture;
                _context.TextureUnits[7].Texture2D = ColorRampSlopeTexture;
                _context.TextureUnits[2].Texture2D = BlendRampTexture;
                _context.TextureUnits[3].Texture2D = GrassTexture;
                _context.TextureUnits[4].Texture2D = StoneTexture;
                _context.TextureUnits[5].Texture2D = BlendMaskTexture;

                _context.Bind(_va);

                if (ShowTerrain || ShowSilhouette)
                {
                    _context.Bind(_spTerrain);
                    _context.Bind(_rsTerrain);
                    _context.Draw(_primitiveType, sceneState);
                }

                if (ShowWireframe)
                {
                    _context.Bind(_spWireframe);
                    _context.Bind(_rsWireframe);
                    _context.Draw(_primitiveType, sceneState);
                }

                if (ShowNormals && (_normalsAlgorithm != TerrainNormalsAlgorithm.None))
                {
                    _context.Bind(_spNormals);
                    _context.Bind(_rsNormals);
                    _context.Draw(PrimitiveType.Points, sceneState);
                }
            }
        }

        public float HeightExaggeration
        {
            get { return _heightExaggeration; }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException("HeightExaggeration", "HeightExaggeration must be greater than zero.");
                }

                //
                // TEXEL_SPACE_TODO:  If one of the AABB z planes is not 0, the
                // scale will incorrectly move the entire tile.
                //
                _heightExaggeration = value;
            }
        }

        public TerrainNormalsAlgorithm NormalsAlgorithm 
        {
            get { return _normalsAlgorithm; }
            set
            {
                if (_normalsAlgorithm != value)
                {
                    _normalsAlgorithm = value;
                    _dirty = true;
                }
            }
        }

        public TerrainShadingAlgorithm ShadingAlgorithm
        {
            get { return _shadingAlgorithm; }
            set
            {
                if (_shadingAlgorithm != value)
                {
                    _shadingAlgorithm = value;
                    _dirty = true;
                }
            }
        }

        public bool ShowTerrain
        {
            get { return _showTerrain; }
            set
            {
                if (_showTerrain != value)
                {
                    _showTerrain = value;
                    _dirty = true;
                }
            }
        }

        public bool ShowSilhouette 
        {
            get { return _showSilhouette; }
            set
            {
                if (_showSilhouette != value)
                {
                    _showSilhouette = value;
                    _dirty = true;
                }
            }
        }

        public bool ShowWireframe { get; set; }
        public bool ShowNormals { get; set; }
        public Texture2D ColorMapTexture { get; set; }
        public Texture2D ColorRampHeightTexture { get; set; }
        public Texture2D ColorRampSlopeTexture { get; set; }
        public Texture2D BlendRampTexture { get; set; }
        public Texture2D GrassTexture { get; set; }
        public Texture2D StoneTexture { get; set; }
        public Texture2D BlendMaskTexture { get; set; }

        #region IDisposable Members

        public void Dispose()
        {
            _spTerrain.Dispose();
            _spWireframe.Dispose();
            _spNormals.Dispose();

            _va.Dispose();
            _texture.Dispose();
        }

        #endregion

        private readonly Context _context;

        private readonly RenderState _rsTerrain;
        private readonly ShaderProgram _spTerrain;
        private readonly Uniform<float> _heightExaggerationUniform;
        private Uniform<float> _minimumHeight;
        private Uniform<float> _maximumHeight;

        private readonly RenderState _rsWireframe;
        private readonly ShaderProgram _spWireframe;
        private readonly Uniform<float> _heightExaggerationWireframe;
        private readonly Uniform<float> _lineWidthWireframe;

        private readonly RenderState _rsNormals;
        private readonly ShaderProgram _spNormals;
        private readonly Uniform<float> _heightExaggerationNormals;
        private readonly Uniform<float> _fillDistanceNormals;

        private readonly Texture2D _texture;
        private readonly VertexArray _va;
        private readonly PrimitiveType _primitiveType;

        private readonly float _tileMinimumHeight;
        private readonly float _tileMaximumHeight;

        private float _heightExaggeration;
        private TerrainNormalsAlgorithm _normalsAlgorithm;
        private TerrainShadingAlgorithm _shadingAlgorithm;
        private bool _showTerrain;
        private bool _showSilhouette;
        private bool _dirty;
    }
}