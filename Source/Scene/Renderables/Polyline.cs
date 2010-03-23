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
        public Polyline(Context context, Mesh mesh)
        {
            if (mesh == null)
            {
                throw new ArgumentNullException("mesh");
            }

            if (mesh.PrimitiveType != PrimitiveType.Lines &&
                mesh.PrimitiveType != PrimitiveType.LineLoop &&
                mesh.PrimitiveType != PrimitiveType.LineStrip)
            {
                throw new ArgumentException("mesh.PrimitiveType must be Lines, LineLoop, or LineStrip.");
            }

            _context = context;
            _renderState = new RenderState();
            _renderState.FacetCulling.Enabled = false;

            string vs =
                @"#version 150

                  in vec4 position;
                  in vec4 color;
                  out vec4 gsColor;

                  void main()                     
                  {
                      gl_Position = position;
                      gsColor = color;
                  }";
            string gs =
                @"#version 150 

                  layout(lines) in;
                  layout(triangle_strip, max_vertices = 4) out;

                  in vec4 gsColor[];
                  out vec4 fsColor;

                  uniform mat4 mg_modelViewPerspectiveProjectionMatrix;
                  uniform mat4 mg_viewportTransformationMatrix;
                  uniform mat4 mg_viewportOrthographicProjectionMatrix;
                  uniform float mg_perspectiveNearPlaneDistance;
                  uniform float u_halfLineWidth;

                  vec4 ClipToWindowCoordinates(vec4 v, mat4 viewportTransformationMatrix)
                  {
                      v.xyz /= v.w;                                                        // normalized device coordinates
                      v.xyz = (viewportTransformationMatrix * vec4(v.xyz + 1.0, 1.0)).xyz; // windows coordinates
                      return v;
                  }

                  void ClipLineSegmentToNearPlane(
                      float nearPlaneDistance, 
                      mat4 modelViewPerspectiveProjectionMatrix,
                      vec4 modelP0, 
                      vec4 modelP1, 
                      out vec4 clipP0, 
                      out vec4 clipP1)
                  {
                      clipP0 = mg_modelViewPerspectiveProjectionMatrix * modelP0;
                      clipP1 = mg_modelViewPerspectiveProjectionMatrix * modelP1;

                      float distanceToP0 = clipP0.z - nearPlaneDistance;
                      float distanceToP1 = clipP1.z - nearPlaneDistance;

                      if ((distanceToP0 * distanceToP1) < 0.0)
                      {
                          float t = distanceToP0 / (distanceToP0 - distanceToP1);
                          vec3 modelV = vec3(modelP0) + t * (vec3(modelP1) - vec3(modelP0));
                          vec4 clipV = modelViewPerspectiveProjectionMatrix * vec4(modelV, 1);

                          if (distanceToP0 < 0.0)
                          {
                              clipP0 = clipV;
                          }
                          else
                          {
                              clipP1 = clipV;
                          }
                      }
                  }

                  void main()
                  {
                      vec4 clipP0;
                      vec4 clipP1;
                      ClipLineSegmentToNearPlane(mg_perspectiveNearPlaneDistance, 
                        mg_modelViewPerspectiveProjectionMatrix,
                        gl_in[0].gl_Position, gl_in[1].gl_Position, clipP0, clipP1);

                      vec4 windowP0 = ClipToWindowCoordinates(clipP0, mg_viewportTransformationMatrix);
                      vec4 windowP1 = ClipToWindowCoordinates(clipP1, mg_viewportTransformationMatrix);

                      vec2 direction = windowP1.xy - windowP0.xy;
                      vec2 normal = normalize(vec2(direction.y, -direction.x));

                      vec4 v0 = vec4(windowP0.xy - (normal * u_halfLineWidth), windowP0.z, 1.0);
                      vec4 v1 = vec4(windowP0.xy + (normal * u_halfLineWidth), windowP0.z, 1.0);
                      vec4 v2 = vec4(windowP1.xy - (normal * u_halfLineWidth), windowP1.z, 1.0);
                      vec4 v3 = vec4(windowP1.xy + (normal * u_halfLineWidth), windowP1.z, 1.0);

                      gl_Position = mg_viewportOrthographicProjectionMatrix * v0;
                      fsColor = gsColor[0];
                      EmitVertex();

                      gl_Position = mg_viewportOrthographicProjectionMatrix * v1;
                      fsColor = gsColor[0];
                      EmitVertex();

                      gl_Position = mg_viewportOrthographicProjectionMatrix * v2;
                      fsColor = gsColor[0];
                      EmitVertex();

                      gl_Position = mg_viewportOrthographicProjectionMatrix * v3;
                      fsColor = gsColor[0];
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

            _va = _context.CreateVertexArray(mesh, _sp.VertexAttributes, BufferHint.StaticDraw);
        }

        public void Render(SceneState sceneState)
        {
            _halfLineWidth.Value = (float)(Width * 0.5 * sceneState.HighResolutionSnapScale);

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
        private readonly VertexArray _va;
    }
}