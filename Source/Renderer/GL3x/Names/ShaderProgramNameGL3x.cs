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
    internal sealed class ShaderProgramNameGL3x : IDisposable
    {
        public ShaderProgramNameGL3x()
        {
            _value = GL.CreateProgram();
            Interlocked.Increment(ref Device.ShaderProgramsCount);
        }

        ~ShaderProgramNameGL3x()
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
                GL.DeleteProgram(_value);
                _value = 0;
                Interlocked.Decrement(ref Device.ShaderProgramsCount);
            }
        }

        private int _value;
    }
}
