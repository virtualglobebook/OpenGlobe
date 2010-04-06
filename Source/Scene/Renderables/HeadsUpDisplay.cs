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
    public sealed class HeadsUpDisplay
    {
        public HeadsUpDisplay(Context context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            ///////////////////////////////////////////////////////////////////

            _context = context;
            _renderState = new RenderState();
            _renderState.FacetCulling.Enabled = false;
            _renderState.DepthTest.Enabled = false;
            _renderState.Blending.Enabled = true;
            _renderState.Blending.SourceRGBFactor = SourceBlendingFactor.SourceAlpha;
            _renderState.Blending.SourceAlphaFactor = SourceBlendingFactor.SourceAlpha;
            _renderState.Blending.DestinationRGBFactor = DestinationBlendingFactor.OneMinusSourceAlpha;
            _renderState.Blending.DestinationAlphaFactor = DestinationBlendingFactor.OneMinusSourceAlpha;

            string vs =
                @"#version 150

                  in vec4 position;

                  void main()                     
                  {
                      gl_Position = position;
                  }";
            string gs =
                @"#version 150 

                  layout(points) in;
                  layout(triangle_strip, max_vertices = 4) out;

                  out vec2 fsTextureCoordinates;

                  uniform mat4 mg_viewportOrthographicProjectionMatrix;
                  uniform sampler2D mg_texture0;
                  uniform float mg_highResolutionSnapScale;
                  uniform vec2 u_originScale;

                  void main()
                  {
                      vec2 halfSize = vec2(textureSize(mg_texture0, 0)) * 0.5 * mg_highResolutionSnapScale;
                      vec4 center = gl_in[0].gl_Position;
                      center.xy += (u_originScale * halfSize);

                      vec4 v0 = vec4(center.xy - halfSize, center.z, 1.0);
                      vec4 v1 = vec4(center.xy + vec2(halfSize.x, -halfSize.y), center.z, 1.0);
                      vec4 v2 = vec4(center.xy + vec2(-halfSize.x, halfSize.y), center.z, 1.0);
                      vec4 v3 = vec4(center.xy + halfSize, center.z, 1.0);

                      gl_Position = mg_viewportOrthographicProjectionMatrix * v0;
                      fsTextureCoordinates = vec2(0.0, 0.0);
                      EmitVertex();

                      gl_Position = mg_viewportOrthographicProjectionMatrix * v1;
                      fsTextureCoordinates = vec2(1.0, 0.0);
                      EmitVertex();

                      gl_Position = mg_viewportOrthographicProjectionMatrix * v2;
                      fsTextureCoordinates = vec2(0.0, 1.0);
                      EmitVertex();

                      gl_Position = mg_viewportOrthographicProjectionMatrix * v3;
                      fsTextureCoordinates = vec2(1.0, 1.0);
                      EmitVertex();
                  }";
            string fs =
                @"#version 150
                 
                  in vec2 fsTextureCoordinates;

                  out vec4 fragmentColor;

                  uniform sampler2D mg_texture0;
                  uniform vec3 u_colorUniform;

                  void main()
                  {
                      vec4 color = texture(mg_texture0, fsTextureCoordinates);

                      if (color.a == 0.0)
                      {
                          discard;
                      }
                      fragmentColor = vec4(color.rgb * u_colorUniform.rgb, color.a);
                  }";
            _sp = Device.CreateShaderProgram(vs, gs, fs);
            _colorUniform = _sp.Uniforms["u_colorUniform"] as Uniform<Vector3S>;
            _originScaleUniform = _sp.Uniforms["u_originScale"] as Uniform<Vector2S>;
            
            ///////////////////////////////////////////////////////////////////

            Color = Color.White;
            HorizontalOrigin = HorizontalOrigin.Left;
            VerticalOrigin = VerticalOrigin.Bottom;
            _positionDirty = true;
        }

        private void CreateVertexArray()
        {
            // TODO:  Hint per buffer?  One hint?
            _positionBuffer = Device.CreateVertexBuffer(BufferHint.StaticDraw, SizeInBytes<Vector2S>.Value);

            AttachedVertexBuffer attachedPositionBuffer = new AttachedVertexBuffer(
                _positionBuffer, VertexAttributeComponentType.Float, 2);

            _va = _context.CreateVertexArray();
            _va.VertexBuffers[_sp.VertexAttributes["position"].Location] = attachedPositionBuffer;
         }

        private void Update()
        {
            if (_positionDirty)
            {
                DisposeVertexArray();
                CreateVertexArray();

                Vector2S[] positions = new Vector2S[] { _position.ToVector2S() };
                _positionBuffer.CopyFromSystemMemory(positions);

                _positionDirty = false;
            }
        }

        public void Render(SceneState sceneState)
        {
            if (Texture == null)
            {
                throw new InvalidOperationException("Texture");
            }

            Update();

            if (_va != null)
            {
                _context.TextureUnits[0].Texture2D = Texture;
                _context.Bind(_renderState);
                _context.Bind(_sp);
                _context.Bind(_va);
                _context.Draw(PrimitiveType.Points, sceneState);
            }
        }

        public Context Context
        {
            get { return _context; }
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

        private readonly Context _context;
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