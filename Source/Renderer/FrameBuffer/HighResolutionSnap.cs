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
        public HighResolutionSnap(MiniGlobeWindow window, SceneState sceneState)
        {
            _window = window;
            _sceneState = sceneState;

            window.Keyboard.KeyDown += delegate(object sender, KeyboardKeyEventArgs e)
            {
                if (e.Key == KeyboardKey.Space)
                {
                    Enable(true);
                }
            };
        }

        private void PreRenderFrame()
        {
            Context context = _window.Context;

            _snapBuffer = new HighResolutionSnapFrameBuffer(context, WidthInInches, DotsPerInch, _sceneState.Camera.AspectRatio);
            context.Bind(_snapBuffer.FrameBuffer);
            context.HighResolutionSnapFrameBufferBound = true;

            _previousViewport = context.Viewport;
            context.Viewport = new Rectangle(0, 0, _snapBuffer.WidthInPixels, _snapBuffer.HeightInPixels);

            _previousSnapScale = _sceneState.HighResolutionSnapScale;
            _sceneState.HighResolutionSnapScale = (double)context.Viewport.Width / (double)_previousViewport.Width;
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

            _window.Context.HighResolutionSnapFrameBufferBound = false;
            _window.Context.Bind(null as FrameBuffer);
            _window.Context.Viewport = _previousViewport;
            _sceneState.HighResolutionSnapScale = _previousSnapScale;

            Enable(false);
            _snapBuffer.Dispose();
            _snapBuffer = null;
        }

        #region IDisposable Members

        public void Dispose()
        {
            Enable(false);

            if (_snapBuffer != null)
            {
                _snapBuffer.Dispose();
            }
        }

        #endregion

        private void Enable(bool value)
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

        public string ColorFilename { get; set; }
        public string DepthFilename { get; set; }
        public double WidthInInches { get; set; }
        public int DotsPerInch { get; set; }

        public HighResolutionSnapFrameBuffer SnapBuffer
        {
            get { return _snapBuffer; }
        }

        private MiniGlobeWindow _window;
        private SceneState _sceneState;
        private bool _enabled;
        private HighResolutionSnapFrameBuffer _snapBuffer;

        private Rectangle _previousViewport;
        private double _previousSnapScale;
    }
}