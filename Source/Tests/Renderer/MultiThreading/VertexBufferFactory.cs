#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System.Threading;
using OpenGlobe.Core;

namespace OpenGlobe.Renderer
{
    internal class VertexBufferFactory : Disposable
    {
        public VertexBufferFactory(GraphicsWindow window, Vector3S[] positions)
        {
            _window = window;
            _positions = positions;
        }

        public void Create()
        {
            _window.MakeCurrent();

            int vbSizeInBytes = ArraySizeInBytes.Size(_positions);
            _vertexBuffer = Device.CreateVertexBuffer(BufferHint.StaticDraw, vbSizeInBytes);
            _vertexBuffer.CopyFromSystemMemory(_positions);
            _positions = null;

            Fence fence = Device.CreateFence();
            while (fence.ClientWait(0) == ClientWaitResult.TimeoutExpired)
            {
                Thread.Sleep(10);
            }
        }

        public VertexBuffer VertexBuffer
        {
            get { return _vertexBuffer; }
        }

        #region Disposable Members

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_vertexBuffer != null)
                {
                    _vertexBuffer.Dispose();
                    _vertexBuffer = null;
                }

                _window.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion

        private readonly GraphicsWindow _window;
        private Vector3S[] _positions;
        private VertexBuffer _vertexBuffer;
    }
}
