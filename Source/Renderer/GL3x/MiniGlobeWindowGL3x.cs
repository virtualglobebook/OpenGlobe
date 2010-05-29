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

namespace MiniGlobe.Renderer.GL3x
{
    internal class MiniGlobeWindowGL3x : MiniGlobeWindow
    {
        public MiniGlobeWindowGL3x(int width, int height, string title, WindowType windowType)
        {
            GameWindowFlags gameWindowFlags = (windowType == WindowType.Default) ? GameWindowFlags.Default : GameWindowFlags.Fullscreen;
            if (windowType == WindowType.FullScreen)
            {
                width = DisplayDevice.Default.Width;
                height = DisplayDevice.Default.Height;
            }

            _gameWindw = new GameWindow(width, height, new GraphicsMode(32, 24, 8), title, gameWindowFlags,
                DisplayDevice.Default, 3, 2, GraphicsContextFlags.ForwardCompatible | GraphicsContextFlags.Debug);

            FinalizerThreadContextGL3x.Initialize();
            _gameWindw.MakeCurrent();

            _gameWindw.Resize += new EventHandler<EventArgs>(this.OnResize);
            _gameWindw.UpdateFrame += new EventHandler<FrameEventArgs>(this.OnUpdateFrame);
            _gameWindw.RenderFrame += new EventHandler<FrameEventArgs>(this.OnRenderFrame);

            _context = new ContextGL3x();

            _mouse = new MouseGL3x(_gameWindw.Mouse);
            _keyboard = new KeyboardGL3x(_gameWindw.Keyboard);
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

        public override void MakeCurrent()
        {
            _gameWindw.MakeCurrent();
        }

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
            if (disposing)
            {
                _gameWindw.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion

        private GameWindow _gameWindw;
        private ContextGL3x _context;
        private MouseGL3x _mouse;
        private KeyboardGL3x _keyboard;
    }
}
