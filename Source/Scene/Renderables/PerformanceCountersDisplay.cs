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
            window.Context.ResetPerformanceCounters();
            window.Context.PerformanceCountersEnabled = true;
            window.Resize += OnResize;
            window.PostRenderFrame += PostRenderFrame;
            window.PostSwapBuffers += PostSwapBuffers;
            
            _display = new HeadsUpDisplay();
            _display.Color = Color.Black;
            _display.VerticalOrigin = VerticalOrigin.Top;
            _display.HorizontalOrigin = HorizontalOrigin.Right;
            _display.Position = new Vector2D(window.Width, window.Height);
            _font = new Font(_fontName, 16);

            _stopWatch = new Stopwatch();

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

            NumberOfSampledFrames = 30;
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

        /// <summary>
        /// Gets or sets the number of frames used to determine the
        /// elapsed milliseconds and frames per second.
        /// </summary>
        /// <remarks>
        /// Setting this value to one would only take the last rendered frame
        /// into account, where as setting this value to 30 would average the
        /// last 30 frames.
        /// </remarks>
        /// <seealso cref="MillisecondsPerFrame"/>
        /// <seealso cref="FramesPerSecond"/>
        public int NumberOfSampledFrames { get; set; }
        public double MillisecondsPerFrame { get { return _elapsedSeconds * 1000.0; } }
        public double FramesPerSecond { get { return 1.0 / _elapsedSeconds; } }
        public long PointsPerSecond { get { return _pointsPerSecond; } }
        public long LinesPerSecond { get { return _linesPerSecond; } }
        public long TrianglesPerSecond { get { return _trianglesPerSecond; } }
        public long PrimitivesPerSecond { get { return _primitivesPerSecond; } }

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
                strings.Add("ms/frame: " + string.Format(CultureInfo.CurrentCulture, "{0:N}", MillisecondsPerFrame) + "\n");
            }

            if (ShowFramesPerSecond)
            {
                strings.Add("fps: " + string.Format(CultureInfo.CurrentCulture, "{0:N}", FramesPerSecond) + "\n");
            }

            if (ShowPointsPerSecond && (_pointsPerSecond > 0))
            {
                strings.Add("points/s: " + string.Format(CultureInfo.CurrentCulture, "{0:n0}", _pointsPerSecond) + "\n");
            }

            if (ShowLinesPerSecond && (_linesPerSecond > 0))
            {
                strings.Add("lines/s: " + string.Format(CultureInfo.CurrentCulture, "{0:n0}", _linesPerSecond) + "\n");
            }

            if (ShowTrianglesPerSecond && (_trianglesPerSecond > 0))
            {
                strings.Add("triangles/s: " + string.Format(CultureInfo.CurrentCulture, "{0:n0}", _trianglesPerSecond) + "\n");
            }

            if (ShowPrimitivesPerSecond && (_primitivesPerSecond > 0))
            {
                strings.Add("primitives/s: " + string.Format(CultureInfo.CurrentCulture, "{0:n0}", _primitivesPerSecond) + "\n");
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

        private void PostSwapBuffers()
        {
            // TODO:  Move this event and related properties to
            // Context.  This will require some GraphicsWindow refactoring.

            _stopWatch.Stop();

            _elapsedSecondsSum += (double)_stopWatch.ElapsedTicks / (double)Stopwatch.Frequency;
            _pointsSum += _window.Context.NumberOfPointsRendered;
            _linesSum += _window.Context.NumberOfLinesRendered;
            _trianglesSum += _window.Context.NumberOfTrianglesRendered;
            _primitivesSum += _window.Context.NumberOfPrimitivesRendered;

            if (++_frameCount == NumberOfSampledFrames)
            {
                _elapsedSeconds = _elapsedSecondsSum / NumberOfSampledFrames;

                _pointsPerSecond = (long)((double)_pointsSum / _elapsedSecondsSum);
                _linesPerSecond = (long)((double)_linesSum / _elapsedSecondsSum);
                _trianglesPerSecond = (long)((double)_trianglesSum / _elapsedSecondsSum);
                _primitivesPerSecond = (long)((double)_primitivesSum / _elapsedSecondsSum);

                _elapsedSecondsSum = 0.0;
                _pointsSum = 0;
                _linesSum = 0;
                _trianglesSum = 0;
                _primitivesSum = 0;
                _frameCount = 0;
            }

            _window.Context.ResetPerformanceCounters();
            _stopWatch.Reset();
            _stopWatch.Start();
        }

        #region IDisposable Members

        public void Dispose()
        {
            _window.Context.ResetPerformanceCounters();
            _window.Context.PerformanceCountersEnabled = false;
            _window.Resize -= OnResize;
            _window.PostRenderFrame -= PostRenderFrame;
            _window.PostSwapBuffers -= PostSwapBuffers;

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

        private readonly Stopwatch _stopWatch;
        private double _elapsedSeconds;
        private double _elapsedSecondsSum;
        private int _frameCount;

        private long _pointsPerSecond;
        private long _linesPerSecond;
        private long _trianglesPerSecond;
        private long _primitivesPerSecond;

        private long _pointsSum;
        private long _linesSum;
        private long _trianglesSum;
        private long _primitivesSum;

        private const string _fontName = "Courier New";
    }
}