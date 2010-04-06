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
            _globeShape = Ellipsoid.Wgs84;

            _window = Device.CreateWindow(800, 600, "Chapter 4:  Depth Buffer Precision");
            _window.Resize += OnResize;
            _window.RenderFrame += OnRenderFrame;
            _window.Keyboard.KeyDown += OnKeyDown;
            _sceneState = new SceneState();

            _sceneState.Camera.PerspectiveNearPlaneDistance = 1;
            _sceneState.Camera.PerspectiveFarPlaneDistance = 10 * _globeShape.MaximumRadius;

            ///////////////////////////////////////////////////////////////////
            Context context = _window.Context;

            _globe = new TessellatedGlobe(_window.Context);
            _globe.Shape = _globeShape;
            _globe.Texture = Device.CreateTexture2D(new Bitmap("MapperWDB.jpg"), TextureFormat.RedGreenBlue8, false);

            _plane = new Plane(_window.Context);
            _plane.XAxis = 0.6 * _globeShape.MaximumRadius * Vector3D.UnitX;
            _plane.YAxis = 0.6 * _globeShape.MinimumRadius * Vector3D.UnitZ;
            _plane.OutlineWidth = 3;
            _cubeRootPlaneAltitude = 100.0;
            UpdatePlaneOrigin();

            _viewportQuad = new ViewportQuad(_window.Context);

            _frameBuffer = _window.Context.CreateFrameBuffer();
            _depthFormatIndex = 1;
            
            ///////////////////////////////////////////////////////////////////

            _hudFont = new Font("Arial", 16);

            _depthFormatHUD = new HeadsUpDisplay(_window.Context);
            _depthFormatHUD.Color = Color.Black;
            UpdateDepthFormatHUD();

            _planeAltitudeHUD = new HeadsUpDisplay(_window.Context);
            _planeAltitudeHUD.Color = Color.Black;
            _planeAltitudeHUD.Position = new Vector2D(0, 24);
            UpdatePlaneFormatHUD();

            ///////////////////////////////////////////////////////////////////

            _camera = new CameraLookAtPoint(_sceneState.Camera, _window, _globeShape);
            _sceneState.Camera.ZoomToTarget(_globeShape.MaximumRadius);

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
            else if ((e.Key == KeyboardKey.Plus) || (e.Key == KeyboardKey.KeypadPlus) ||
                     (e.Key == KeyboardKey.Minus) || (e.Key == KeyboardKey.KeypadMinus))
            {
                _cubeRootPlaneAltitude += ((e.Key == KeyboardKey.Plus) || (e.Key == KeyboardKey.KeypadPlus)) ? 1 : -1;

                UpdatePlaneOrigin();
                UpdatePlaneFormatHUD();
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
            UpdateHUDTexture(_depthFormatHUD, "Depth Format: " + _depthFormatsStrings[_depthFormatIndex]);
        }

        private void UpdatePlaneOrigin()
        {
            _plane.Origin = -(_globeShape.MaximumRadius * Vector3D.UnitY +
                (_cubeRootPlaneAltitude * _cubeRootPlaneAltitude * _cubeRootPlaneAltitude * Vector3D.UnitY));
        }

        private void UpdatePlaneFormatHUD()
        {
            UpdateHUDTexture(_planeAltitudeHUD, "Plane Altitude: " +
                String.Format("{0:N}", _cubeRootPlaneAltitude * _cubeRootPlaneAltitude * _cubeRootPlaneAltitude));
        }

        private void UpdateHUDTexture(HeadsUpDisplay hud, string text)
        {
            if (hud.Texture != null)
            {
                hud.Texture.Dispose();
                hud.Texture = null;
            }

            hud.Texture = Device.CreateTexture2D(
                Device.CreateBitmapFromText(text, _hudFont),
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
            _plane.Render(_sceneState);

            _depthFormatHUD.Render(_sceneState);
            _planeAltitudeHUD.Render(_sceneState);

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
            _plane.Dispose();
            _viewportQuad.Dispose();

            DisposeFrameBufferAttachments();
            _frameBuffer.Dispose();

            _hudFont.Dispose();
            _depthFormatHUD.Texture.Dispose();
            _depthFormatHUD.Dispose();
            _planeAltitudeHUD.Texture.Dispose();
            _planeAltitudeHUD.Dispose();
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

        private readonly Ellipsoid _globeShape;

        private readonly MiniGlobeWindow _window;
        private readonly SceneState _sceneState;
        private readonly CameraLookAtPoint _camera;
        private readonly TessellatedGlobe _globe;
        private readonly Plane _plane;
        private double _cubeRootPlaneAltitude;
        private readonly ViewportQuad _viewportQuad;

        private Texture2D _colorTexture;
        private Texture2D _depthTexture;
        private readonly FrameBuffer _frameBuffer;
        private int _depthFormatIndex;

        private readonly Font _hudFont;
        private readonly HeadsUpDisplay _depthFormatHUD;
        private readonly HeadsUpDisplay _planeAltitudeHUD;
        
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