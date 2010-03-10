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
using MiniGlobe.Core.Geometry;
using MiniGlobe.Renderer;
using OpenTK;

namespace MiniGlobe.Scene
{
    public sealed class BillboardGroup : IDisposable
    {
        public BillboardGroup(Context context, Vector3[] positions, Bitmap bitmap)
        {
            _context = context;
            _renderState = new RenderState();
            _renderState.FacetCulling.Enabled = false;
            _renderState.Blending.Enabled = true;
            _renderState.Blending.SourceRGBFactor = SourceBlendingFactor.SourceAlpha;
            _renderState.Blending.SourceAlphaFactor = SourceBlendingFactor.SourceAlpha;
            _renderState.Blending.DestinationRGBFactor = DestinationBlendingFactor.OneMinusSourceAlpha;
            _renderState.Blending.DestinationAlphaFactor = DestinationBlendingFactor.OneMinusSourceAlpha;

            string vs =
                @"#version 150

                  in vec4 position;

                  uniform mat4 mg_modelViewPerspectiveProjectionMatrix;
                  uniform mat4 mg_viewportTransformationMatrix;

                  vec4 WorldToWindowCoordinates(
                      vec4 v, 
                      mat4 modelViewPerspectiveProjectionMatrix, 
                      mat4 viewportTransformationMatrix)
                  {
                      v = modelViewPerspectiveProjectionMatrix * v;                        // clip coordinates

                      // TODO:  Just to avoid z fighting with Earth for now.
                     // v.z -= 0.001;

                      v.xyz /= v.w;                                                           // normalized device coordinates
                      v.xyz = (viewportTransformationMatrix * vec4(v.xyz + 1.0, 1.0)).xyz; // windows coordinates
                      return v;
                  }

                  void main()                     
                  {
                      gl_Position = WorldToWindowCoordinates(position, 
                          mg_modelViewPerspectiveProjectionMatrix, mg_viewportTransformationMatrix);
                  }";
            string gs =
                @"#version 150 

                  layout(points) in;
                  layout(triangle_strip, max_vertices = 4) out;

                  out vec2 textureCoordinates;

                  uniform mat4 mg_viewportOrthographicProjectionMatrix;
                  uniform sampler2D mg_texture0;
                  uniform float mg_highResolutionSnapScale;
                  uniform vec4 mg_viewport;

                  void main()
                  {
                      vec4 center = gl_in[0].gl_Position;
                      vec2 halfSize = vec2(textureSize(mg_texture0, 0)) * 0.5 * mg_highResolutionSnapScale;

center.x -= halfSize.x;

                      vec4 v0 = vec4(center.xy - halfSize, center.z, 1.0);
                      vec4 v1 = vec4(center.xy + vec2(halfSize.x, -halfSize.y), center.z, 1.0);
                      vec4 v2 = vec4(center.xy + vec2(-halfSize.x, halfSize.y), center.z, 1.0);
                      vec4 v3 = vec4(center.xy + halfSize, center.z, 1.0);

                      //
                      // Cull - could also cull in z.
                      //
                      if ((v3.x < mg_viewport.x) || (v3.y < mg_viewport.y) ||
                          (v0.x > mg_viewport.z) || (v0.y > mg_viewport.w))
                      {
                          return;
                      }

                      gl_Position = mg_viewportOrthographicProjectionMatrix * v0;
                      textureCoordinates = vec2(0, 0);
                      EmitVertex();

                      gl_Position = mg_viewportOrthographicProjectionMatrix * v1;
                      textureCoordinates = vec2(1, 0);
                      EmitVertex();

                      gl_Position = mg_viewportOrthographicProjectionMatrix * v2;
                      textureCoordinates = vec2(0, 1);
                      EmitVertex();

                      gl_Position = mg_viewportOrthographicProjectionMatrix * v3;
                      textureCoordinates = vec2(1, 1);
                      EmitVertex();
                  }";
            string fs =
                @"#version 150
                 
                  in vec2 textureCoordinates;
                  out vec4 fragmentColor;
                  uniform vec3 u_colorUniform;
                  uniform sampler2D mg_texture0;

                  void main()
                  {
                      vec4 color = texture(mg_texture0, textureCoordinates);

                      if (color.a == 0.0)
                      {
                          discard;
                      }
                      fragmentColor = vec4(color.rgb * u_colorUniform, color.a);
                  }";
            _sp = Device.CreateShaderProgram(vs, gs, fs);
            _colorUniform = _sp.Uniforms["u_colorUniform"] as Uniform<Vector3>;
            Color = Color.White;

            VertexBuffer positionBuffer = Device.CreateVertexBuffer(BufferHint.StaticDraw, positions.Length * Vector3.SizeInBytes);
            positionBuffer.CopyFromSystemMemory(positions);

            AttachedVertexBuffer attachedPositionBuffer = new AttachedVertexBuffer(
                positionBuffer, VertexAttributeComponentType.Float, 3);
            _va = context.CreateVertexArray();
            _va.VertexBuffers[_sp.VertexAttributes["position"].Location] = attachedPositionBuffer;

            _texture = Device.CreateTexture2D(bitmap, TextureFormat.RedGreenBlueAlpha8, false);
        }

        public void Render(SceneState sceneState)
        {
            _context.TextureUnits[0].Texture2D = _texture;
            _context.Bind(_renderState);
            _context.Bind(_sp);
            _context.Bind(_va);
            _context.Draw(PrimitiveType.Points, sceneState);
        }

        public Context Context
        {
            get { return _context; }
        }

        public bool Wireframe
        {
            get { return _renderState.RasterizationMode == RasterizationMode.Line; }
            set { _renderState.RasterizationMode = value ? RasterizationMode.Line : RasterizationMode.Fill; }
        }

        public bool DepthTestEnabled
        {
            get { return _renderState.DepthTest.Enabled; }
            set { _renderState.DepthTest.Enabled = value; }
        }

        public Color Color
        {
            get { return _color; }

            set
            {
                _color = value;
                _colorUniform.Value = new Vector3(_color.R / 255.0f, _color.G / 255.0f, _color.B / 255.0f);
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            _texture.Dispose();
            _sp.Dispose();
            _va.Dispose();
        }

        #endregion

        private readonly Context _context;
        private readonly RenderState _renderState;
        private readonly ShaderProgram _sp;
        private readonly VertexArray _va;
        private readonly Texture2D _texture;
        private readonly Uniform<Vector3> _colorUniform;
        private Color _color;
    }
}