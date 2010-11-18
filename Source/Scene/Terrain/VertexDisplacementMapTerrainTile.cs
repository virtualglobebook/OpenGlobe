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
using OpenGlobe.Core;
using OpenGlobe.Renderer;
using OpenGlobe.Scene;

namespace OpenGlobe.Terrain
{
    public enum TerrainNormalsAlgorithm
    {
        None,
        ForwardDifference,
        CentralDifference,
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
            //
            // Upload height map as a one channel floating point texture
            //
            WritePixelBuffer pixelBuffer = Device.CreateWritePixelBuffer(PixelBufferHint.Stream,
                sizeof(float) * tile.Heights.Length);
            pixelBuffer.CopyFromSystemMemory(tile.Heights);

            _texture = Device.CreateTexture2DRectangle(new Texture2DDescription(
                tile.Resolution.X, tile.Resolution.Y, TextureFormat.Red32f));
            _texture.CopyFromBuffer(pixelBuffer, ImageFormat.Red, ImageDatatype.Float);
            
            ///////////////////////////////////////////////////////////////////

            ShaderProgram spTerrain = Device.CreateShaderProgram(
                EmbeddedResources.GetText("OpenGlobe.Scene.Terrain.VertexDisplacementMapTerrainTile.TerrainVS.glsl"),
                EmbeddedResources.GetText("OpenGlobe.Scene.Terrain.VertexDisplacementMapTerrainTile.TerrainFS.glsl"));
            _heightExaggerationUniform = (Uniform<float>)spTerrain.Uniforms["u_heightExaggeration"];
            ((Uniform<Vector2F>)spTerrain.Uniforms["u_positionToTextureCoordinate"]).Value = new Vector2F(
                (float)(1.0 / (double)(tile.Resolution.X)), 
                (float)( 1.0 / (double)(tile.Resolution.Y)));
            ((Uniform<Vector2F>)spTerrain.Uniforms["u_positionToRepeatTextureCoordinate"]).Value = new Vector2F(
                (float)(4.0 / (double)tile.Resolution.X),
                (float)(4.0 / (double)tile.Resolution.Y));
            
            ///////////////////////////////////////////////////////////////////

            ShaderProgram spNormals = Device.CreateShaderProgram(
                EmbeddedResources.GetText("OpenGlobe.Scene.Terrain.VertexDisplacementMapTerrainTile.NormalsVS.glsl"),
                EmbeddedResources.GetText("OpenGlobe.Scene.Terrain.VertexDisplacementMapTerrainTile.NormalsGS.glsl"),
                EmbeddedResources.GetText("OpenGlobe.Scene.Terrain.VertexDisplacementMapTerrainTile.NormalsFS.glsl"));
            _heightExaggerationNormals = (Uniform<float>)spNormals.Uniforms["u_heightExaggeration"];
            _fillDistanceNormals = (Uniform<float>)spNormals.Uniforms["u_fillDistance"];
            ((Uniform<Vector3F>)spNormals.Uniforms["u_color"]).Value = Vector3F.Zero;

            ///////////////////////////////////////////////////////////////////

            ShaderProgram spWireframe = Device.CreateShaderProgram(
                EmbeddedResources.GetText("OpenGlobe.Scene.Terrain.VertexDisplacementMapTerrainTile.WireframeVS.glsl"),
                EmbeddedResources.GetText("OpenGlobe.Scene.Terrain.VertexDisplacementMapTerrainTile.WireframeGS.glsl"),
                EmbeddedResources.GetText("OpenGlobe.Scene.Terrain.VertexDisplacementMapTerrainTile.WireframeFS.glsl"));
            _lineWidthWireframe = (Uniform<float>)spWireframe.Uniforms["u_halfLineWidth"];
            _heightExaggerationWireframe = (Uniform<float>)spWireframe.Uniforms["u_heightExaggeration"];
            ((Uniform<Vector3F>)spWireframe.Uniforms["u_color"]).Value = Vector3F.Zero;
            
            ///////////////////////////////////////////////////////////////////

            Mesh mesh = RectangleTessellator.Compute(new RectangleD(new Vector2D(0.5, 0.5),
                new Vector2D((double)tile.Resolution.X - 0.5, (double)tile.Resolution.Y - 0.5)),
                tile.Resolution.X - 1, tile.Resolution.Y - 1);
            _va = context.CreateVertexArray(mesh, spWireframe.VertexAttributes, BufferHint.StaticDraw);
            _primitiveType = mesh.PrimitiveType;

            ///////////////////////////////////////////////////////////////////

            RenderState rsTerrain = new RenderState();
            rsTerrain.FacetCulling.FrontFaceWindingOrder = mesh.FrontFaceWindingOrder;

            RenderState rsWireframe = new RenderState();
            rsWireframe.Blending.Enabled = true;
            rsWireframe.Blending.SourceRGBFactor = SourceBlendingFactor.SourceAlpha;
            rsWireframe.Blending.SourceAlphaFactor = SourceBlendingFactor.SourceAlpha;
            rsWireframe.Blending.DestinationRGBFactor = DestinationBlendingFactor.OneMinusSourceAlpha;
            rsWireframe.Blending.DestinationAlphaFactor = DestinationBlendingFactor.OneMinusSourceAlpha;
            rsWireframe.FacetCulling.FrontFaceWindingOrder = mesh.FrontFaceWindingOrder;
            rsWireframe.DepthTest.Function = DepthTestFunction.LessThanOrEqual;

            RenderState rsNormals = new RenderState();
            rsNormals.FacetCulling.Enabled = false;
            rsNormals.Blending.Enabled = true;
            rsNormals.Blending.SourceRGBFactor = SourceBlendingFactor.SourceAlpha;
            rsNormals.Blending.SourceAlphaFactor = SourceBlendingFactor.SourceAlpha;
            rsNormals.Blending.DestinationRGBFactor = DestinationBlendingFactor.OneMinusSourceAlpha;
            rsNormals.Blending.DestinationAlphaFactor = DestinationBlendingFactor.OneMinusSourceAlpha;

            ///////////////////////////////////////////////////////////////////

            _drawStateTerrain = new DrawState(rsTerrain, spTerrain, _va);
            _drawStateWireframe = new DrawState(rsWireframe, spWireframe, _va);
            _drawStateNormals = new DrawState(rsNormals, spNormals, _va);

            _tileMinimumHeight = tile.MinimumHeight;
            _tileMaximumHeight = tile.MaximumHeight;

            _heightExaggeration = 1;
            _normalsAlgorithm = TerrainNormalsAlgorithm.ForwardDifference;
            _showTerrain = true;
            _dirty = true;
        }

        private void Update(SceneState sceneState)
        {
            if (_dirty)
            {
                ShaderProgram sp = _drawStateTerrain.ShaderProgram;

                ((Uniform<int>)sp.Uniforms["u_normalAlgorithm"]).Value = (int)_normalsAlgorithm;
                ((Uniform<int>)sp.Uniforms["u_shadingAlgorithm"] ).Value = (int)_shadingAlgorithm;
                ((Uniform<bool>)sp.Uniforms["u_showTerrain"]).Value = _showTerrain;
                ((Uniform<bool>)sp.Uniforms["u_showSilhouette"]).Value = _showSilhouette;
                ((Uniform<int>)sp.Uniforms["u_normalAlgorithm"]).Value = (int)_normalsAlgorithm;

                _minimumHeight = (Uniform<float>)sp.Uniforms["u_minimumHeight"];
                _maximumHeight = (Uniform<float>)sp.Uniforms["u_maximumHeight"];

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

        public void Render(Context context, SceneState sceneState)
        {
            Verify.ThrowIfNull(context);
            Verify.ThrowIfNull(sceneState);
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

                context.TextureUnits[0].Texture = _texture;
                context.TextureUnits[0].TextureSampler = Device.TextureSamplers.NearestClamp;

                context.TextureUnits[6].Texture = ColorMapTexture;
                context.TextureUnits[6].TextureSampler = Device.TextureSamplers.LinearClamp;

                context.TextureUnits[1].Texture = ColorRampHeightTexture;
                context.TextureUnits[1].TextureSampler = Device.TextureSamplers.LinearClamp;

                context.TextureUnits[7].Texture = ColorRampSlopeTexture;
                context.TextureUnits[7].TextureSampler = Device.TextureSamplers.LinearClamp;

                context.TextureUnits[2].Texture = BlendRampTexture;
                context.TextureUnits[2].TextureSampler = Device.TextureSamplers.LinearClamp;

                context.TextureUnits[3].Texture = GrassTexture;
                context.TextureUnits[3].TextureSampler = Device.TextureSamplers.LinearRepeat;

                context.TextureUnits[4].Texture = StoneTexture;
                context.TextureUnits[4].TextureSampler = Device.TextureSamplers.LinearRepeat;

                context.TextureUnits[5].Texture = BlendMaskTexture;
                context.TextureUnits[5].TextureSampler = Device.TextureSamplers.LinearClamp;

                if (ShowTerrain || ShowSilhouette)
                {
                    context.Draw(_primitiveType, _drawStateTerrain, sceneState);
                }

                if (ShowWireframe)
                {
                    context.Draw(_primitiveType, _drawStateWireframe, sceneState);
                }

                if (ShowNormals && (_normalsAlgorithm != TerrainNormalsAlgorithm.None))
                {
                    context.Draw(PrimitiveType.Points, _drawStateNormals, sceneState);
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
            _drawStateTerrain.ShaderProgram.Dispose();
            _drawStateWireframe.ShaderProgram.Dispose();
            _drawStateNormals.ShaderProgram.Dispose();

            _va.Dispose();
            _texture.Dispose();
        }

        #endregion

        private readonly DrawState _drawStateTerrain;
        private readonly Uniform<float> _heightExaggerationUniform;
        private Uniform<float> _minimumHeight;
        private Uniform<float> _maximumHeight;

        private readonly DrawState _drawStateWireframe;
        private readonly Uniform<float> _heightExaggerationWireframe;
        private readonly Uniform<float> _lineWidthWireframe;

        private readonly DrawState _drawStateNormals;
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