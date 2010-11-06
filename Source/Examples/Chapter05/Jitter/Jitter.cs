#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Drawing;
using System.Collections.Generic;
using System.Globalization;
using OpenGlobe.Core;
using OpenGlobe.Renderer;
using OpenGlobe.Scene;

namespace OpenGlobe.Examples
{
    sealed class Jitter : IDisposable
    {
        public Jitter()
        {
            double xTranslation = Ellipsoid.Wgs84.Radii.X;

            _window = Device.CreateWindow(800, 600, "Chapter 5:  Jitter");
            _window.Resize += OnResize;
            _window.RenderFrame += OnRenderFrame;
            _sceneState = new SceneState();

            _scene = new JitteryScene(_window.Context, xTranslation);

            ///////////////////////////////////////////////////////////////////

            _hudFont = new Font("Arial", 16);
            _hud = new HeadsUpDisplay(_window.Context);
            _hud.Color = Color.Black;

            ///////////////////////////////////////////////////////////////////

            Camera camera = _sceneState.Camera;
            camera.PerspectiveNearPlaneDistance = 0.1;
            camera.PerspectiveFarPlaneDistance = 1000000;
            camera.Target = Vector3D.UnitX * xTranslation;
            camera.Eye = Vector3D.UnitX * xTranslation * 1.1;

            _camera = new CameraLookAtPoint(camera, _window, Ellipsoid.UnitSphere);
            _camera.Range = (camera.Eye - camera.Target).Magnitude;
            _camera.MinimumZoomRate = 1;
            _camera.MaximumZoomRate = Double.MaxValue;
            _camera.ZoomFactor = 10;
            _camera.ZoomRateRangeAdjustment = 0;
        }

        private void UpdateHUD()
        {
            string text = "Distance: " + string.Format(CultureInfo.CurrentCulture, "{0:N}", _camera.Range);

            if (_hud.Texture != null)
            {
                _hud.Texture.Dispose();
                _hud.Texture = null;
            }
            _hud.Texture = Device.CreateTexture2D(
                Device.CreateBitmapFromText(text, _hudFont),
                TextureFormat.RedGreenBlueAlpha8, false);
        }

        private void OnResize()
        {
            _window.Context.Viewport = new Rectangle(0, 0, _window.Width, _window.Height);
            _sceneState.Camera.AspectRatio = _window.Width / (double)_window.Height;
        }

        private void OnRenderFrame()
        {
            UpdateHUD();

            Context context = _window.Context;
            context.Clear(ClearState.Default);

            _scene.Render(context, _sceneState);
            _hud.Render(context, _sceneState);
        }

        #region IDisposable Members

        public void Dispose()
        {
            _camera.Dispose();
            _hudFont.Dispose();
            _hud.Texture.Dispose();
            _hud.Dispose();
            _window.Dispose();
        }

        #endregion

        private void Run(double updateRate)
        {
            _window.Run(updateRate);
        }

        static void Main()
        {
            using (Jitter example = new Jitter())
            {
                example.Run(30.0);
            }
        }

        private readonly GraphicsWindow _window;
        private readonly SceneState _sceneState;

        private readonly IRenderable _scene;

        private readonly Font _hudFont;
        private readonly HeadsUpDisplay _hud;

        private readonly CameraLookAtPoint _camera;
    }
}