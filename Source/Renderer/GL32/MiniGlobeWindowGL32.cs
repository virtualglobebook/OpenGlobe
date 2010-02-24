#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using OpenTK;
using OpenTK.Graphics;

namespace MiniGlobe.Renderer.GL32
{
    internal class MiniGlobeWindowGL32 : MiniGlobeWindow
    {
        public MiniGlobeWindowGL32(int width, int height, string title, WindowType windowType)
        {
            GameWindowFlags fameWindowFlags = (windowType == WindowType.Default) ? GameWindowFlags.Default : GameWindowFlags.Fullscreen;
            if (windowType == WindowType.FullScreen)
            {
                width = DisplayDevice.Default.Width;
                height = DisplayDevice.Default.Height;
            }

            _gameWindw = new GameWindow(width, height, new GraphicsMode(32, 24, 8), title, fameWindowFlags,
                DisplayDevice.Default, 3, 2, GraphicsContextFlags.ForwardCompatible | GraphicsContextFlags.Debug);

            _gameWindw.Resize += new EventHandler<EventArgs>(this.OnResize);
            _gameWindw.UpdateFrame += new EventHandler<FrameEventArgs>(this.OnUpdateFrame);
            _gameWindw.RenderFrame += new EventHandler<FrameEventArgs>(this.OnRenderFrame);

            _context = new ContextGL32();

            _mouse = new MouseGL32(_gameWindw.Mouse);
            _keyboard = new KeyboardGL32(_gameWindw.Keyboard);
        }

        private void OnResize<T>(object sender, T e)
        {
            OnResize();
        }

        private void OnUpdateFrame<T>(object sender, T e)
        {
            OnUpdateFrame();
        }

        private void OnRenderFrame<T>(object sender, T e)
        {
            OnPreRenderFrame();
            OnRenderFrame();
            OnPostRenderFrame();
            _gameWindw.SwapBuffers();
        }

        #region MiniGlobeWindow Members

        public override void Run(double updateRate)
        {
            _gameWindw.Run(updateRate);
        }

        public override Context Context
        {
            get { return _context; }
        }

        public override int Width
        {
            get { return _gameWindw.Width; }
        }

        public override int Height
        {
            get { return _gameWindw.Height; }
        }

        public override Mouse Mouse
        {
            get { return _mouse; }
        }

        public override Keyboard Keyboard
        {
            get { return _keyboard; }
        }

        #endregion

        #region Disposable Members

        protected override void Dispose(bool disposing)
        {
            _gameWindw.Dispose();
        }

        #endregion

        private GameWindow _gameWindw;
        private ContextGL32 _context;
        private MouseGL32 _mouse;
        private KeyboardGL32 _keyboard;
    }
}
