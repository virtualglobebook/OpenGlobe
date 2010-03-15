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
using OpenTK;

namespace MiniGlobe.Scene
{
    public sealed class Plane : IDisposable
    {
        public Plane(Context context, Vector3D centerOrigin, Vector3D x, Vector3D y)
        {
            _context = context;
            _lineRS = new RenderState();
            _lineRS.FacetCulling.Enabled = false;

            string lineVS =
                @"#version 150

                  in vec4 position;
                  in vec4 color;

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
                      gl_Position = ModelToWindowCoordinates(position,
                          mg_modelViewPerspectiveProjectionMatrix, mg_viewportTransformationMatrix);
                  }";
            string lineGS =
                @"#version 150 

                  layout(lines) in;
                  layout(triangle_strip, max_vertices = 4) out;

                  in vec4 gsColor[];

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
                      EmitVertex();

                      gl_Position = mg_viewportOrthographicProjectionMatrix * v1;
                      EmitVertex();

                      gl_Position = mg_viewportOrthographicProjectionMatrix * v2;
                      EmitVertex();

                      gl_Position = mg_viewportOrthographicProjectionMatrix * v3;
                      EmitVertex();
                  }";
            string lineFS =
                @"#version 150
                 
                  out vec3 fragmentColor;
                  uniform vec3 u_color;

                  void main()
                  {
                      fragmentColor = u_color;
                  }";
            _lineSP = Device.CreateShaderProgram(lineVS, lineGS, lineFS);

            _halfLineWidth = _lineSP.Uniforms["u_halfLineWidth"] as Uniform<float>;
            OutlineWidth = 1;

            _lineColorUniform = _lineSP.Uniforms["u_color"] as Uniform<Vector3S>;
            OutlineColor = Color.Gray;

            Vector3S[] positions = new Vector3S[] 
            { 
                (centerOrigin - x - y).ToVector3S(), 
                (centerOrigin + x - y).ToVector3S(), 
                (centerOrigin + x + y).ToVector3S(), 
                (centerOrigin - x + y).ToVector3S()
            };

            VertexBuffer positionBuffer = Device.CreateVertexBuffer(BufferHint.StaticDraw, positions.Length * Vector3S.SizeInBytes);
            positionBuffer.CopyFromSystemMemory(positions);

            ///////////////////////////////////////////////////////////////////

            _fillRS = new RenderState();
            _fillRS.FacetCulling.Enabled = false;
            _fillRS.Blending.Enabled = true;
            _fillRS.Blending.SourceRGBFactor = SourceBlendingFactor.SourceAlpha;
            _fillRS.Blending.SourceAlphaFactor = SourceBlendingFactor.SourceAlpha;
            _fillRS.Blending.DestinationRGBFactor = DestinationBlendingFactor.OneMinusSourceAlpha;
            _fillRS.Blending.DestinationAlphaFactor = DestinationBlendingFactor.OneMinusSourceAlpha;
            
            string fillVS =
                @"#version 150

                  in vec4 position;
                  uniform mat4 mg_modelViewPerspectiveProjectionMatrix;

                  void main()                     
                  {
                        gl_Position = mg_modelViewPerspectiveProjectionMatrix * position; 
                  }";
            string fillFS =
                @"#version 150
                 
                  out vec4 fragmentColor;
                  uniform vec3 u_color;
                  uniform float u_alpha;

                  void main()
                  {
                      fragmentColor = vec4(u_color, u_alpha);
                  }";
            _fillSP = Device.CreateShaderProgram(fillVS, fillFS);

            _fillColorUniform = _fillSP.Uniforms["u_color"] as Uniform<Vector3S>;
            FillColor = Color.Gray;

            _fillAlphaUniform = _fillSP.Uniforms["u_alpha"] as Uniform<float>;
            FillTranslucency = 0.5f;

            ///////////////////////////////////////////////////////////////////

            ushort[] indices = new ushort[] 
            { 
                0, 1, 2, 3,                             // Line loop
                0, 1, 2, 0, 2, 3                        // Triangles
            };
            IndexBuffer indexBuffer = Device.CreateIndexBuffer(BufferHint.StaticDraw, indices.Length * sizeof(ushort));
            indexBuffer.CopyFromSystemMemory(indices);

            AttachedVertexBuffer attachedPositionBuffer = new AttachedVertexBuffer(
                positionBuffer, VertexAttributeComponentType.Float, 3);
            _va = context.CreateVertexArray();
            _va.VertexBuffers[_lineSP.VertexAttributes["position"].Location] = attachedPositionBuffer;
            _va.IndexBuffer = indexBuffer;

            ShowOutline = true;
            ShowFill = true;
        }

        public void Render(SceneState sceneState)
        {
            _context.Bind(_va);

            if (ShowOutline)
            {
                //
                // Pass 1:  Outline
                //
                _halfLineWidth.Value = (float)(OutlineWidth * 0.5 * sceneState.HighResolutionSnapScale);
                _context.Bind(_lineRS);
                _context.Bind(_lineSP);
                _context.Draw(PrimitiveType.LineLoop, 0, 4, sceneState);
            }

            if (ShowFill)
            {
                //
                // Pass 2:  Fill
                //
                _context.Bind(_fillRS);
                _context.Bind(_fillSP);
                _context.Draw(PrimitiveType.Triangles, 4, 6, sceneState);
            }
        }

        public Context Context
        {
            get { return _context; }
        }

        public double OutlineWidth { get; set; }
        public bool ShowOutline { get; set; }
        public bool ShowFill { get; set; }

        public Color OutlineColor
        {
            get { return _lineColor; }

            set
            {
                _lineColor = value;
                _lineColorUniform.Value = new Vector3S(value.R / 255.0f, value.G / 255.0f, value.B / 255.0f);
            }
        }

        public Color FillColor
        {
            get { return _fillColor; }

            set
            {
                _fillColor = value;
                _fillColorUniform.Value = new Vector3S(value.R / 255.0f, value.G / 255.0f, value.B / 255.0f);
            }
        }

        public float FillTranslucency
        {
            get { return _fillTranslucency; }

            set
            {
                _fillTranslucency = value;
                _fillAlphaUniform.Value = 1.0f - value;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            _lineSP.Dispose();
            _fillSP.Dispose();
            _va.Dispose();
        }

        #endregion

        private readonly Context _context;
        private readonly RenderState _lineRS;
        private readonly ShaderProgram _lineSP;

        private readonly Uniform<float> _halfLineWidth;
        private readonly Uniform<Vector3S> _lineColorUniform;
        private Color _lineColor;

        private readonly RenderState _fillRS;
        private readonly ShaderProgram _fillSP;

        private readonly VertexArray _va;

        private readonly Uniform<Vector3S> _fillColorUniform;
        private Color _fillColor;
        private readonly Uniform<float> _fillAlphaUniform;
        private float _fillTranslucency;
    }
}