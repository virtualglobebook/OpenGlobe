#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;
using System.Drawing;
using OpenGlobe.Core;
using OpenGlobe.Renderer;

namespace OpenGlobe.Scene
{
    public sealed class HeadsUpDisplay : IDisposable
    {
        public HeadsUpDisplay()
        {
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
            _colorUniform = (Uniform<Vector3F>)sp.Uniforms["u_color"];
            _originScaleUniform = (Uniform<Vector2F>)sp.Uniforms["u_originScale"];
            _showBackground = (Uniform<bool>)sp.Uniforms["u_showBackground"];
            _drawState = new DrawState(renderState, sp, null);

            Color = Color.White;
            HorizontalOrigin = HorizontalOrigin.Left;
            VerticalOrigin = VerticalOrigin.Bottom;
            _positionDirty = true;
        }

        private void CreateVertexArray(Context context)
        {
            // TODO:  Buffer hint.
            _positionBuffer = Device.CreateVertexBuffer(BufferHint.StaticDraw, SizeInBytes<Vector2F>.Value);

            VertexBufferAttribute positionAttribute = new VertexBufferAttribute(
                _positionBuffer, ComponentDatatype.Float, 2);

            _vertexArray = context.CreateVertexArray();
            _vertexArray.Attributes[_drawState.ShaderProgram.VertexAttributes["position"].Location] = positionAttribute;

            _drawState.VertexArray = _vertexArray;
         }

        private void Update(Context context)
        {
            if (_positionDirty)
            {
                DisposeVertexArray();
                CreateVertexArray(context);

                Vector2F[] positions = new Vector2F[] { _position.ToVector2F() };
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
                context.TextureUnits[0].TextureSampler = Device.TextureSamplers.LinearClamp;
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
                _colorUniform.Value = Vector3F.FromNormalizedColor(value);
            }
        }

        public HorizontalOrigin HorizontalOrigin
        {
            get { return _horizontalOrigin; }
            set
            {
                _horizontalOrigin = value;
                _originScaleUniform.Value = new Vector2F(
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
                _originScaleUniform.Value = new Vector2F(
                    _originScaleUniform.Value.X,
                    _originScale[(int)value]);
            }
        }

        public bool ShowBackground
        {
            get { return _showBackground.Value; }
            set { _showBackground.Value = value; }
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

            if (_vertexArray != null)
            {
                _vertexArray.Dispose();
                _drawState.VertexArray = null;
            }
        }

        private readonly DrawState _drawState;
        private readonly Uniform<Vector3F> _colorUniform;
        private readonly Uniform<Vector2F> _originScaleUniform;
        private readonly Uniform<bool> _showBackground;
        private Color _color;

        private Vector2D _position;
        private bool _positionDirty;
        private HorizontalOrigin _horizontalOrigin;
        private VerticalOrigin _verticalOrigin;

        private VertexBuffer _positionBuffer;
        private VertexArray _vertexArray;

        private static readonly Half[] _originScale = new Half[] { new Half(0.0), new Half(1.0), new Half(-1.0) };
    }
}