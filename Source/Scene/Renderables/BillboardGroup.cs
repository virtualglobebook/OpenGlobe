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
    public sealed class BillboardGroup : IRenderable, IDisposable
    {
        public BillboardGroup(Context context)
        {
            _context = context;
            _renderState = new RenderState();
            _renderState.FacetCulling.Enabled = false;

            string vs =
                @"#version 150

                  in vec4 position;
                  out vec4 gsPosition;

                  uniform mat4 mg_ModelViewPerspectiveProjectionMatrix;
                  uniform mat4 mg_ViewportTransformationMatrix;

                  vec4 WorldToWindowCoordinates(vec4 v)
                  {
                      v = mg_ModelViewPerspectiveProjectionMatrix * v;                        // clip coordinates
                      v.xyz /= v.w;                                                           // normalized device coordinates
                      v.xyz = (mg_ViewportTransformationMatrix * vec4(v.xyz + 1.0, 1.0)).xyz; // windows coordinates
                      return v;
                  }

                  void main()                     
                  {
                      gl_Position = WorldToWindowCoordinates(position);
                  }";
            string gs =
                @"#version 150 

                  layout(points) in;
                  layout(triangle_strip, max_vertices = 4) out;

                  uniform mat4 mg_OrthographicProjectionMatrix;

                  void main()
                  {
                      vec4 center = gl_in[0].gl_Position;
                      vec2 halfSize = vec2(50, 25);

                      vec4 v0 = vec4(center.xy - halfSize, center.z, 1.0);
                      vec4 v1 = vec4(center.xy + vec2(halfSize.x, -halfSize.y), center.z, 1.0);
                      vec4 v2 = vec4(center.xy + vec2(-halfSize.x, halfSize.y), center.z, 1.0);
                      vec4 v3 = vec4(center.xy + halfSize, center.z, 1.0);

                      gl_Position = mg_OrthographicProjectionMatrix * v0;
                      EmitVertex();

                      gl_Position = mg_OrthographicProjectionMatrix * v1;
                      EmitVertex();

                      gl_Position = mg_OrthographicProjectionMatrix * v2;
                      EmitVertex();

                      gl_Position = mg_OrthographicProjectionMatrix * v3;
                      EmitVertex();
                  }";
            string fs =
                @"#version 150
                 
                  out vec4 fragColor;

                  void main()
                  {
                      fragColor = vec4(1, 0, 0, 1);
                  }";
            _sp = Device.CreateShaderProgram(vs, gs, fs);

            Vector3[] positions = new Vector3[] 
            { 
                new Vector3(1.01f, 0, 0),
                new Vector3(0, 1.01f, 0),
                new Vector3(0, 0, 1.01f)
            };
            VertexBuffer positionBuffer = Device.CreateVertexBuffer(BufferHint.StaticDraw, positions.Length * Vector3.SizeInBytes);
            positionBuffer.CopyFromSystemMemory(positions);

            AttachedVertexBuffer attachedPositionBuffer = new AttachedVertexBuffer(
                positionBuffer, VertexAttributeComponentType.Float, 3);
            _va = context.CreateVertexArray();
            _va.VertexBuffers[_sp.VertexAttributes["position"].Location] = attachedPositionBuffer;
        }

        #region IRenderable Members

        public void Render(SceneState sceneState)
        {
            _context.Bind(_renderState);
            _context.Bind(_sp);
            _context.Bind(_va);
            _context.Draw(PrimitiveType.Points, sceneState);
        }

        #endregion

        public Context Context
        {
            get { return _context; }
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
        private readonly VertexArray _va;
    }
}