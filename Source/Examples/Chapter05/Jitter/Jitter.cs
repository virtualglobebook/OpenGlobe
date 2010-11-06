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
    public enum JitterAlgorithm
    {
        Jittery,
        JitterFreeSceneRelativeToCenter,
        JitterFreeSceneCPURelativeToEye,
        JitterFreeSceneGPURelativeToEye,
    }

    sealed class Jitter : IDisposable
    {
        public Jitter()
        {
            _xTranslation = Ellipsoid.Wgs84.Radii.X;
            _triangleDelta = 0.5;

            _window = Device.CreateWindow(800, 600, "Chapter 5:  Jitter");
            _window.Resize += OnResize;
            _window.RenderFrame += OnRenderFrame;
            _window.Keyboard.KeyUp += OnKeyUp;
            _sceneState = new SceneState();
            _clearState = new ClearState();

            ///////////////////////////////////////////////////////////////////

            _hudFont = new Font("Arial", 16);
            _hud = new HeadsUpDisplay(_window.Context);
            _hud.Color = Color.Black;

            ///////////////////////////////////////////////////////////////////

            Camera camera = _sceneState.Camera;
            camera.PerspectiveNearPlaneDistance = 0.1;
            camera.PerspectiveFarPlaneDistance = 1000000;
            camera.Target = Vector3D.UnitX * _xTranslation;
            camera.Eye = Vector3D.UnitX * _xTranslation * 1.1;

            _camera = new CameraLookAtPoint(camera, _window, Ellipsoid.UnitSphere);
            _camera.Range = (camera.Eye - camera.Target).Magnitude;
            _camera.MinimumZoomRate = 1;
            _camera.MaximumZoomRate = Double.MaxValue;
            _camera.ZoomFactor = 10;
            _camera.ZoomRateRangeAdjustment = 0;

            ///////////////////////////////////////////////////////////////////

            _scene = new JitteryScene(_window.Context, _xTranslation, _triangleDelta);

            ///////////////////////////////////////////////////////////////////

            PersistentView.Execute(@"E:\Manuscript\VertexTransformPrecision\Figures\aaa.xml", _window, _sceneState.Camera);
            HighResolutionSnap snap = new HighResolutionSnap(_window, _sceneState);
            snap.ColorFilename = @"E:\Manuscript\VertexTransformPrecision\Figures\aaa.png";
            snap.WidthInInches = 3;
            snap.DotsPerInch = 600;
        }

        private static string JitterAlgorithmToString(JitterAlgorithm algorithm)
        {
            switch (algorithm)
            {
                case JitterAlgorithm.Jittery:
                    return "Relative to World [Jittery]";
                case JitterAlgorithm.JitterFreeSceneRelativeToCenter:
                    return "Realtive to Center";
                case JitterAlgorithm.JitterFreeSceneCPURelativeToEye:
                    return "CPU Relative to Eye";
                case JitterAlgorithm.JitterFreeSceneGPURelativeToEye:
                    return "GPU Relative To Eye";
            }

            return string.Empty;
        }

        private void UpdateHUD()
        {
            string text;

            text = "Algorithm: " + JitterAlgorithmToString(_jitterAlgorithm) + " (left/right)\n";
            text += "Distance: " + string.Format(CultureInfo.CurrentCulture, "{0:N}", _camera.Range);

            if (_hud.Texture != null)
            {
                _hud.Texture.Dispose();
                _hud.Texture = null;
            }
            _hud.Texture = Device.CreateTexture2D(
                Device.CreateBitmapFromText(text, _hudFont),
                TextureFormat.RedGreenBlueAlpha8, false);
        }

        private void OnKeyUp(object sender, KeyboardKeyEventArgs e)
        {
            if ((e.Key == KeyboardKey.Left) || (e.Key == KeyboardKey.Right))
            {
                _jitterAlgorithm += (e.Key == KeyboardKey.Right) ? 1 : -1;

                if (_jitterAlgorithm < JitterAlgorithm.Jittery)
                {
                    _jitterAlgorithm = JitterAlgorithm.JitterFreeSceneGPURelativeToEye;
                }
                else if (_jitterAlgorithm > JitterAlgorithm.JitterFreeSceneGPURelativeToEye)
                {
                    _jitterAlgorithm = JitterAlgorithm.Jittery;
                }

                ((IDisposable)_scene).Dispose();
                _scene = null;

                switch (_jitterAlgorithm)
                {
                    case JitterAlgorithm.Jittery:
                        _scene = new JitteryScene(_window.Context, _xTranslation, _triangleDelta);
                        break;
                    case JitterAlgorithm.JitterFreeSceneRelativeToCenter:
                    case JitterAlgorithm.JitterFreeSceneCPURelativeToEye:
                    case JitterAlgorithm.JitterFreeSceneGPURelativeToEye:
                        _scene = new JitterFreeSceneCPURelativeToEye(_window.Context, _xTranslation, _triangleDelta);
                        break;
                }
            }
            else if ((e.Key == KeyboardKey.Down) || (e.Key == KeyboardKey.Up))
            {
                _camera.Range += (e.Key == KeyboardKey.Down) ? 0.01 : -0.01;
            }
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
            context.Clear(_clearState);

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

        private readonly double _xTranslation;
        private readonly double _triangleDelta;

        private readonly GraphicsWindow _window;
        private readonly SceneState _sceneState;
        private readonly ClearState _clearState;

        private readonly Font _hudFont;
        private readonly HeadsUpDisplay _hud;

        private readonly CameraLookAtPoint _camera;

        private IRenderable _scene;
        private JitterAlgorithm _jitterAlgorithm;
    }
}