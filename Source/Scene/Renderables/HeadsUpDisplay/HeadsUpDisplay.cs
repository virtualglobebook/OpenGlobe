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
using OpenGlobe.Core;
using OpenGlobe.Core.Geometry;
using OpenGlobe.Renderer;

namespace OpenGlobe.Scene
{
    public sealed class HeadsUpDisplay : IDisposable
    {
        public HeadsUpDisplay(Context context)
        {
            Verify.ThrowIfNull(context);

            RenderState renderState = new RenderState();
            renderState.FacetCulling.Enabled = false;
            renderState.DepthTest.Enabled = false;
            renderState.Blending.Enabled = true;
            renderState.Blending.SourceRGBFactor = SourceBlendingFactor.SourceAlpha;
            renderState.Blending.SourceAlphaFactor = SourceBlendingFactor.SourceAlpha;
            renderState.Blending.DestinationRGBFactor = DestinationBlendingFactor.OneMinusSourceAlpha;
            renderState.Blending.DestinationAlphaFactor = DestinationBlendingFactor.OneMinusSourceAlpha;

            ShaderProgram sp = Device.CreateShaderProgram(
                EmbeddedResources.GetText("OpenGlobe.Scene.Renderables.HeadsUpDisplay.Shaders.HeadsUpDisplayVS.glsl"),
                EmbeddedResources.GetText("OpenGlobe.Scene.Renderables.HeadsUpDisplay.Shaders.HeadsUpDisplayGS.glsl"),
                EmbeddedResources.GetText("OpenGlobe.Scene.Renderables.HeadsUpDisplay.Shaders.HeadsUpDisplayFS.glsl"));
            _colorUniform = sp.Uniforms["u_color"] as Uniform<Vector3S>;
            _originScaleUniform = sp.Uniforms["u_originScale"] as Uniform<Vector2S>;

            _drawState = new DrawState(renderState, sp, null);

            Color = Color.White;
            HorizontalOrigin = HorizontalOrigin.Left;
            VerticalOrigin = VerticalOrigin.Bottom;
            _positionDirty = true;
        }

        private void CreateVertexArray(Context context)
        {
            // TODO:  Hint per buffer?  One hint?
            _positionBuffer = Device.CreateVertexBuffer(BufferHint.StaticDraw, SizeInBytes<Vector2S>.Value);

            VertexBufferAttribute positionAttribute = new VertexBufferAttribute(
                _positionBuffer, ComponentDatatype.Float, 2);

            _drawState.VertexArray = context.CreateVertexArray();
            _drawState.VertexArray.Attributes[_drawState.ShaderProgram.VertexAttributes["position"].Location] = positionAttribute;
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
            Verify.ThrowInvalidOperationIfNull(Texture, "Texture");

            Update(context);

            if (_drawState.VertexArray != null)
            {
                context.TextureUnits[0].Texture = Texture;
                context.TextureUnits[0].TextureSampler = Device.TextureSamplers.LinearClampToEdge;
                context.Draw(PrimitiveType.Points, _drawState, sceneState);
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
            _drawState.ShaderProgram.Dispose();
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

            if (_drawState.VertexArray != null)
            {
                _drawState.VertexArray.Dispose();
                _drawState.VertexArray = null;
            }
        }

        private readonly DrawState _drawState;
        private readonly Uniform<Vector3S> _colorUniform;
        private readonly Uniform<Vector2S> _originScaleUniform;
        private Color _color;

        private Vector2D _position;
        private bool _positionDirty;
        private HorizontalOrigin _horizontalOrigin;
        private VerticalOrigin _verticalOrigin;

        private VertexBuffer _positionBuffer;

        private static readonly Half[] _originScale = new Half[] { new Half(0.0), new Half(1.0), new Half(-1.0) };
    }
}