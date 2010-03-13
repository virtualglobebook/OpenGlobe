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
using System.Collections.Generic;
using MiniGlobe.Core;
using MiniGlobe.Core.Geometry;
using MiniGlobe.Renderer;
using OpenTK;

namespace MiniGlobe.Scene
{
    public sealed class BillboardGroup2 : IDisposable
    {
        public BillboardGroup2(Context context, IEnumerable<Billboard> billboards, Bitmap bitmap)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            if (billboards == null)
            {
                throw new ArgumentNullException("billboards");
            }

            if (bitmap == null)
            {
                throw new ArgumentNullException("bitmap");
            }

            ///////////////////////////////////////////////////////////////////

            int numberOfBillboards = EnumerableCount(billboards);
            
            _billboards = billboards;
            _dirtyBillboards = new List<Billboard>(numberOfBillboards);

            int offset = 0;
            foreach (Billboard b in _billboards)
            {
                if (b.Owner != null)
                {
                    throw new ArgumentException("A billboard in billboards is already in another BillboardGroup.");
                }

                b.Dirty = true;
                b.Owner = this;
                b.VertexBufferOffset = offset++;

                _dirtyBillboards.Add(b);
            }
            
            ///////////////////////////////////////////////////////////////////

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

//center.x -= halfSize.x;

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
                  uniform sampler2D mg_texture0;

                  void main()
                  {
                      vec4 color = texture(mg_texture0, textureCoordinates);

                      if (color.a == 0.0)
                      {
                          discard;
                      }
                      fragmentColor = color;
                  }";
            _sp = Device.CreateShaderProgram(vs, gs, fs);

            // TODO:  Hint
            _positionBuffer = Device.CreateVertexBuffer(BufferHint.StaticDraw, numberOfBillboards * Vector3.SizeInBytes);

            AttachedVertexBuffer attachedPositionBuffer = new AttachedVertexBuffer(
                _positionBuffer, VertexAttributeComponentType.Float, 3);
            _va = context.CreateVertexArray();
            _va.VertexBuffers[_sp.VertexAttributes["position"].Location] = attachedPositionBuffer;

            _texture = Device.CreateTexture2D(bitmap, TextureFormat.RedGreenBlueAlpha8, false);
        }

        public void Render(SceneState sceneState)
        {
            if (_dirtyBillboards.Count != 0)
            {
                Vector3[] dirtyPosition = new Vector3[1];
                foreach (Billboard b in _dirtyBillboards)
                {
                    // TODO: Combine for performance
                    dirtyPosition[0] = Conversion.ToVector3(b.Position);
                    _positionBuffer.CopyFromSystemMemory(dirtyPosition, b.VertexBufferOffset * Vector3.SizeInBytes);

                    b.Dirty = false;
                }
                _dirtyBillboards.Clear();
            }

            _context.TextureUnits[0].Texture2D = _texture;
            _context.Bind(_renderState);
            _context.Bind(_sp);
            _context.Bind(_va);
            _context.Draw(PrimitiveType.Points, sceneState);
        }

        public IEnumerable<Billboard> Billboards
        {
            get { return _billboards; }
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

        #region IDisposable Members

        public void Dispose()
        {
            foreach (Billboard b in _billboards)
            {
                b.Dirty = false;
                b.Owner = null;
                b.VertexBufferOffset = 0;
            }

            _sp.Dispose();
            _positionBuffer.Dispose();
            _va.Dispose();
            _texture.Dispose();
        }

        #endregion

        internal void NotifyDirty(Billboard billboard)
        {
            _dirtyBillboards.Add(billboard);
        }

        private static int EnumerableCount<T>(IEnumerable<T> enumerable)
        {
            IList<T> list = enumerable as IList<T>;

            if (list != null)
            {
                return list.Count;
            }

            int count = 0;
            foreach (T t in enumerable)
            {
                ++count;
            }

            return count;
        }

        private readonly IEnumerable<Billboard> _billboards;
        private readonly IList<Billboard> _dirtyBillboards;
        private readonly Context _context;
        private readonly RenderState _renderState;
        private readonly ShaderProgram _sp;
        private readonly VertexBuffer _positionBuffer;
        private readonly VertexArray _va;
        private readonly Texture2D _texture;
    }
}