#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using MiniGlobe.Renderer;
using OpenTK.Graphics.OpenGL;

namespace MiniGlobe.Renderer.GL32
{
    internal class UniformBufferGL32 : UniformBuffer
    {
        public UniformBufferGL32(BufferHint usageHint, int sizeInBytes)
        {
            _bufferObject = new BufferGL32(BufferTarget.UniformBuffer, usageHint, sizeInBytes);
        }

        internal int Handle
        {
            get { return _bufferObject.Handle; }
        }

        #region UniformBuffer Members

        public override void CopyFromSystemMemory<T>(T[] bufferInSystemMemory, int destinationOffsetInBytes)
        {
            _bufferObject.CopyFromSystemMemory(bufferInSystemMemory, destinationOffsetInBytes);
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

        #endregion

        #region Disposable Members

        protected override void Dispose(bool disposing)
        {
            _bufferObject.Dispose(disposing);
            base.Dispose(disposing);
        }

        #endregion

        BufferGL32 _bufferObject;
    }
}
