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

namespace OpenGlobe.Renderer.GL3x
{
    internal class GraphicsWindowGL3x : GraphicsWindow
    {
        public GraphicsWindowGL3x(int width, int height, string title, WindowType windowType)
        {
            if (width < 0)
            {
                throw new ArgumentOutOfRangeException("width", "Width must be greater than or equal to zero.");
            }

            if (height < 0)
            {
                throw new ArgumentOutOfRangeException("height", "Height must be greater than or equal to zero.");
            }

            GameWindowFlags gameWindowFlags = (windowType == WindowType.Default) ? GameWindowFlags.Default : GameWindowFlags.Fullscreen;
            if (windowType == WindowType.FullScreen)
            {
                width = DisplayDevice.Default.Width;
                height = DisplayDevice.Default.Height;
            }
            
            _gameWindow = new GameWindow(width, height, new GraphicsMode(24, 24, 8), title, gameWindowFlags,
                DisplayDevice.Default, 3, 3, GraphicsContextFlags.ForwardCompatible | GraphicsContextFlags.Debug);

            FinalizerThreadContextGL3x.Initialize();
            _gameWindow.MakeCurrent();
            
            _gameWindow.Resize += new EventHandler<EventArgs>(this.OnResize);
            _gameWindow.UpdateFrame += new EventHandler<FrameEventArgs>(this.OnUpdateFrame);
            _gameWindow.RenderFrame += new EventHandler<FrameEventArgs>(this.OnRenderFrame);

            _context = new ContextGL3x(_gameWindow, width, height);

            _mouse = new MouseGL3x(_gameWindow.Mouse);
            _keyboard = new KeyboardGL3x(_gameWindow.Keyboard);
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
            _gameWindow.SwapBuffers();      
        }

        #region GraphicsWindow Members

        public override void Run(double updateRate)
        {
            _gameWindow.Run(updateRate);
        }

        public override Context Context
        {
            get { return _context; }
        }

        public override int Width
        {
            get { return _gameWindow.Width; }
        }

        public override int Height
        {
            get { return _gameWindow.Height; }
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
                _gameWindow.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion

        private GameWindow _gameWindow;
        private ContextGL3x _context;
        private MouseGL3x _mouse;
        private KeyboardGL3x _keyboard;
    }
}
