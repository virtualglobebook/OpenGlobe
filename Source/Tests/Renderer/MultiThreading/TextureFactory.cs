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
    internal class TextureFactory : Disposable
    {
        public TextureFactory(BlittableRGBA rgba)
        {
            _rgba = rgba;
        }

        public void Create(MiniGlobeWindow window)
        {
            window.MakeCurrent();
            _window = window;

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

        private BlittableRGBA _rgba;
        private Texture2D _texture;
        private MiniGlobeWindow _window;
    }
}
