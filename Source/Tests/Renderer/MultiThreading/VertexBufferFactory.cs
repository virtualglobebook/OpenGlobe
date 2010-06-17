#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

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

            int vbSizeInBytes = _positions.Length * SizeInBytes<Vector3S>.Value;
            _vertexBuffer = Device.CreateVertexBuffer(BufferHint.StaticDraw, vbSizeInBytes);
            _vertexBuffer.CopyFromSystemMemory(_positions);
            _positions = null;

            _fence = Device.CreateFence();
        }

        public VertexBuffer VertexBuffer
        {
            get { return _vertexBuffer; }
        }

        public Fence Fence
        {
            get { return _fence; }
        }

        #region Disposable Members

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _fence.Dispose();
                _vertexBuffer.Dispose();
                _window.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion

        private readonly GraphicsWindow _window;
        private Vector3S[] _positions;
        private VertexBuffer _vertexBuffer;
        private Fence _fence;
    }
}
