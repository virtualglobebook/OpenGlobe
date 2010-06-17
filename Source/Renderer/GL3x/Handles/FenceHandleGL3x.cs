#region License
//
// (C) Copyright 2010 Patrick Cozzi, Deron Ohlarik, and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using OpenTK.Graphics.OpenGL;

namespace OpenGlobe.Renderer.GL3x
{
    internal sealed class FenceHandleGL3x : IDisposable
    {
        public FenceHandleGL3x()
        {
            _value = GL.FenceSync(ArbSync.SyncGpuCommandsComplete, 0);
        }

        ~FenceHandleGL3x()
        {
            FinalizerThreadContextGL3x.RunFinalizer(Dispose);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public IntPtr Value
        {
            get { return _value.Value; }
        }

        private void Dispose(bool disposing)
        {
            if (_value != null)
            {
                GL.DeleteSync(_value.Value);
                _value = null;
            }
        }

        private Nullable<IntPtr> _value;
    }
}
