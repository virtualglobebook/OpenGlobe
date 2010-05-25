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
using System.Collections;

namespace MiniGlobe.Scene
{
    public sealed class BillboardCollection : IList<Billboard>, IDisposable
    {
        public BillboardCollection(Context context)
            : this(context, 0)
        {
        }

        public BillboardCollection(Context context, int capacity)
        {
            Verify.ThrowIfNull(context);

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
                  in vec4 textureCoordinates;
                  in vec4 color;
                  in float origin;                  // TODO:  Why does this not work when float is int?
                  in vec2 pixelOffset;

                  out vec4 gsTextureCoordinates;
                  out vec4 gsColor;
                  out float gsOrigin;
                  out vec2 gsPixelOffset;

                  uniform mat4 mg_modelViewPerspectiveProjectionMatrix;
                  uniform mat4 mg_viewportTransformationMatrix;

                  vec4 WorldToWindowCoordinates(
                      vec4 v, 
                      mat4 modelViewPerspectiveProjectionMatrix, 
                      mat4 viewportTransformationMatrix)
                  {
                      v = modelViewPerspectiveProjectionMatrix * v;                        // clip coordinates
                      v.xyz /= v.w;                                                           // normalized device coordinates
                      v.xyz = (viewportTransformationMatrix * vec4(v.xyz + 1.0, 1.0)).xyz; // windows coordinates
                      return v;
                  }

                  void main()                     
                  {
                      gl_Position = WorldToWindowCoordinates(position, 
                          mg_modelViewPerspectiveProjectionMatrix, mg_viewportTransformationMatrix);
                      gsTextureCoordinates = textureCoordinates;
                      gsColor = color;
                      gsOrigin = origin;
                      gsPixelOffset = pixelOffset;
                  }";
            string gs =
                @"#version 150 

                  layout(points) in;
                  layout(triangle_strip, max_vertices = 4) out;

                  in vec4 gsTextureCoordinates[];
                  in vec4 gsColor[];
                  in float gsOrigin[];
                  in vec2 gsPixelOffset[];

                  out vec2 fsTextureCoordinates;
                  out vec4 fsColor;

                  uniform mat4 mg_viewportOrthographicProjectionMatrix;
                  uniform sampler2D mg_texture0;
                  uniform float mg_highResolutionSnapScale;
                  uniform vec4 mg_viewport;

                  void main()
                  {
                      float originScales[3] = float[](0.0, 1.0, -1.0);

                      vec4 textureCoordinate = gsTextureCoordinates[0];
                      vec2 atlasSize = vec2(textureSize(mg_texture0, 0));
                      vec2 subTextureSize = vec2(
                          atlasSize.x * (textureCoordinate.p - textureCoordinate.s), 
                          atlasSize.y * (textureCoordinate.q - textureCoordinate.t));
                      vec2 halfSize = subTextureSize * 0.5 * mg_highResolutionSnapScale;

                      vec4 center = gl_in[0].gl_Position;
                      int horizontalOrigin = int(gsOrigin[0]) & 3;         // bits 0-1
                      int verticalOrigin = (int(gsOrigin[0]) & 12) >> 2;   // bits 2-3
                      center.xy += (vec2(originScales[horizontalOrigin], originScales[verticalOrigin]) * halfSize);

                      center.xy += (gsPixelOffset[0] * mg_highResolutionSnapScale);

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
                      fsTextureCoordinates = textureCoordinate.st;
                      fsColor = gsColor[0];
                      EmitVertex();

                      gl_Position = mg_viewportOrthographicProjectionMatrix * v1;
                      fsTextureCoordinates = textureCoordinate.pt;
                      fsColor = gsColor[0];
                      EmitVertex();

                      gl_Position = mg_viewportOrthographicProjectionMatrix * v2;
                      fsTextureCoordinates = textureCoordinate.sq;
                      fsColor = gsColor[0];
                      EmitVertex();

                      gl_Position = mg_viewportOrthographicProjectionMatrix * v3;
                      fsTextureCoordinates = textureCoordinate.pq;
                      fsColor = gsColor[0];
                      EmitVertex();
                  }";
            string fs =
                @"#version 150
                 
                  in vec2 fsTextureCoordinates;
                  in vec4 fsColor;

                  out vec4 fragmentColor;

                  uniform sampler2D mg_texture0;
                  uniform float u_zOffset;

                  void main()
                  {
                      vec4 color = texture(mg_texture0, fsTextureCoordinates);

                      if (color.a == 0.0)
                      {
                          discard;
                      }
                      fragmentColor = vec4(color.rgb * fsColor.rgb, color.a);
                      gl_FragDepth = gl_FragCoord.z + u_zOffset;
                  }";
            _sp = Device.CreateShaderProgram(vs, gs, fs);
            _zOffsetUniform = _sp.Uniforms["u_zOffset"] as Uniform<float>;
        }

        private void CreateVertexArray()
        {
            // TODO:  Hint per buffer?  One hint?
            _positionBuffer = Device.CreateVertexBuffer(BufferHint.StaticDraw, _billboards.Count * SizeInBytes<Vector3S>.Value);
            _colorBuffer = Device.CreateVertexBuffer(BufferHint.StaticDraw, _billboards.Count * SizeInBytes<BlittableRGBA>.Value);
            _originBuffer = Device.CreateVertexBuffer(BufferHint.StaticDraw, _billboards.Count);
            _pixelOffsetBuffer = Device.CreateVertexBuffer(BufferHint.StaticDraw, _billboards.Count * SizeInBytes<Vector2H>.Value);
            _textureCoordinatesBuffer = Device.CreateVertexBuffer(BufferHint.StaticDraw, _billboards.Count * SizeInBytes<Vector4H>.Value);

            AttachedVertexBuffer attachedPositionBuffer = new AttachedVertexBuffer(
                _positionBuffer, VertexAttributeComponentType.Float, 3);
            AttachedVertexBuffer attachedColorBuffer = new AttachedVertexBuffer(
                _colorBuffer, VertexAttributeComponentType.UnsignedByte, 4, true);
            AttachedVertexBuffer attachedOriginBuffer = new AttachedVertexBuffer(
                _originBuffer, VertexAttributeComponentType.UnsignedByte, 1);
            AttachedVertexBuffer attachedPixelOffsetBuffer = new AttachedVertexBuffer(
                _pixelOffsetBuffer, VertexAttributeComponentType.HalfFloat, 2);
            AttachedVertexBuffer attachedTextureCoordinatesBuffer = new AttachedVertexBuffer(
                _textureCoordinatesBuffer, VertexAttributeComponentType.HalfFloat, 4);

            _va = _context.CreateVertexArray();
            _va.VertexBuffers[_sp.VertexAttributes["position"].Location] = attachedPositionBuffer;
            _va.VertexBuffers[_sp.VertexAttributes["textureCoordinates"].Location] = attachedTextureCoordinatesBuffer;
            _va.VertexBuffers[_sp.VertexAttributes["color"].Location] = attachedColorBuffer;
            _va.VertexBuffers[_sp.VertexAttributes["origin"].Location] = attachedOriginBuffer;
            _va.VertexBuffers[_sp.VertexAttributes["pixelOffset"].Location] = attachedPixelOffsetBuffer;
        }

        private void Update()
        {
            if (_rewriteBillboards)
            {
                UpdateAll();
            }
            else if (_dirtyBillboards.Count != 0)
            {
                UpdateDirty();
            }
        }

        private void UpdateAll()
        {
            //
            // Since billboards were added or removed, all billboards are 
            // rewritten so dirty billboards are automatically cleaned.
            //
            _dirtyBillboards.Clear();

            //
            // Create vertex array with appropriately sized vertex buffers
            //
            DisposeVertexArray();

            if (_billboards.Count != 0)
            {
                CreateVertexArray();

                //
                // Write vertex buffers
                //
                Vector3S[] positions = new Vector3S[_billboards.Count];
                Vector4H[] textureCoordinates = new Vector4H[_billboards.Count];
                BlittableRGBA[] colors = new BlittableRGBA[_billboards.Count];
                byte[] origins = new byte[_billboards.Count];
                Vector2H[] pixelOffets = new Vector2H[_billboards.Count];

                for (int i = 0; i < _billboards.Count; ++i)
                {
                    Billboard b = _billboards[i];

                    positions[i] = b.Position.ToVector3S();
                    textureCoordinates[i] = new Vector4H(
                        b.TextureCoordinates.LowerLeft.X, b.TextureCoordinates.LowerLeft.Y,
                        b.TextureCoordinates.UpperRight.X, b.TextureCoordinates.UpperRight.Y);
                    colors[i] = new BlittableRGBA(b.Color);
                    origins[i] = BillboardOrigin(b);
                    pixelOffets[i] = b.PixelOffset;

                    b.VertexBufferOffset = i;
                    b.Dirty = false;
                }
                CopyBillboardsFromSystemMemory(positions, textureCoordinates, colors, origins, pixelOffets, 0, _billboards.Count);

                _rewriteBillboards = false;
            }
        }

        private void UpdateDirty()
        {
            // PERFORMANCE:  Sort by buffer offset
            // PERFORMANCE:  Map buffer range
            // PERFORMANCE:  Round robin multiple buffers

            Vector3S[] positions = new Vector3S[_dirtyBillboards.Count];
            Vector4H[] textureCoordinates = new Vector4H[_dirtyBillboards.Count];
            BlittableRGBA[] colors = new BlittableRGBA[_dirtyBillboards.Count];
            byte[] origins = new byte[_dirtyBillboards.Count];
            Vector2H[] pixelOffets = new Vector2H[_dirtyBillboards.Count];

            int bufferOffset = _dirtyBillboards[0].VertexBufferOffset;
            int previousBufferOffset = bufferOffset - 1;
            int length = 0;

            for (int i = 0; i < _dirtyBillboards.Count; ++i)
            {
                Billboard b = _dirtyBillboards[i];

                if (previousBufferOffset != b.VertexBufferOffset - 1)
                {
                    CopyBillboardsFromSystemMemory(positions, textureCoordinates, colors, origins, pixelOffets, bufferOffset, length);

                    bufferOffset = b.VertexBufferOffset;
                    length = 0;
                }

                positions[length] = b.Position.ToVector3S();
                textureCoordinates[length] = new Vector4H(
                    b.TextureCoordinates.LowerLeft.X, b.TextureCoordinates.LowerLeft.Y,
                    b.TextureCoordinates.UpperRight.X, b.TextureCoordinates.UpperRight.Y);
                colors[length] = new BlittableRGBA(b.Color);
                origins[length] = BillboardOrigin(b);
                pixelOffets[length] = b.PixelOffset;
                ++length;

                previousBufferOffset = b.VertexBufferOffset;
                b.Dirty = false;
            }
            CopyBillboardsFromSystemMemory(positions, textureCoordinates, colors, origins, pixelOffets, bufferOffset, length);

            _dirtyBillboards.Clear();
        }

        private void CopyBillboardsFromSystemMemory(
            Vector3S[] positions,
            Vector4H[] textureCoordinates,
            BlittableRGBA[] colors,
            byte[] origins,
            Vector2H[] pixelOffsets,
            int bufferOffset,
            int length)
        {
            _positionBuffer.CopyFromSystemMemory(positions,
                bufferOffset * SizeInBytes<Vector3S>.Value,
                length * SizeInBytes<Vector3S>.Value);
            _textureCoordinatesBuffer.CopyFromSystemMemory(textureCoordinates,
                bufferOffset * SizeInBytes<Vector4H>.Value,
                length * SizeInBytes<Vector4H>.Value);
            _colorBuffer.CopyFromSystemMemory(colors,
                bufferOffset * SizeInBytes<BlittableRGBA>.Value,
                length * SizeInBytes<BlittableRGBA>.Value);
            _originBuffer.CopyFromSystemMemory(origins,
                bufferOffset,
                length);
            _pixelOffsetBuffer.CopyFromSystemMemory(pixelOffsets,
                bufferOffset * SizeInBytes<Vector2H>.Value,
                length * SizeInBytes<Vector2H>.Value);
        }

        private static byte BillboardOrigin(Billboard b)
        {
            return (byte)((byte)b.HorizontalOrigin | ((byte)b.VerticalOrigin << 2));
        }

        public void Render(SceneState sceneState)
        {
            Update();

            Verify.ThrowInvalidOperationIfNull(Texture, "Texture");

            if (_va != null)
            {
                _zOffsetUniform.Value = (float)ZOffset;
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

        // TODO:  Better way to avoid z fighting
        public double ZOffset { get; set; }

        #region IList<Billboard> Members

        public Billboard this[int index]
        {
            get { return _billboards[index]; }
            set
            {
                Billboard b = _billboards[index];

                AddBillboad(value);
                RemoveBillboad(b);
                _billboards[index] = value;
            }
        }

        public int IndexOf(Billboard item)
        {
            return _billboards.IndexOf(item);
        }

        public void Insert(int index, Billboard item)
        {
            if (index < Count)
            {
                Billboard b = _billboards[index];
                AddBillboad(item);
                RemoveBillboad(b);
            }
            else
            {
                AddBillboad(item);
            }

            _billboards.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            Billboard b = _billboards[index];
            RemoveBillboad(b);
            _billboards.RemoveAt(index);
        }

        #endregion

        #region ICollection<Billboard> Members

        public void Add(Billboard item)
        {
            AddBillboad(item);
            _billboards.Add(item);
        }

        public bool Remove(Billboard item)
        {
            bool b = _billboards.Remove(item);

            if (b)
            {
                RemoveBillboad(item);
            }

            return b;
        }

        public int Count
        {
            get { return _billboards.Count; }
        }

        public void Clear()
        {
            _billboards.Clear();
            _dirtyBillboards.Clear();
            _rewriteBillboards = true;
        }

        public bool Contains(Billboard item)
        {
            return _billboards.Contains(item);
        }

        public void CopyTo(Billboard[] array, int arrayIndex)
        {
            _billboards.CopyTo(array, arrayIndex);
        }

        public bool IsReadOnly
        {
            get { return _billboards.IsReadOnly; }
        }

        #endregion

        #region IEnumerable<Billboard> Members

        IEnumerator<Billboard> IEnumerable<Billboard>.GetEnumerator()
        {
            return _billboards.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _billboards.GetEnumerator();
        }

        #endregion

        private void AddBillboad(Billboard billboard)
        {
            if (billboard == null)
            {
                throw new ArgumentNullException("billboard");
            }

            if (billboard.Group != null)
            {
                if (billboard.Group != this)
                {
                    throw new ArgumentException("billboard is already in another BillboardCollection.");
                }
                else
                {
                    throw new ArgumentException("billboard was already added to this BillboardCollection.");
                }
            }

            billboard.Group = this;
            _rewriteBillboards = true;
        }

        private void RemoveBillboad(Billboard billboard)
        {
            if (billboard.Dirty)
            {
                _dirtyBillboards.Remove(billboard);
            }

            _rewriteBillboards = true;
            ReleaseBillboard(billboard);
        }

        internal void NotifyDirty(Billboard billboard)
        {
            _dirtyBillboards.Add(billboard);
        }

        #region IDisposable Members

        public void Dispose()
        {
            foreach (Billboard b in _billboards)
            {
                ReleaseBillboard(b);
            }

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

            if (_textureCoordinatesBuffer != null)
            {
                _textureCoordinatesBuffer.Dispose();
                _textureCoordinatesBuffer = null;
            }

            if (_colorBuffer != null)
            {
                _colorBuffer.Dispose();
                _colorBuffer = null;
            }

            if (_originBuffer != null)
            {
                _originBuffer.Dispose();
                _originBuffer = null;
            }

            if (_pixelOffsetBuffer != null)
            {
                _pixelOffsetBuffer.Dispose();
                _pixelOffsetBuffer = null;
            }

            if (_va != null)
            {
                _va.Dispose();
                _va = null;
            }
        }

        private static void ReleaseBillboard(Billboard billboard)
        {
            billboard.Dirty = false;
            billboard.Group = null;
            billboard.VertexBufferOffset = 0;
        }

        private readonly IList<Billboard> _billboards;
        private readonly IList<Billboard> _dirtyBillboards;

        private bool _rewriteBillboards;

        private readonly Context _context;
        private readonly RenderState _renderState;
        private readonly ShaderProgram _sp;
        private readonly Uniform<float> _zOffsetUniform;

        private VertexBuffer _positionBuffer;
        private VertexBuffer _textureCoordinatesBuffer;
        private VertexBuffer _colorBuffer;
        private VertexBuffer _originBuffer;
        private VertexBuffer _pixelOffsetBuffer;
        private VertexArray _va;
    }
}