#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using MiniGlobe.Core;
using OpenTK.Graphics.OpenGL;

namespace MiniGlobe.Renderer
{
    internal class VertexBufferFactory : Disposable
    {
        public VertexBufferFactory(MiniGlobeWindow window, Vector3S[] positions)
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

            // TODO:  Don't call Flush directly.
            GL.Flush();
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
                _vertexBuffer.Dispose();
                _window.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion

        private readonly MiniGlobeWindow _window;
        private Vector3S[] _positions;
        private VertexBuffer _vertexBuffer;
    }
}
