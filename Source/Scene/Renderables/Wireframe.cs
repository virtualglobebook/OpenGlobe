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
    public sealed class Wireframe : IDisposable
    {
        public Wireframe(Context context, Mesh mesh)
        {
            _context = context;

            _renderState = new RenderState();
            _renderState.Blending.Enabled = true;
            _renderState.Blending.SourceRGBFactor = SourceBlendingFactor.SourceAlpha;
            _renderState.Blending.SourceAlphaFactor = SourceBlendingFactor.SourceAlpha;
            _renderState.Blending.DestinationRGBFactor = DestinationBlendingFactor.OneMinusSourceAlpha;
            _renderState.Blending.DestinationAlphaFactor = DestinationBlendingFactor.OneMinusSourceAlpha;
            _renderState.FacetCulling.FrontFaceWindingOrder = mesh.FrontFaceWindingOrder;
            _renderState.DepthTest.Function = DepthTestFunction.LessThanOrEqual;

            //
            // This implementation is based on the 2006 SIGGRAPH Sketch:
            //
            //    Single-pass Wireframe Rendering
            //    http://www2.imm.dtu.dk/pubdb/views/edoc_download.php/4884/pdf/imm4884.pdf
            //
            // NVIDIA published a white paper with some enhancements we can consider:
            //
            //    Solid Wireframe
            //    http://developer.download.nvidia.com/SDK/10.5/direct3d/Source/SolidWireframe/Doc/SolidWireframe.pdf
            //
            // More recent work, which I was not aware of at the time, is:
            //
            //    Two Methods for Antialiased Wireframe Drawing with Hidden Line Removal
            //    http://orbit.dtu.dk/getResource?recordId=219956&objectId=1&versionId=1
            //
            string vs =
                @"#version 150

                  in vec4 position;
                  out vec2 windowPosition;
                  uniform mat4 mg_modelViewPerspectiveProjectionMatrix;
                  uniform mat4 mg_viewportTransformationMatrix;

                  vec4 ClipToWindowCoordinates(vec4 v, mat4 viewportTransformationMatrix)
                  {
                      v.xyz /= v.w;                                                        // normalized device coordinates
                      v.xyz = (viewportTransformationMatrix * vec4(v.xyz + 1.0, 1.0)).xyz; // windows coordinates
                      return v;
                  }

                  void main()                     
                  {
                      gl_Position = mg_modelViewPerspectiveProjectionMatrix * position;
                      windowPosition = ClipToWindowCoordinates(gl_Position, mg_viewportTransformationMatrix).xy;
                  }";
            string gs =
                @"#version 150 

                  layout(triangles) in;
                  layout(triangle_strip, max_vertices = 3) out;

                  in vec2 windowPosition[];
                  noperspective out vec3 distanceToEdges;

                  float distanceToLine(vec2 f, vec2 p0, vec2 p1)
                  {
                      vec2 l = f - p0;
                      vec2 d = p1 - p0;

                      //
                      // Closed point on line to f
                      //
                      vec2 p = p0 + (d * (dot(l, d) / dot(d, d)));
                      return distance(f, p);
                  }

                  void main()
                  {
                      vec2 p0 = windowPosition[0];
                      vec2 p1 = windowPosition[1];
                      vec2 p2 = windowPosition[2];

                      gl_Position = gl_in[0].gl_Position;
                      distanceToEdges = vec3(distanceToLine(p0, p1, p2), 0.0, 0.0);
                      EmitVertex();

                      gl_Position = gl_in[1].gl_Position;
                      distanceToEdges = vec3(0.0, distanceToLine(p1, p2, p0), 0.0);
                      EmitVertex();

                      gl_Position = gl_in[2].gl_Position;
                      distanceToEdges = vec3(0.0, 0.0, distanceToLine(p2, p0, p1));
                      EmitVertex();
                  }";
            string fs =
                @"#version 150
                 
                  uniform float u_halfLineWidth;
                  uniform vec3 u_colorUniform;
                  noperspective in vec3 distanceToEdges;
                  out vec4 fragmentColor;

                  void main()
                  {
                      float d = min(distanceToEdges.x, min(distanceToEdges.y, distanceToEdges.z));

                      if (d > u_halfLineWidth + 1.0)
                      {
                          discard;
                      }

                      d = clamp(d - (u_halfLineWidth - 1.0), 0.0, 2.0);
                      fragmentColor = vec4(u_colorUniform, exp2(-2.0 * d * d));
                  }";
            _sp = Device.CreateShaderProgram(vs, gs, fs);

            _lineWidth = _sp.Uniforms["u_halfLineWidth"] as Uniform<float>;
            Width = 1;

            _colorUniform = _sp.Uniforms["u_colorUniform"] as Uniform<Vector3>;
            Color = Color.Black;

            _va = context.CreateVertexArray(mesh, _sp.VertexAttributes, BufferHint.StaticDraw);
            _primitiveType = mesh.PrimitiveType;
        }

        public void Render(SceneState sceneState)
        {
            _lineWidth.Value = (float)(0.5 * Width * sceneState.HighResolutionSnapScale);

            _context.Bind(_renderState);
            _context.Bind(_sp);
            _context.Bind(_va);
            _context.Draw(_primitiveType, sceneState);
        }

        public Context Context
        {
            get { return _context; }
        }

        public double Width { get; set; }
        public Color Color 
        {
            get { return _color; }

            set
            {
                _color = value;
                _colorUniform.Value = new Vector3(_color.R / 255.0f, _color.G / 255.0f, _color.B / 255.0f);
            }
        }

        public bool FacetCullingEnabled
        {
            get { return _renderState.FacetCulling.Enabled; }
            set { _renderState.FacetCulling.Enabled = value; }
        }

        public CullFace FacetCullingFace
        {
            get { return _renderState.FacetCulling.Face; }
            set { _renderState.FacetCulling.Face = value; }
        }

        public bool DepthTestEnabled
        {
            get { return _renderState.DepthTest.Enabled; }
            set { _renderState.DepthTest.Enabled = value; }
        }

        public bool Enabled { get; set; }
        public WindingOrder FrontFaceWindingOrder { get; set; }

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
        private readonly Uniform<float> _lineWidth;
        private readonly Uniform<Vector3> _colorUniform;
        private Color _color;
        private readonly VertexArray _va;
        private readonly PrimitiveType _primitiveType;
    }
}