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

        protected virtual void OnResize()
        {
            GraphicsHandler handler = Resize;
            if (handler != null)
            {
                handler();
            }
        }

        protected virtual void OnUpdateFrame()
        {
            GraphicsHandler handler = UpdateFrame;
            if (handler != null)
            {
                handler();
            }
        }

        protected virtual void OnPreRenderFrame()
        {
            GraphicsHandler handler = PreRenderFrame;
            if (handler != null)
            {
                handler();
            }
        }

        protected virtual void OnRenderFrame()
        {
            GraphicsHandler handler = RenderFrame;
            if (handler != null)
            {
                handler();
            }
        }

        protected virtual void OnPostRenderFrame()
        {
            GraphicsHandler handler = PostRenderFrame;
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
