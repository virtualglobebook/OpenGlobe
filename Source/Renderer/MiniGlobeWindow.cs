#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using MiniGlobe.Core;

namespace MiniGlobe.Renderer
{
    public abstract class MiniGlobeWindow : Disposable
    {
        public delegate void MiniGlobeHandler();

        public event MiniGlobeHandler Resize;
        public event MiniGlobeHandler UpdateFrame;
        public event MiniGlobeHandler RenderFrame;

        protected void RaiseResize()
        {
            if (Resize != null)
            {
                Resize();
            }
        }

        protected void RaiseUpdateFrame()
        {
            if (UpdateFrame != null)
            {
                UpdateFrame();
            }
        }

        protected void RaiseRenderFrame()
        {
            if (RenderFrame != null)
            {
                RenderFrame();
            }
        }

        public abstract void Run(double updateRate);
        public abstract Context Context { get; }
        public abstract int Width { get; }
        public abstract int Height { get; }
    }
}
