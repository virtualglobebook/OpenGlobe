#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using OpenGlobe.Core;
using OpenTK.Graphics.OpenGL;
using System;

namespace OpenGlobe.Renderer.GL3x
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
            if (typeof(T) == typeof(ushort))
            {
                _dataType = IndexBufferDatatype.UnsignedShort;
            }
            else if (typeof(T) == typeof(uint))
            {
                _dataType = IndexBufferDatatype.UnsignedInt;
            }
            else
            {
                throw new ArgumentException(
                    "bufferInSystemMemory must be an array of ushort or uint.", 
                    "bufferInSystemMemory");
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

        public override IndexBufferDatatype Datatype
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
        IndexBufferDatatype _dataType;
        int _count;
    }
}
