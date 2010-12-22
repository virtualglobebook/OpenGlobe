#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;
using System.Drawing;
using System.Globalization;
using System.Collections.Generic;
using System.Diagnostics;
using OpenGlobe.Core;
using OpenGlobe.Renderer;

namespace OpenGlobe.Scene
{
    public sealed class PerformanceCountersDisplay : IDisposable
    {
        public PerformanceCountersDisplay(GraphicsWindow window, SceneState sceneState)
        {
            window.Context.PerformanceCountersEnabled = true;
            window.Resize += OnResize;
            window.PostRenderFrame += PostRenderFrame;
            
            _display = new HeadsUpDisplay();
            _display.Color = Color.Black;
            _display.VerticalOrigin = VerticalOrigin.Top;
            _display.HorizontalOrigin = HorizontalOrigin.Right;
            _display.Position = new Vector2D(window.Width, window.Height);
            _font = new Font(_fontName, 16);

            _window = window;
            _sceneState = sceneState;

            ShowNumberOfPointsRendered = true;
            ShowNumberOfLinesRendered = true;
            ShowNumberOfTrianglesRendered = true;
            ShowNumberOfPrimitivesRendered = true;
            ShowNumberOfDrawCalls = true;
            ShowNumberOfClearCalls = true;
            ShowMillisecondsPerFrame = true;
            ShowFramesPerSecond = true;
            ShowPointsPerSecond = true;
            ShowLinesPerSecond = true;
            ShowTrianglesPerSecond = true;
            ShowPrimitivesPerSecond = true;
        }

        public Color Color
        {
            get { return _display.Color; }
            set { _display.Color = value; }
        }

        public float FontPointSize
        {
            get { return _font.Size; }
            set
            {
                if (_font.Size != value)
                {
                    _font.Dispose();
                    _font = null;
                    _font = new Font(_fontName, value);
                }
            }
        }

        public bool ShowNumberOfPointsRendered { get; set; }
        public bool ShowNumberOfLinesRendered { get; set; }
        public bool ShowNumberOfTrianglesRendered { get; set; }
        public bool ShowNumberOfPrimitivesRendered { get; set; }
        public bool ShowNumberOfDrawCalls { get; set; }
        public bool ShowNumberOfClearCalls { get; set; }

        public bool ShowMillisecondsPerFrame { get; set; }
        public bool ShowFramesPerSecond { get; set; }
        public bool ShowPointsPerSecond { get; set; }
        public bool ShowLinesPerSecond { get; set; }
        public bool ShowTrianglesPerSecond { get; set; }
        public bool ShowPrimitivesPerSecond { get; set; }

        private void OnResize()
        {
            _display.Position = new Vector2D(_window.Width, _window.Height);
        }

        private void PostRenderFrame()
        {
            Context context = _window.Context;

            List<string> strings = new List<string>();

            if (ShowNumberOfPointsRendered && (context.NumberOfPointsRendered > 0))
            {
                strings.Add("points: " + string.Format(CultureInfo.CurrentCulture, "{0:n0}", context.NumberOfPointsRendered) + "\n");
            }

            if (ShowNumberOfLinesRendered && (context.NumberOfLinesRendered > 0))
            {
                strings.Add("lines: " + string.Format(CultureInfo.CurrentCulture, "{0:n0}", context.NumberOfLinesRendered) + "\n");
            }

            if (ShowNumberOfTrianglesRendered && (context.NumberOfTrianglesRendered > 0))
            {
                strings.Add("triangles: " + string.Format(CultureInfo.CurrentCulture, "{0:n0}", context.NumberOfTrianglesRendered) + "\n");
            }

            if (ShowNumberOfPrimitivesRendered && (context.NumberOfPrimitivesRendered > 0))
            {
                strings.Add("total primitives: " + string.Format(CultureInfo.CurrentCulture, "{0:n0}", context.NumberOfPrimitivesRendered) + "\n");
            }

            if (ShowNumberOfDrawCalls && (context.NumberOfDrawCalls > 0))
            {
                strings.Add("draw calls: " + string.Format(CultureInfo.CurrentCulture, "{0:n0}", context.NumberOfDrawCalls) + "\n");
            }

            if (ShowNumberOfClearCalls && (context.NumberOfClearCalls > 0))
            {
                strings.Add("clear calls: " + string.Format(CultureInfo.CurrentCulture, "{0:n0}", context.NumberOfClearCalls) + "\n");
            }

            if (ShowMillisecondsPerFrame)
            {
                strings.Add("ms/frame: " + string.Format(CultureInfo.CurrentCulture, "{0:N}", context.MillisecondsPerFrame) + "\n");
            }

            if (ShowFramesPerSecond)
            {
                strings.Add("fps: " + string.Format(CultureInfo.CurrentCulture, "{0:N}", context.FramesPerSecond) + "\n");
            }

            if (ShowPointsPerSecond && (context.PointsPerSecond > 0))
            {
                strings.Add("points/s: " + string.Format(CultureInfo.CurrentCulture, "{0:n0}", context.PointsPerSecond) + "\n");
            }

            if (ShowLinesPerSecond && (context.LinesPerSecond > 0))
            {
                strings.Add("lines/s: " + string.Format(CultureInfo.CurrentCulture, "{0:n0}", context.LinesPerSecond) + "\n");
            }

            if (ShowTrianglesPerSecond && (context.TrianglesPerSecond > 0))
            {
                strings.Add("triangles/s: " + string.Format(CultureInfo.CurrentCulture, "{0:n0}", context.TrianglesPerSecond) + "\n");
            }

            if (ShowPrimitivesPerSecond && (context.PrimitivesPerSecond > 0))
            {
                strings.Add("primitives/s: " + string.Format(CultureInfo.CurrentCulture, "{0:n0}", context.PrimitivesPerSecond) + "\n");
            }

            if (_display.Texture != null)
            {
                _display.Texture.Dispose();
                _display.Texture = null;
            }

            if (strings.Count != 0)
            {
                int maxLength = 0;
                for (int i = 0; i < strings.Count; ++i)
                {
                    maxLength = Math.Max(maxLength, strings[i].Length);
                }

                string text = "";
                for (int i = 0; i < strings.Count; ++i)
                {
                    text += strings[i].PadLeft(maxLength, ' ');
                }

                _display.Texture = Device.CreateTexture2D(
                    Device.CreateBitmapFromText(text, _font),
                    TextureFormat.RedGreenBlueAlpha8, false);
                _display.Render(context, _sceneState);
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            _window.Context.PerformanceCountersEnabled = false;
            _window.Resize -= OnResize;
            _window.PostRenderFrame -= PostRenderFrame;

            if (_display.Texture != null)
            {
                _display.Texture.Dispose();
            }
            _display.Dispose();
            _font.Dispose();
        }

        #endregion

        private GraphicsWindow _window;
        private SceneState _sceneState;
        private readonly HeadsUpDisplay _display;
        private Font _font;

        private const string _fontName = "Courier New";
    }
}