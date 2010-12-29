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
    public sealed class ResourceCountersDisplay : IDisposable
    {
        public ResourceCountersDisplay(GraphicsWindow window, SceneState sceneState)
        {
            window.Resize += OnResize;
            window.PostRenderFrame += PostRenderFrame;

            _display = new HeadsUpDisplay();
            _display.Color = Color.Black;
            _display.VerticalOrigin = VerticalOrigin.Bottom;
            _display.HorizontalOrigin = HorizontalOrigin.Right;
            _display.ShowBackground = true;
            _display.Position = new Vector2D(window.Width, window.Height);
            _font = new Font(_fontName, 16);

            _window = window;
            _sceneState = sceneState;

            VisibleCounters = ResourceCounters.All;
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

        public ResourceCounters VisibleCounters { get; set; }

        private void OnResize()
        {
            _display.Position = new Vector2D(_window.Width, 0);
        }

        private void PostRenderFrame()
        {
            Context context = _window.Context;

            //
            // Do not count the time it takes to render the display.
            //
            Device.Finish();
            bool enablePerformanceCounters = context.PerformanceCountersEnabled;
            context.PauseTiming();
            context.PerformanceCountersEnabled = false;

            try
            {
                List<string> strings = new List<string>();

                if (((VisibleCounters & ResourceCounters.NumberOfShaderProgramsCreated) == ResourceCounters.NumberOfShaderProgramsCreated) &&
                    (Device.NumberOfShaderProgramsCreated > 0))
                {
                    strings.Add("shader programs: " + string.Format(CultureInfo.CurrentCulture, "{0:n0}", Device.NumberOfShaderProgramsCreated) + "\n");
                }

                if (((VisibleCounters & ResourceCounters.NumberOfVertexBuffersCreated) == ResourceCounters.NumberOfVertexBuffersCreated) &&
                    (Device.NumberOfVertexBuffersCreated > 0))
                {
                    strings.Add("vertex buffers: " + string.Format(CultureInfo.CurrentCulture, "{0:n0}", Device.NumberOfVertexBuffersCreated) + "\n");
                }

                if (((VisibleCounters & ResourceCounters.NumberOfIndexBuffersCreated) == ResourceCounters.NumberOfIndexBuffersCreated) &&
                    (Device.NumberOfIndexBuffersCreated > 0))
                {
                    strings.Add("index buffers: " + string.Format(CultureInfo.CurrentCulture, "{0:n0}", Device.NumberOfIndexBuffersCreated) + "\n");
                }

                if (((VisibleCounters & ResourceCounters.NumberOfTexturesCreated) == ResourceCounters.NumberOfTexturesCreated) &&
                    (Device.NumberOfTexturesCreated > 0))
                {
                    strings.Add("textures: " + string.Format(CultureInfo.CurrentCulture, "{0:n0}", Device.NumberOfTexturesCreated) + "\n");
                }

                if (((VisibleCounters & ResourceCounters.NumberOfFencesCreated) == ResourceCounters.NumberOfFencesCreated) &&
                    (Device.NumberOfFencesCreated > 0))
                {
                    strings.Add("fences: " + string.Format(CultureInfo.CurrentCulture, "{0:n0}", Device.NumberOfFencesCreated) + "\n");
                }

                if (((VisibleCounters & ResourceCounters.NumberOfVertexArraysCreated) == ResourceCounters.NumberOfVertexArraysCreated) &&
                    (Device.NumberOfVertexArraysCreated > 0))
                {
                    strings.Add("vertex arrays: " + string.Format(CultureInfo.CurrentCulture, "{0:n0}", Device.NumberOfVertexArraysCreated) + "\n");
                }

                if (((VisibleCounters & ResourceCounters.NumberOfFramebuffersCreated) == ResourceCounters.NumberOfFramebuffersCreated) &&
                    (Device.NumberOfFramebuffersCreated > 0))
                {
                    strings.Add("frame buffers: " + string.Format(CultureInfo.CurrentCulture, "{0:n0}", Device.NumberOfFramebuffersCreated) + "\n");
                }

                if (((VisibleCounters & ResourceCounters.VertexBufferMemoryUsedInBytes) == ResourceCounters.VertexBufferMemoryUsedInBytes) &&
                    (Device.VertexBufferMemoryUsedInBytes > 0))
                {
                    strings.Add("vertex buffer memory: " + string.Format(CultureInfo.CurrentCulture, "{0:n0}", Device.VertexBufferMemoryUsedInBytes) + "\n");
                }

                if (((VisibleCounters & ResourceCounters.IndexBufferMemoryUsedInBytes) == ResourceCounters.IndexBufferMemoryUsedInBytes) &&
                    (Device.IndexBufferMemoryUsedInBytes > 0))
                {
                    strings.Add("index buffer memory: " + string.Format(CultureInfo.CurrentCulture, "{0:n0}", Device.IndexBufferMemoryUsedInBytes) + "\n");
                }

                if (((VisibleCounters & ResourceCounters.TextureMemoryUsedInBytes) == ResourceCounters.TextureMemoryUsedInBytes) &&
                    (Device.TextureMemoryUsedInBytes > 0))
                {
                    strings.Add("texture memory: " + string.Format(CultureInfo.CurrentCulture, "{0:n0}", Device.TextureMemoryUsedInBytes) + "\n");
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

                    using (Bitmap bitmap = Device.CreateBitmapFromText(text, _font))
                    {
                        _display.Texture = Device.CreateTexture2D(
                            bitmap, TextureFormat.RedGreenBlueAlpha8, false);
                    }
                    _display.Render(context, _sceneState);
                    Device.Finish();
                }
            }
            finally
            {
                context.PerformanceCountersEnabled = enablePerformanceCounters;
                context.ResumeTiming();
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
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