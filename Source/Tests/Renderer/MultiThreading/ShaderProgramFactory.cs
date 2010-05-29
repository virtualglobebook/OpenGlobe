#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using MiniGlobe.Core;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace MiniGlobe.Renderer
{
    internal class ShaderProgramFactory : Disposable
    {
        public ShaderProgramFactory(string vertexShader, string fragmentShader)
        {
            _vs = vertexShader;
            _fs = fragmentShader;
        }

        public void Create()
        {
            //_window = new NativeWindow();
            //_context = new GraphicsContext(new GraphicsMode(32, 24, 8), _window.WindowInfo, 3, 2, GraphicsContextFlags.ForwardCompatible | GraphicsContextFlags.Debug);
            //_context.MakeCurrent(_window.WindowInfo);
            //_window = Device.CreateWindow(1, 1);

            _sp = Device.CreateShaderProgram(_vs, _fs);

            // TODO:  Don't call Flush directly.
            GL.Flush();
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
                _sp.Dispose();
                //_window.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion

        //private NativeWindow _window;
        //private GraphicsContext _context;
        //private MiniGlobeWindow _window;

        private string _vs;
        private string _fs;
        private ShaderProgram _sp;
    }
}
