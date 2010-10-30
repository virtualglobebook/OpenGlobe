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
    internal class TextureFactory : Disposable
    {
        public TextureFactory(GraphicsWindow window, BlittableRGBA rgba)
        {
            _window = window;
            _rgba = rgba;
        }

        public void Create()
        {
            _window.MakeCurrent();

            _texture = TestUtility.CreateTexture(_rgba);

            Fence fence = Device.CreateFence();
            while (fence.ClientWait(0) == ClientWaitResult.TimeoutExpired)
            {
                Thread.Sleep(10);
            }
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
            }
            base.Dispose(disposing);
        }

        #endregion

        private readonly GraphicsWindow _window;
        private readonly BlittableRGBA _rgba;
        private Texture2D _texture;
    }
}
