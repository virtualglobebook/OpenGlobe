#region License
//
// (C) Copyright 2009 Patrick Cozzi, Deron Ohlarik, and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;
using System.Threading;
using OpenTK.Graphics.OpenGL;

namespace OpenGlobe.Renderer.GL3x
{
    internal sealed class VertexArrayNameGL3x : IDisposable
    {
        public VertexArrayNameGL3x()
        {
            GL.GenVertexArrays(1, out _value);
            Interlocked.Increment(ref Device.VertexArrayCount);
        }

        ~VertexArrayNameGL3x()
        {
            FinalizerThreadContextGL3x.RunFinalizer(Dispose);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public int Value
        {
            get { return _value; }
        }

        private void Dispose(bool disposing)
        {
            if (_value != 0)
            {
                GL.DeleteVertexArrays(1, ref _value);
                _value = 0;
                Interlocked.Decrement(ref Device.VertexArrayCount);
            }
        }

        private int _value;
    }
}
