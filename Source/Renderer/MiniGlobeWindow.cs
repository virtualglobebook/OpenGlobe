#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using OpenGlobe.Core;

namespace OpenGlobe.Renderer
{
    public abstract class GraphicsWindow : Disposable
    {
        public delegate void MiniGlobeHandler();

        public event MiniGlobeHandler Resize;
        public event MiniGlobeHandler UpdateFrame;

        public event MiniGlobeHandler PreRenderFrame;
        public event MiniGlobeHandler RenderFrame;
        public event MiniGlobeHandler PostRenderFrame;

        protected virtual void OnResize()
        {
            MiniGlobeHandler handler = Resize;
            if (handler != null)
            {
                handler();
            }
        }

        protected virtual void OnUpdateFrame()
        {
            MiniGlobeHandler handler = UpdateFrame;
            if (handler != null)
            {
                handler();
            }
        }

        protected virtual void OnPreRenderFrame()
        {
            MiniGlobeHandler handler = PreRenderFrame;
            if (handler != null)
            {
                handler();
            }
        }

        protected virtual void OnRenderFrame()
        {
            MiniGlobeHandler handler = RenderFrame;
            if (handler != null)
            {
                handler();
            }
        }

        protected virtual void OnPostRenderFrame()
        {
            MiniGlobeHandler handler = PostRenderFrame;
            if (handler != null)
            {
                handler();
            }
        }

        public abstract void MakeCurrent();
        public abstract void Run(double updateRate);
        public abstract Context Context { get; }
        public abstract int Width { get; }
        public abstract int Height { get; }
        public abstract Mouse Mouse { get; }
        public abstract Keyboard Keyboard { get; }
    }
}
