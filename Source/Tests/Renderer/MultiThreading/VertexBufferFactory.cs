#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System.Threading;
using OpenGlobe.Core;

namespace OpenGlobe.Renderer
{
    internal class VertexBufferFactory : Disposable
    {
        public VertexBufferFactory(Context context, Vector3F[] positions)
        {
            _context = context;
            _positions = positions;
        }

        public void Create()
        {
            _context.MakeCurrent();

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
            }
            base.Dispose(disposing);
        }

        #endregion

        private readonly Context _context;
        private Vector3F[] _positions;
        private VertexBuffer _vertexBuffer;
    }
}
