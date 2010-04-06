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

using MiniGlobe.Core.Geometry;
using MiniGlobe.Renderer;
using MiniGlobe.Scene;

using MiniGlobe.Core;
using MiniGlobe.Terrain;

namespace MiniGlobe.Examples.Chapter5
{
    sealed class DepthBufferPrecision : IDisposable
    {
        public DepthBufferPrecision()
        {
            _window = Device.CreateWindow(800, 600, "Chapter 4:  Depth Buffer Precision");
            _window.Resize += OnResize;
            _window.RenderFrame += OnRenderFrame;
            _window.Keyboard.KeyDown += OnKeyDown;
            _sceneState = new SceneState();

            _sceneState.Camera.PerspectiveFarPlaneDistance = 1;
            _sceneState.Camera.PerspectiveFarPlaneDistance = 4096;

            ///////////////////////////////////////////////////////////////////

            Ellipsoid globeShape = Ellipsoid.UnitSphere;
            _globe = new TessellatedGlobe(_window.Context);
            _globe.Shape = globeShape;
            _globe.Texture = Device.CreateTexture2D(new Bitmap("MapperWDB.jpg"), TextureFormat.RedGreenBlue8, false);

            _viewportQuad = new ViewportQuad(_window.Context);

            _frameBuffer = _window.Context.CreateFrameBuffer();
            _depthFormatIndex = 1;
            _depthFormatHUD = new HeadsUpDisplay(_window.Context);
            _depthFormatHUD.Color = Color.Black;
            _hudFont = new Font("Arial", 16);
            UpdateDepthFormatHUD();

            ///////////////////////////////////////////////////////////////////

            _camera = new CameraLookAtPoint(_sceneState.Camera, _window, globeShape);
            _sceneState.Camera.ZoomToTarget(globeShape.MaximumRadius);

            HighResolutionSnap snap = new HighResolutionSnap(_window, _sceneState);
            snap.ColorFilename = @"E:\Dropbox\My Dropbox\Book\Manuscript\DepthBufferPrecision\Figures\DepthBufferPrecision.png";
            snap.WidthInInches = 3;
            snap.DotsPerInch = 600;
        }

        private void OnResize()
        {
            _window.Context.Viewport = new Rectangle(0, 0, _window.Width, _window.Height);
            _sceneState.Camera.AspectRatio = _window.Width / (double)_window.Height;

            UpdateFrameBufferAttachments();
        }

        private void OnKeyDown(object sender, KeyboardKeyEventArgs e)
        {
            if ((e.Key == KeyboardKey.Left) || (e.Key == KeyboardKey.Right))
            {
               _depthFormatIndex += (e.Key == KeyboardKey.Right) ? 1 : -1;
                if (_depthFormatIndex < 0)
                {
                    _depthFormatIndex = 2;
                }
                else if (_depthFormatIndex > 2)
                {
                    _depthFormatIndex = 0;
                }

                UpdateFrameBufferAttachments();
                UpdateDepthFormatHUD();
            }
        }

        private void UpdateFrameBufferAttachments()
        {
            DisposeFrameBufferAttachments();
            _colorTexture = Device.CreateTexture2D(new Texture2DDescription(_window.Width, _window.Height, TextureFormat.RedGreenBlue8, false));
            _depthTexture = Device.CreateTexture2D(new Texture2DDescription(_window.Width, _window.Height, _depthFormats[_depthFormatIndex], false));
            _frameBuffer.ColorAttachments[0] = _colorTexture;
            _frameBuffer.DepthAttachment = _depthTexture;
            _viewportQuad.Texture = _colorTexture;
        }

        private void UpdateDepthFormatHUD()
        {
            if (_depthFormatHUD.Texture != null)
            {
                _depthFormatHUD.Texture.Dispose();
                _depthFormatHUD.Texture = null;
            }

            _depthFormatHUD.Texture = Device.CreateTexture2D(
                Device.CreateBitmapFromText("Depth Format: " + _depthFormatsStrings[_depthFormatIndex], _hudFont),
                TextureFormat.RedGreenBlueAlpha8, false);
        }

        private void OnRenderFrame()
        {
            Context context = _window.Context;

            //
            // Render to frame buffer
            //
            context.Bind(_frameBuffer);
            context.Clear(ClearBuffers.ColorAndDepthBuffer, Color.White, 1, 0);
            _globe.Render(_sceneState);
            _depthFormatHUD.Render(_sceneState);

            //
            // Render viewport quad to show contents of frame buffer's color buffer
            //
            context.Bind(null as FrameBuffer);
            _viewportQuad.Render(_sceneState);
        }

        #region IDisposable Members

        public void Dispose()
        {
            _window.Dispose();
            _camera.Dispose();
            _globe.Texture.Dispose();
            _globe.Dispose();
            _viewportQuad.Dispose();

            DisposeFrameBufferAttachments();
            _frameBuffer.Dispose();
            _depthFormatHUD.Texture.Dispose();
            _depthFormatHUD.Dispose();
            _hudFont.Dispose();
        }

        #endregion

        private void DisposeFrameBufferAttachments()
        {
            if (_colorTexture != null)
            {
                _colorTexture.Dispose();
                _colorTexture = null;
            }

            if (_depthTexture != null)
            {
                _depthTexture.Dispose();
                _depthTexture = null;
            }
        }

        private void Run(double updateRate)
        {
            _window.Run(updateRate);
        }

        static void Main()
        {
            using (DepthBufferPrecision example = new DepthBufferPrecision())
            {
                example.Run(30.0);
            }
        }

        private readonly MiniGlobeWindow _window;
        private readonly SceneState _sceneState;
        private readonly CameraLookAtPoint _camera;
        private readonly TessellatedGlobe _globe;
        private readonly ViewportQuad _viewportQuad;

        private Texture2D _colorTexture;
        private Texture2D _depthTexture;
        private readonly FrameBuffer _frameBuffer;
        private int _depthFormatIndex;

        private readonly HeadsUpDisplay _depthFormatHUD;
        private readonly Font _hudFont;
        
        private readonly TextureFormat[] _depthFormats = new TextureFormat[]
        {
            TextureFormat.Depth16,
            TextureFormat.Depth24,
            TextureFormat.Depth32f
        };
        private readonly string[] _depthFormatsStrings = new string[]
        {
            "16-bit fixed point",
            "24-bit fixed point",
            "32-bit floating point",
        };
    }
}