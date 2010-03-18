#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using MiniGlobe.Core;
using MiniGlobe.Core.Geometry;
using MiniGlobe.Renderer;

namespace MiniGlobe.Scene
{
    public sealed class Polyline : IDisposable
    {
        public Polyline(Context context, Vector3S[] positions, BlittableRGBA[] colors)
        {
            if (positions.Length != colors.Length)
            {
                throw new ArgumentException("positions.Length != colors.Length");
            }

            _context = context;
            _renderState = new RenderState();
            _renderState.FacetCulling.Enabled = false;

            string vs =
                @"#version 150

                  in vec4 position;
                  in vec4 color;
                  out vec4 gsColor;

                  uniform mat4 mg_modelViewPerspectiveProjectionMatrix;
                  uniform mat4 mg_viewportTransformationMatrix;
                  uniform float u_lineLength;

                  vec4 ModelToWindowCoordinates(
                      vec4 v, 
                      mat4 modelViewPerspectiveProjectionMatrix,
                      mat4 viewportTransformationMatrix)
                  {
                      v = modelViewPerspectiveProjectionMatrix * v;                        // clip coordinates
                      v.xyz /= v.w;                                                        // normalized device coordinates
                      v.xyz = (viewportTransformationMatrix * vec4(v.xyz + 1.0, 1.0)).xyz; // windows coordinates
                      return v;
                  }

                  void main()                     
                  {
                      gl_Position = ModelToWindowCoordinates(vec4(u_lineLength * position.xyz, position.w),
                          mg_modelViewPerspectiveProjectionMatrix, mg_viewportTransformationMatrix);
                      gsColor = color;
                  }";
            string gs =
                @"#version 150 

                  layout(lines) in;
                  layout(triangle_strip, max_vertices = 4) out;

                  in vec4 gsColor[];
                  out vec4 fsColor;

                  uniform mat4 mg_viewportOrthographicProjectionMatrix;
                  uniform float u_halfLineWidth;

                  void main()
                  {
                      vec4 firstPosition = gl_in[0].gl_Position;
                      vec4 secondPosition = gl_in[1].gl_Position;

                      vec2 direction = secondPosition.xy - firstPosition.xy;
                      vec2 normal = normalize(vec2(direction.y, -direction.x));

                      vec4 v0 = vec4(firstPosition.xy - (normal * u_halfLineWidth), firstPosition.z, 1.0);
                      vec4 v1 = vec4(firstPosition.xy + (normal * u_halfLineWidth), firstPosition.z, 1.0);
                      vec4 v2 = vec4(secondPosition.xy - (normal * u_halfLineWidth), secondPosition.z, 1.0);
                      vec4 v3 = vec4(secondPosition.xy + (normal * u_halfLineWidth), secondPosition.z, 1.0);

                      gl_Position = mg_viewportOrthographicProjectionMatrix * v0;
                      fsColor = gsColor[0];
                      EmitVertex();

                      gl_Position = mg_viewportOrthographicProjectionMatrix * v1;
                      fsColor = gsColor[0];
                      EmitVertex();

                      gl_Position = mg_viewportOrthographicProjectionMatrix * v2;
                      fsColor = gsColor[1];
                      EmitVertex();

                      gl_Position = mg_viewportOrthographicProjectionMatrix * v3;
                      fsColor = gsColor[1];
                      EmitVertex();
                  }";
            string fs =
                @"#version 150
                 
                  in vec4 fsColor;
                  out vec4 fragmentColor;

                  void main()
                  {
                      fragmentColor = fsColor;
                  }";
            _sp = Device.CreateShaderProgram(vs, gs, fs);

            _halfLineWidth = _sp.Uniforms["u_halfLineWidth"] as Uniform<float>;
            Width = 1;

            _lineLength = _sp.Uniforms["u_lineLength"] as Uniform<float>;
            Length = 1;

            VertexBuffer positionBuffer = Device.CreateVertexBuffer(BufferHint.StaticDraw, positions.Length * Vector3S.SizeInBytes);
            positionBuffer.CopyFromSystemMemory(positions);

            VertexBuffer colorBuffer = Device.CreateVertexBuffer(BufferHint.StaticDraw, colors.Length * BlittableRGBA.SizeInBytes);
            colorBuffer.CopyFromSystemMemory(colors);

            AttachedVertexBuffer attachedPositionBuffer = new AttachedVertexBuffer(
                positionBuffer, VertexAttributeComponentType.Float, 3);
            AttachedVertexBuffer attachedColorBuffer = new AttachedVertexBuffer(
                colorBuffer, VertexAttributeComponentType.UnsignedByte, 4, true);
            _va = context.CreateVertexArray();
            _va.VertexBuffers[_sp.VertexAttributes["position"].Location] = attachedPositionBuffer;
            _va.VertexBuffers[_sp.VertexAttributes["color"].Location] = attachedColorBuffer;
        }

        public void Render(SceneState sceneState)
        {
            _halfLineWidth.Value = (float)(Width * 0.5 * sceneState.HighResolutionSnapScale);
            _lineLength.Value = (float)Length;

            _context.Bind(_renderState);
            _context.Bind(_sp);
            _context.Bind(_va);
            _context.Draw(PrimitiveType.Lines, sceneState);
        }

        public Context Context
        {
            get { return _context; }
        }

        public double Width { get; set; }
        public double Length { get; set; }

        public bool Wireframe
        {
            get { return _renderState.RasterizationMode == RasterizationMode.Line; }
            set { _renderState.RasterizationMode = value ? RasterizationMode.Line : RasterizationMode.Fill; }
        }

        #region IDisposable Members

        public void Dispose()
        {
            _sp.Dispose();
            _va.Dispose();
        }

        #endregion

        private readonly Context _context;
        private readonly RenderState _renderState;
        private readonly ShaderProgram _sp;
        private readonly Uniform<float> _halfLineWidth;
        private readonly Uniform<float> _lineLength;
        private readonly VertexArray _va;
    }
}