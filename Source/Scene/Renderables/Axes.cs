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
    public sealed class Axes : IDisposable
    {
        public Axes(Context context)
        {
            _context = context;
            _renderState = new RenderState();

            string vs =
                @"#version 150

                  in vec4 position;
                  in vec4 color;
                  out vec4 gsPosition;
                  out vec4 gsColor;

                  uniform mat4 mg_ModelViewPerspectiveProjectionMatrix;
                  uniform mat4 mg_ViewportTransformationMatrix;
                  uniform float u_lineLength;

                  vec4 WorldToWindowCoordinates(vec4 v)
                  {
                      v = mg_ModelViewPerspectiveProjectionMatrix * v;                        // clip coordinates
                      v.xyz /= v.w;                                                           // normalized device coordinates
                      v.xyz = (mg_ViewportTransformationMatrix * vec4(v.xyz + 1.0, 1.0)).xyz; // windows coordinates
                      return v;
                  }

                  void main()                     
                  {
                      gl_Position = WorldToWindowCoordinates(vec4(u_lineLength * position.xyz, position.w));
                      gsColor = color;
                  }";
            string gs =
                @"#version 150 

                  layout(lines) in;
                  layout(triangle_strip, max_vertices = 4) out;

                  in vec4 gsColor[];
                  out vec4 fsColor;

                  uniform mat4 mg_OrthographicProjectionMatrix;
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

                      gl_Position = mg_OrthographicProjectionMatrix * v0;
                      fsColor = gsColor[0];
                      EmitVertex();

                      gl_Position = mg_OrthographicProjectionMatrix * v1;
                      fsColor = gsColor[0];
                      EmitVertex();

                      gl_Position = mg_OrthographicProjectionMatrix * v2;
                      fsColor = gsColor[1];
                      EmitVertex();

                      gl_Position = mg_OrthographicProjectionMatrix * v3;
                      fsColor = gsColor[1];
                      EmitVertex();
                  }";
            string fs =
                @"#version 150
                 
                  in vec4 fsColor;
                  out vec4 fragColor;

                  void main()
                  {
                      fragColor = fsColor;
                  }";
            _sp = Device.CreateShaderProgram(vs, gs, fs);

            _halfLineWidth = _sp.Uniforms["u_halfLineWidth"] as Uniform<float>;
            Width = 1;

            _lineLength = _sp.Uniforms["u_lineLength"] as Uniform<float>;
            Length = 1;

            Vector3[] positions = new Vector3[] 
            { 
                new Vector3(0, 0, 0),
                new Vector3(1, 0, 0),
                new Vector3(0, 0, 0),
                new Vector3(0, 1, 0),
                new Vector3(0, 0, 0),
                new Vector3(0, 0, 1)
            };
            VertexBuffer positionBuffer = Device.CreateVertexBuffer(BufferHint.StaticDraw, positions.Length * Vector3.SizeInBytes);
            positionBuffer.CopyFromSystemMemory(positions);

            BlittableRGBA[] colors = new BlittableRGBA[] 
            { 
                new BlittableRGBA(Color.Red),
                new BlittableRGBA(Color.Red),
                new BlittableRGBA(Color.Green),
                new BlittableRGBA(Color.Green),
                new BlittableRGBA(Color.Blue),
                new BlittableRGBA(Color.Blue)
            };
            VertexBuffer colorBuffer = Device.CreateVertexBuffer(BufferHint.StaticDraw, colors.Length * BlittableRGBA.SizeInBytes);
            colorBuffer.CopyFromSystemMemory(colors);

            AttachedVertexBuffer attachedPositionBuffer = new AttachedVertexBuffer(
                positionBuffer, VertexAttributeComponentType.Float, 3);
            AttachedVertexBuffer attachedColorBuffer = new AttachedVertexBuffer(
                colorBuffer, VertexAttributeComponentType.UnsignedByte, 4);
            _va = context.CreateVertexArray();
            _va.VertexBuffers[_sp.VertexAttributes["position"].Location] = attachedPositionBuffer;
            _va.VertexBuffers[_sp.VertexAttributes["color"].Location] = attachedColorBuffer;
        }

        public void Render(SceneState sceneState)
        {
            _context.Bind(_renderState);
            _context.Bind(_sp);
            _context.Bind(_va);
            _context.Draw(PrimitiveType.Lines, sceneState);
        }

        public Context Context
        {
            get { return _context; }
        }

        public float Width
        {
            get { return _lineWidth; }
            set 
            {
                _lineWidth = value;
                _halfLineWidth.Value = _lineWidth * 0.5f; ; 
            }
        }

        public float Length
        {
            get { return _lineLength.Value; }
            set { _lineLength.Value = value; }
        }

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
        private float _lineWidth;
        private readonly Uniform<float> _halfLineWidth;
        private readonly Uniform<float> _lineLength;
        private readonly VertexArray _va;
    }
}