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
    internal class TextureFactory : Disposable
    {
        public TextureFactory(BlittableRGBA rgba)
        {
            _rgba = rgba;
        }

        public void Create()
        {
            _window = new NativeWindow();
            _context = new GraphicsContext(new GraphicsMode(32, 24, 8), _window.WindowInfo, 3, 2, GraphicsContextFlags.ForwardCompatible | GraphicsContextFlags.Debug);
            _context.MakeCurrent(_window.WindowInfo);

            //_window = Device.CreateWindow(1, 1);
            _texture = TestUtility.CreateTexture(_rgba);

            // TODO:  Don't call Flush directly.
            GL.Flush();
        }

        public Texture2D Texture
        {
            get { return _texture; }
        }

        #region Disposable Members

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _texture.Dispose();
                _window.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion

        private NativeWindow _window;
        private GraphicsContext _context;
        //private MiniGlobeWindow _window;

        private Texture2D _texture;
        private BlittableRGBA _rgba;
    }
}
