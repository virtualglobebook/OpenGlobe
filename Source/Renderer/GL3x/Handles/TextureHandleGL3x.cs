#region License
//
// (C) Copyright 2009 Patrick Cozzi, Deron Ohlarik, and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using OpenTK.Graphics.OpenGL;

namespace MiniGlobe.Renderer.GL3x
{
    internal sealed class TextureHandleGL3x : IDisposable
    {
        public TextureHandleGL3x()
        {
            _value = GL.GenTexture();
        }

        ~TextureHandleGL3x()
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
                GL.DeleteTexture(_value);
                _value = 0;
            }
        }

        private int _value;
    }
}
