#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Drawing;

namespace MiniGlobe.Renderer
{
    public sealed class HighResolutionSnap : IDisposable
    {
        public HighResolutionSnap(MiniGlobeWindow window, Camera camera)
        {
            _window = window;
            _camera = camera;
        }

        private void PreRenderFrame()
        {
            Context context = _window.Context;

            _snapBuffer = new HighResolutionSnapFrameBuffer(context, WidthInInches, DotsPerInch, _camera.AspectRatio);
            context.Bind(_snapBuffer.FrameBuffer);

            _previousViewport = context.Viewport;
            context.Viewport = new Rectangle(0, 0, _snapBuffer.WidthInPixels, _snapBuffer.HeightInPixels);
        }

        private void PostRenderFrame()
        {
            if (ColorFilename != null)
            {
                _snapBuffer.SaveColorBuffer(ColorFilename);
            }

            if (DepthFilename != null)
            {
                _snapBuffer.SaveDepthBuffer(DepthFilename);
            }

            if (ExitAfterSnap)
            {
                Environment.Exit(0);
            }

            _window.Context.Viewport = _previousViewport;
            _window.Context.Bind(null as FrameBuffer);
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_enabled)
            {
                _window.PreRenderFrame -= PreRenderFrame;
                _window.PostRenderFrame -= PostRenderFrame;
            }

            _snapBuffer.Dispose();
        }

        #endregion

        public bool Enabled
        {
            get { return _enabled; }

            set
            {
                if (_enabled != value)
                {
                    if (value)
                    {
                        _window.PreRenderFrame += PreRenderFrame;
                        _window.PostRenderFrame += PostRenderFrame;
                    }
                    else
                    {
                        _window.PreRenderFrame -= PreRenderFrame;
                        _window.PostRenderFrame -= PostRenderFrame;
                    }

                    _enabled = value;
                }
            }
        }

        public string ColorFilename { get; set; }
        public string DepthFilename { get; set; }
        public double WidthInInches { get; set; }
        public int DotsPerInch { get; set; }

        public bool ExitAfterSnap { get; set; }

        public HighResolutionSnapFrameBuffer SnapBuffer
        {
            get { return _snapBuffer; }
        }

        private MiniGlobeWindow _window;
        private Camera _camera;
        private bool _enabled;
        private HighResolutionSnapFrameBuffer _snapBuffer;
        private Rectangle _previousViewport;
    }
}