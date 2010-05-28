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
    public sealed class HeadsUpDisplay : IDisposable
    {
        public HeadsUpDisplay(Context context)
        {
            Verify.ThrowIfNull(context);

            _renderState = new RenderState();
            _renderState.FacetCulling.Enabled = false;
            _renderState.DepthTest.Enabled = false;
            _renderState.Blending.Enabled = true;
            _renderState.Blending.SourceRGBFactor = SourceBlendingFactor.SourceAlpha;
            _renderState.Blending.SourceAlphaFactor = SourceBlendingFactor.SourceAlpha;
            _renderState.Blending.DestinationRGBFactor = DestinationBlendingFactor.OneMinusSourceAlpha;
            _renderState.Blending.DestinationAlphaFactor = DestinationBlendingFactor.OneMinusSourceAlpha;

            _sp = Device.CreateShaderProgram(
                EmbeddedResources.GetText("MiniGlobe.Scene.Renderables.HeadsUpDisplay.Shaders.HeadsUpDisplayVS.glsl"),
                EmbeddedResources.GetText("MiniGlobe.Scene.Renderables.HeadsUpDisplay.Shaders.HeadsUpDisplayGS.glsl"),
                EmbeddedResources.GetText("MiniGlobe.Scene.Renderables.HeadsUpDisplay.Shaders.HeadsUpDisplayFS.glsl"));
            _colorUniform = _sp.Uniforms["u_color"] as Uniform<Vector3S>;
            _originScaleUniform = _sp.Uniforms["u_originScale"] as Uniform<Vector2S>;
            
            ///////////////////////////////////////////////////////////////////

            Color = Color.White;
            HorizontalOrigin = HorizontalOrigin.Left;
            VerticalOrigin = VerticalOrigin.Bottom;
            _positionDirty = true;
        }

        private void CreateVertexArray(Context context)
        {
            // TODO:  Hint per buffer?  One hint?
            _positionBuffer = Device.CreateVertexBuffer(BufferHint.StaticDraw, SizeInBytes<Vector2S>.Value);

            AttachedVertexBuffer attachedPositionBuffer = new AttachedVertexBuffer(
                _positionBuffer, VertexAttributeComponentType.Float, 2);

            _va = context.CreateVertexArray();
            _va.VertexBuffers[_sp.VertexAttributes["position"].Location] = attachedPositionBuffer;
         }

        private void Update(Context context)
        {
            if (_positionDirty)
            {
                DisposeVertexArray();
                CreateVertexArray(context);

                Vector2S[] positions = new Vector2S[] { _position.ToVector2S() };
                _positionBuffer.CopyFromSystemMemory(positions);

                _positionDirty = false;
            }
        }

        public void Render(Context context, SceneState sceneState)
        {
            Verify.ThrowIfNull(context);
            Verify.ThrowIfNull(sceneState);
            Verify.ThrowInvalidOperationIfNull(Texture, "Texture");

            Update(context);

            if (_va != null)
            {
                context.TextureUnits[0].Texture2D = Texture;
                context.Bind(_renderState);
                context.Bind(_sp);
                context.Bind(_va);
                context.Draw(PrimitiveType.Points, sceneState);
            }
        }

        public Texture2D Texture { get; set; }

        public Color Color
        {
            get { return _color; }

            set
            {
                _color = value;
                _colorUniform.Value = new Vector3S(_color.R / 255.0f, _color.G / 255.0f, _color.B / 255.0f);
            }
        }

        public HorizontalOrigin HorizontalOrigin
        {
            get { return _horizontalOrigin; }
            set
            {
                _horizontalOrigin = value;
                _originScaleUniform.Value = new Vector2S(
                    _originScale[(int)value],
                    _originScaleUniform.Value.Y);
            }
        }

        public VerticalOrigin VerticalOrigin
        {
            get { return _verticalOrigin; }
            set
            {
                _verticalOrigin = value;
                _originScaleUniform.Value = new Vector2S(
                    _originScaleUniform.Value.X,
                    _originScale[(int)value]);
            }
        }

        public Vector2D Position
        {
            get { return _position; }

            set
            {
                if (_position != value)
                {
                    _position = value;
                    _positionDirty = true;
                }
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            _sp.Dispose();
            DisposeVertexArray();
        }

        #endregion

        private void DisposeVertexArray()
        {
            if (_positionBuffer != null)
            {
                _positionBuffer.Dispose();
                _positionBuffer = null;
            }

            if (_va != null)
            {
                _va.Dispose();
                _va = null;
            }
        }

        private readonly RenderState _renderState;
        private readonly ShaderProgram _sp;
        private readonly Uniform<Vector3S> _colorUniform;
        private readonly Uniform<Vector2S> _originScaleUniform;
        private Color _color;

        private Vector2D _position;
        private bool _positionDirty;
        private HorizontalOrigin _horizontalOrigin;
        private VerticalOrigin _verticalOrigin;

        private VertexBuffer _positionBuffer;
        private VertexArray _va;

        private static readonly Half[] _originScale = new Half[] { new Half(0.0), new Half(1.0), new Half(-1.0) };
    }
}