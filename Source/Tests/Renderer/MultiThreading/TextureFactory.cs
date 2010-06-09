#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using MiniGlobe.Core;

namespace MiniGlobe.Renderer
{
    internal class TextureFactory : Disposable
    {
        public TextureFactory(MiniGlobeWindow window, BlittableRGBA rgba)
        {
            _window = window;
            _rgba = rgba;
        }

        public void Create()
        {
            _window.MakeCurrent();

            _texture = TestUtility.CreateTexture(_rgba);
            _window.Context.Finish();
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

        private readonly MiniGlobeWindow _window;
        private readonly BlittableRGBA _rgba;
        private Texture2D _texture;
    }
}
