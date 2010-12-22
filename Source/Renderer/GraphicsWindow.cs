#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using OpenGlobe.Core;

namespace OpenGlobe.Renderer
{
    public abstract class GraphicsWindow : Disposable
    {
        public delegate void GraphicsHandler();

        public event GraphicsHandler Resize;
        public event GraphicsHandler UpdateFrame;

        public event GraphicsHandler PreRenderFrame;
        public event GraphicsHandler RenderFrame;
        public event GraphicsHandler PostRenderFrame;
        public event GraphicsHandler PostSwapBuffers;

        protected virtual void OnResize()
        {
            Handler(Resize);
        }

        protected virtual void OnUpdateFrame()
        {
            Handler(UpdateFrame);
        }

        protected virtual void OnPreRenderFrame()
        {
            Handler(PreRenderFrame);
        }

        protected virtual void OnRenderFrame()
        {
            Handler(RenderFrame);
        }

        protected virtual void OnPostRenderFrame()
        {
            Handler(PostRenderFrame);
        }

        protected virtual void OnPostSwapBuffers()
        {
            Handler(PostSwapBuffers);
        }

        private void Handler(GraphicsHandler handler)
        {
            if (handler != null)
            {
                handler();
            }
        }

        public abstract void Run(double updateRate);
        public abstract Context Context { get; }
        public abstract int Width { get; }
        public abstract int Height { get; }
        public abstract Mouse Mouse { get; }
        public abstract Keyboard Keyboard { get; }
    }
}
