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
using System.Collections;

namespace MiniGlobe.Scene
{
    public sealed class BillboardGroup2 : IDisposable
    {
        public BillboardGroup2(Context context, Bitmap bitmap)
            : this(context, bitmap, 0)
        {
        }

        public BillboardGroup2(Context context, Bitmap bitmap, int capacity)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            if (bitmap == null)
            {
                throw new ArgumentNullException("bitmap");
            }

            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException("capacity");
            }

            ///////////////////////////////////////////////////////////////////

            _billboards = new List<Billboard>(capacity);
            _dirtyBillboards = new List<Billboard>();

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

            _texture = Device.CreateTexture2D(bitmap, TextureFormat.RedGreenBlueAlpha8, false);
        }

        private void CreateVertexArray(int count)
        {
            // TODO:  Hint
            _positionBuffer = Device.CreateVertexBuffer(BufferHint.StaticDraw, count * Vector3S.SizeInBytes);

            AttachedVertexBuffer attachedPositionBuffer = new AttachedVertexBuffer(
                _positionBuffer, VertexAttributeComponentType.Float, 3);
            _va = _context.CreateVertexArray();
            _va.VertexBuffers[_sp.VertexAttributes["position"].Location] = attachedPositionBuffer;
        }

        private void Update()
        {
            if (_billboardsAdded)
            {
                //
                // Since billboards were added, all billboards are rewritten
                // so dirty billboards are automatically cleaned.
                //
                _dirtyBillboards.Clear();
                
                //
                // Create vertex array with appropriately sized vertex buffers
                //
                DisposeVertexArray();
                CreateVertexArray(_billboards.Count);

                //
                // Write vertex buffers
                //
                Vector3S[] positions = new Vector3S[_billboards.Count];
                for (int i = 0; i < _billboards.Count; ++i)
                {
                    Billboard b = _billboards[i];

                    positions[i] = b.Position.ToVector3S();
                    b.VertexBufferOffset = i;
                    b.Dirty = false;
                }
                _positionBuffer.CopyFromSystemMemory(positions, 0);

                _billboardsAdded = false;
            }

            //if (_dirtyBillboards.Count != 0)
            //{
            //    Vector3S[] dirtyPosition = new Vector3S[1];
            //    foreach (Billboard b in _dirtyBillboards)
            //    {
            //        // TODO: Combine for performance
            //        dirtyPosition[0] = b.Position.ToVector3S();
            //        _positionBuffer.CopyFromSystemMemory(dirtyPosition, b.VertexBufferOffset * Vector3S.SizeInBytes);

            //        b.Dirty = false;
            //    }
            //    _dirtyBillboards.Clear();
            //}
        }

        public void Render(SceneState sceneState)
        {
            Update();

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

        #region Collection Members

        public void Add(Billboard billboard)
        {
            if (billboard == null)
            {
                throw new ArgumentNullException("billboard");
            }

            if (billboard.Owner != null)
            {
                if (billboard.Owner != this)
                {
                    throw new ArgumentException("billboard is already in another BillboardGroup.");
                }
                else
                {
                    throw new ArgumentException("billboard was already added to this BillboardGroup.");
                }
            }

            billboard.Owner = this;

            _billboards.Add(billboard);
            _billboardsAdded = true;
        }

        // TODO:  Remove()

        public Billboard this[int index] 
        {
            get { return _billboards[index]; }
            set { _billboards[index] = value; }
        }

        public int Count
        {
            get { return _billboards.Count; }
        }

        public IEnumerator GetEnumerator()
        {
            return _billboards.GetEnumerator();
        }

        #endregion

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
            _texture.Dispose();
            DisposeVertexArray();
        }

        #endregion

        private void DisposeVertexArray()
        {
            if (_positionBuffer != null)
            {
                _positionBuffer.Dispose();
            }

            if (_va != null)
            {
                _va.Dispose();
            }
        }

        internal void NotifyDirty(Billboard billboard)
        {
            _dirtyBillboards.Add(billboard);
        }

        //private static int EnumerableCount<T>(IEnumerable<T> enumerable)
        //{
        //    IList<T> list = enumerable as IList<T>;

        //    if (list != null)
        //    {
        //        return list.Count;
        //    }

        //    int count = 0;
        //    foreach (T t in enumerable)
        //    {
        //        ++count;
        //    }

        //    return count;
        //}

        private readonly IList<Billboard> _billboards;
        private readonly IList<Billboard> _dirtyBillboards;

        private bool _billboardsAdded;

        private readonly Context _context;
        private readonly RenderState _renderState;
        private readonly ShaderProgram _sp;
        private readonly Texture2D _texture;

        private VertexBuffer _positionBuffer;
        private VertexArray _va;
    }
}