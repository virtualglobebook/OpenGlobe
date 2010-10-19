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
using OpenGlobe.Core;
using OpenGlobe.Core.Geometry;
using OpenGlobe.Renderer;
using System.Collections;

namespace OpenGlobe.Scene
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

            RenderState renderState = new RenderState();
            renderState.FacetCulling.Enabled = false;
            renderState.Blending.Enabled = true;
            renderState.Blending.SourceRGBFactor = SourceBlendingFactor.SourceAlpha;
            renderState.Blending.SourceAlphaFactor = SourceBlendingFactor.SourceAlpha;
            renderState.Blending.DestinationRGBFactor = DestinationBlendingFactor.OneMinusSourceAlpha;
            renderState.Blending.DestinationAlphaFactor = DestinationBlendingFactor.OneMinusSourceAlpha;

            ShaderProgram sp = Device.CreateShaderProgram(
                EmbeddedResources.GetText("OpenGlobe.Scene.Renderables.BillboardCollection.Shaders.BillboardsVS.glsl"),
                EmbeddedResources.GetText("OpenGlobe.Scene.Renderables.BillboardCollection.Shaders.BillboardsGS.glsl"),
                EmbeddedResources.GetText("OpenGlobe.Scene.Renderables.BillboardCollection.Shaders.BillboardsFS.glsl"));

            _drawState = new DrawState(renderState, sp, null);
        }

        private void CreateVertexArray(Context context)
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
                _colorBuffer, VertexAttributeComponentType.UnsignedByte, 4, true, 0, 0);
            AttachedVertexBuffer attachedOriginBuffer = new AttachedVertexBuffer(
                _originBuffer, VertexAttributeComponentType.UnsignedByte, 1);
            AttachedVertexBuffer attachedPixelOffsetBuffer = new AttachedVertexBuffer(
                _pixelOffsetBuffer, VertexAttributeComponentType.HalfFloat, 2);
            AttachedVertexBuffer attachedTextureCoordinatesBuffer = new AttachedVertexBuffer(
                _textureCoordinatesBuffer, VertexAttributeComponentType.HalfFloat, 4);

            ShaderProgram sp = _drawState.ShaderProgram;
            VertexArray va = context.CreateVertexArray();
            va.VertexBuffers[sp.VertexAttributes["position"].Location] = attachedPositionBuffer;
            va.VertexBuffers[sp.VertexAttributes["textureCoordinates"].Location] = attachedTextureCoordinatesBuffer;
            va.VertexBuffers[sp.VertexAttributes["color"].Location] = attachedColorBuffer;
            va.VertexBuffers[sp.VertexAttributes["origin"].Location] = attachedOriginBuffer;
            va.VertexBuffers[sp.VertexAttributes["pixelOffset"].Location] = attachedPixelOffsetBuffer;

            _drawState.VertexArray = va;
        }

        private void Update(Context context)
        {
            if (_rewriteBillboards)
            {
                UpdateAll(context);
            }
            else if (_dirtyBillboards.Count != 0)
            {
                UpdateDirty();
            }
        }

        private void UpdateAll(Context context)
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
                CreateVertexArray(context);

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

        public void Render(Context context, SceneState sceneState)
        {
            Verify.ThrowIfNull(context);
            Verify.ThrowInvalidOperationIfNull(Texture, "Texture");

            Update(context);

            if (_drawState.VertexArray != null)
            {
                context.TextureUnits[0].Texture2D = Texture;
                context.Draw(PrimitiveType.Points, _drawState, sceneState);
            }
        }

        public Texture2D Texture { get; set; }

        public bool Wireframe
        {
            get { return _drawState.RenderState.RasterizationMode == RasterizationMode.Line; }
            set { _drawState.RenderState.RasterizationMode = value ? RasterizationMode.Line : RasterizationMode.Fill; }
        }

        public bool DepthTestEnabled
        {
            get { return _drawState.RenderState.DepthTest.Enabled; }
            set { _drawState.RenderState.DepthTest.Enabled = value; }
        }

        public bool DepthWrite
        {
            get { return _drawState.RenderState.DepthMask; }
            set { _drawState.RenderState.DepthMask = value; }
        }

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

            _drawState.ShaderProgram.Dispose();
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

            if (_drawState.VertexArray != null)
            {
                _drawState.VertexArray.Dispose();
                _drawState.VertexArray = null;
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

        private readonly DrawState _drawState;

        private VertexBuffer _positionBuffer;
        private VertexBuffer _textureCoordinatesBuffer;
        private VertexBuffer _colorBuffer;
        private VertexBuffer _originBuffer;
        private VertexBuffer _pixelOffsetBuffer;
    }
}