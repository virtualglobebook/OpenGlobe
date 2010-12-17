#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System.Threading;
using OpenGlobe.Core;

namespace OpenGlobe.Renderer
{
    internal class TextureFactory : Disposable
    {
        public TextureFactory(Context context, BlittableRGBA rgba)
        {
            _context = context;
            _rgba = rgba;
        }

        public void Create()
        {
            _context.MakeCurrent();

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
                if (_texture != null)
                {
                    _texture.Dispose();
                    _texture = null;
                }
            }
            base.Dispose(disposing);
        }

        #endregion

        private readonly Context _context;
        private readonly BlittableRGBA _rgba;
        private Texture2D _texture;
    }
}
