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
    internal class ShaderProgramFactory : Disposable
    {
        public ShaderProgramFactory(Context context, string vertexShader, string fragmentShader)
        {
            _context = context;
            _vs = vertexShader;
            _fs = fragmentShader;
        }

        public void Create()
        {
            _context.MakeCurrent();

            _sp = Device.CreateShaderProgram(_vs, _fs);

            Fence fence = Device.CreateFence();
            while (fence.ClientWait(0) == ClientWaitResult.TimeoutExpired)
            {
                Thread.Sleep(10);
            }
        }

        public ShaderProgram ShaderProgram
        {
            get { return _sp; }
        }

        #region Disposable Members

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_sp != null)
                {
                    _sp.Dispose();
                    _sp = null;
                }
            }
            base.Dispose(disposing);
        }

        #endregion

        private readonly Context _context;
        private readonly string _vs;
        private readonly string _fs;
        private ShaderProgram _sp;
    }
}
