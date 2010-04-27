#region License
//
// (C) Copyright 2009 Patrick Cozzi, Deron Ohlarik, and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;

namespace MiniGlobe.Renderer.GL3x
{
    internal sealed class BufferHandleGL3x : IDisposable
    {
        public BufferHandleGL3x(int handle)
        {
            _value = handle;
        }

        ~BufferHandleGL3x()
        {
            FinalizerThreadContextGL3x.MakeCurrent();
            Dispose(false);
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
                GL.DeleteBuffers(1, ref _value);
                _value = 0;
            }
        }

        private int _value;
    }
}
