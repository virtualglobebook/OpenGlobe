#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System.Diagnostics;
using MiniGlobe.Core;
using MiniGlobe.Renderer;
using OpenTK.Graphics.OpenGL;
using System.Runtime.InteropServices;

namespace MiniGlobe.Renderer.GL3x
{
    internal class IndexBufferGL3x : IndexBuffer
    {
        public IndexBufferGL3x(BufferHint usageHint, int sizeInBytes)
        {
            _bufferObject = new BufferGL3x(BufferTarget.ElementArrayBuffer, usageHint, sizeInBytes);
        }

        internal void Bind()
        {
            _bufferObject.Bind();
        }

        internal static void UnBind()
        {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }

        internal int Count
        {
            get { return _count; }
        }
        
        #region IndexBuffer Members

        public override void CopyFromSystemMemory<T>(
            T[] bufferInSystemMemory,
            int destinationOffsetInBytes,
            int lengthInBytes)
        {
            if (typeof(T) == typeof(byte))
            {
                _dataType = IndexBufferDataType.UnsignedByte;
            }
            else if (typeof(T) == typeof(ushort))
            {
                _dataType = IndexBufferDataType.UnsignedShort;
            }
            else
            {
                Debug.Assert(typeof(T) == typeof(uint));
                _dataType = IndexBufferDataType.UnsignedInt;
            }
            _count = _bufferObject.SizeInBytes / SizeInBytes<T>.Value;
            _bufferObject.CopyFromSystemMemory(bufferInSystemMemory, destinationOffsetInBytes, lengthInBytes);
        }

        public override T[] CopyToSystemMemory<T>(int offsetInBytes, int sizeInBytes)
        {
            return _bufferObject.CopyToSystemMemory<T>(offsetInBytes, sizeInBytes);
        }

        public override int SizeInBytes
        {
            get { return _bufferObject.SizeInBytes; }
        }

        public override BufferHint UsageHint
        {
            get { return _bufferObject.UsageHint; }
        }

        public override IndexBufferDataType DataType
        {
            get { return _dataType; }
        }

        #endregion

        #region Disposable Members

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _bufferObject.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion

        BufferGL3x _bufferObject;
        IndexBufferDataType _dataType;
        int _count;
    }
}
