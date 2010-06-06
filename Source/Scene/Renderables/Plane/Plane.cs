#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Drawing;
using MiniGlobe.Core;
using MiniGlobe.Core.Geometry;
using MiniGlobe.Renderer;

namespace MiniGlobe.Scene
{
    public sealed class Plane : IDisposable
    {
        public Plane(Context context)
        {
            Verify.ThrowIfNull(context);

            RenderState lineRS = new RenderState();
            lineRS.FacetCulling.Enabled = false;

            ShaderProgram lineSP = Device.CreateShaderProgram(
                EmbeddedResources.GetText("MiniGlobe.Scene.Renderables.Plane.Shaders.LineVS.glsl"),
                EmbeddedResources.GetText("MiniGlobe.Scene.Renderables.Plane.Shaders.LineGS.glsl"),
                EmbeddedResources.GetText("MiniGlobe.Scene.Renderables.Plane.Shaders.LineFS.glsl"));
            _lineLogarithmicDepth = lineSP.Uniforms["u_logarithmicDepth"] as Uniform<bool>;
            _lineLogarithmicDepthConstant = lineSP.Uniforms["u_logarithmicDepthConstant"] as Uniform<float>;
            _lineFillDistance = lineSP.Uniforms["u_fillDistance"] as Uniform<float>;
            _lineColorUniform = lineSP.Uniforms["u_color"] as Uniform<Vector3S>;

            OutlineWidth = 1;
            OutlineColor = Color.Gray;

            ///////////////////////////////////////////////////////////////////

            RenderState fillRS = new RenderState();
            fillRS.FacetCulling.Enabled = false;
            fillRS.Blending.Enabled = true;
            fillRS.Blending.SourceRGBFactor = SourceBlendingFactor.SourceAlpha;
            fillRS.Blending.SourceAlphaFactor = SourceBlendingFactor.SourceAlpha;
            fillRS.Blending.DestinationRGBFactor = DestinationBlendingFactor.OneMinusSourceAlpha;
            fillRS.Blending.DestinationAlphaFactor = DestinationBlendingFactor.OneMinusSourceAlpha;
            
            ShaderProgram fillSP = Device.CreateShaderProgram(
                EmbeddedResources.GetText("MiniGlobe.Scene.Renderables.Plane.Shaders.FillVS.glsl"),
                EmbeddedResources.GetText("MiniGlobe.Scene.Renderables.Plane.Shaders.FillFS.glsl"));
            _fillLogarithmicDepth = fillSP.Uniforms["u_logarithmicDepth"] as Uniform<bool>;
            _fillLogarithmicDepthConstant = fillSP.Uniforms["u_logarithmicDepthConstant"] as Uniform<float>;
            _fillColorUniform = fillSP.Uniforms["u_color"] as Uniform<Vector3S>;
            _fillAlphaUniform = fillSP.Uniforms["u_alpha"] as Uniform<float>;

            LogarithmicDepthConstant = 1;
            FillColor = Color.Gray;
            FillTranslucency = 0.5f;

            ///////////////////////////////////////////////////////////////////

            _positionBuffer = Device.CreateVertexBuffer(BufferHint.StaticDraw, 4 * SizeInBytes<Vector3S>.Value);

            ushort[] indices = new ushort[] 
            { 
                0, 1, 2, 3,                             // Line loop
                0, 1, 2, 0, 2, 3                        // Triangles
            };
            IndexBuffer indexBuffer = Device.CreateIndexBuffer(BufferHint.StaticDraw, indices.Length * sizeof(ushort));
            indexBuffer.CopyFromSystemMemory(indices);

            AttachedVertexBuffer attachedPositionBuffer = new AttachedVertexBuffer(
                _positionBuffer, VertexAttributeComponentType.Float, 3);
            _va = context.CreateVertexArray();
            _va.VertexBuffers[lineSP.VertexAttributes["position"].Location] = attachedPositionBuffer;
            _va.IndexBuffer = indexBuffer;

            ShowOutline = true;
            ShowFill = true;

            ///////////////////////////////////////////////////////////////////

            _drawStateLine = new DrawState(lineRS, lineSP, _va);
            _drawStateFill = new DrawState(fillRS, fillSP, _va);

            Origin = Vector3D.Zero;
            XAxis = Vector3D.UnitX;
            YAxis = Vector3D.UnitY;
        }

        private void Update()
        {
            if (_dirty)
            {
                Vector3S[] positions = new Vector3S[] 
                { 
                    (_origin - _xAxis - _yAxis).ToVector3S(), 
                    (_origin + _xAxis - _yAxis).ToVector3S(), 
                    (_origin + _xAxis + _yAxis).ToVector3S(), 
                    (_origin - _xAxis + _yAxis).ToVector3S()
                };
                _positionBuffer.CopyFromSystemMemory(positions);

                _dirty = false;
            }
        }

        public void Render(Context context, SceneState sceneState)
        {
            Verify.ThrowIfNull(context);
            Verify.ThrowIfNull(sceneState);

            Update();

            if (ShowOutline)
            {
                //
                // Pass 1:  Outline
                //
                _lineFillDistance.Value = (float)(OutlineWidth * 0.5 * sceneState.HighResolutionSnapScale);
                context.Draw(PrimitiveType.LineLoop, 0, 4, _drawStateLine, sceneState);
            }

            if (ShowFill)
            {
                //
                // Pass 2:  Fill
                //
                context.Draw(PrimitiveType.Triangles, 4, 6, _drawStateFill, sceneState);
            }
        }

        public DepthTestFunction DepthTestFunction
        {
            get { return _drawStateLine.RenderState.DepthTest.Function; }
            set 
            {
                _drawStateLine.RenderState.DepthTest.Function = value;
                _drawStateFill.RenderState.DepthTest.Function = value; 
            }
        }

        public Vector3D Origin
        {
            get { return _origin; }

            set
            {
                if (_origin != value)
                {
                    _origin = value;
                    _dirty = true;
                }
            }
        }

        public Vector3D XAxis
        {
            get { return _xAxis; }

            set
            {
                if (_xAxis != value)
                {
                    _xAxis = value;
                    _dirty = true;
                }
            }
        }

        public Vector3D YAxis
        {
            get { return _yAxis; }

            set
            {
                if (_yAxis != value)
                {
                    _yAxis = value;
                    _dirty = true;
                }
            }
        }

        public double OutlineWidth { get; set; }
        public bool ShowOutline { get; set; }
        public bool ShowFill { get; set; }

        public bool LogarithmicDepth
        {
            get { return _lineLogarithmicDepth.Value; }
            set
            {
                _lineLogarithmicDepth.Value = value;
                _fillLogarithmicDepth.Value = value;
            }
        }

        public float LogarithmicDepthConstant
        {
            get { return _lineLogarithmicDepthConstant.Value; }
            set
            {
                _lineLogarithmicDepthConstant.Value = value;
                _fillLogarithmicDepthConstant.Value = value;
            }
        }

        public Color OutlineColor
        {
            get { return _lineColor; }

            set
            {
                _lineColor = value;
                _lineColorUniform.Value = new Vector3S(value.R / 255.0f, value.G / 255.0f, value.B / 255.0f);
            }
        }

        public Color FillColor
        {
            get { return _fillColor; }

            set
            {
                _fillColor = value;
                _fillColorUniform.Value = new Vector3S(value.R / 255.0f, value.G / 255.0f, value.B / 255.0f);
            }
        }

        public float FillTranslucency
        {
            get { return _fillTranslucency; }

            set
            {
                _fillTranslucency = value;
                _fillAlphaUniform.Value = 1.0f - value;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            _drawStateLine.ShaderProgram.Dispose();
            _drawStateFill.ShaderProgram.Dispose();
            _positionBuffer.Dispose();
            _va.Dispose();
        }

        #endregion

        private readonly DrawState _drawStateLine;
        private readonly Uniform<bool> _lineLogarithmicDepth;
        private readonly Uniform<float> _lineLogarithmicDepthConstant;

        private readonly Uniform<float> _lineFillDistance;
        private readonly Uniform<Vector3S> _lineColorUniform;
        private Color _lineColor;

        private readonly DrawState _drawStateFill;
        private readonly Uniform<bool> _fillLogarithmicDepth;
        private readonly Uniform<float> _fillLogarithmicDepthConstant;

        private readonly VertexBuffer _positionBuffer;
        private readonly VertexArray _va;

        private bool _dirty;
        private Vector3D _origin;
        private Vector3D _xAxis;
        private Vector3D _yAxis;

        private readonly Uniform<Vector3S> _fillColorUniform;
        private Color _fillColor;
        private readonly Uniform<float> _fillAlphaUniform;
        private float _fillTranslucency;
    }
}