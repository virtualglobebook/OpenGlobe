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
using System.Collections.Generic;
using System.Globalization;
using OpenGlobe.Core;
using OpenGlobe.Renderer;
using OpenGlobe.Scene;

namespace OpenGlobe.Examples
{
    public enum JitterAlgorithm
    {
        RelativeToWorld,
        RelativeToCenter,
        CPURelativeToEye,
        GPURelativeToEye,
        GPURelativeToEyeDSFUN90,
        GPURelativeToEyeLOD,
    }

    sealed class Jitter : IDisposable
    {
        public Jitter()
        {
            _window = Device.CreateWindow(800, 600, "Chapter 5:  Jitter");
            _window.Resize += OnResize;
            _window.RenderFrame += OnRenderFrame;
            _window.Keyboard.KeyUp += OnKeyUp;
            _sceneState = new SceneState();
            _clearState = new ClearState();

            _hudFont = new Font("Arial", 16);
            _hud = new HeadsUpDisplay();
            _hud.Color = Color.Black;

            CreateCamera();
            CreateAlgorithm();

            PersistentView.Execute(@"E:\Manuscript\VertexTransformPrecision\Figures\aaa.xml", _window, _sceneState.Camera);
            HighResolutionSnap snap = new HighResolutionSnap(_window, _sceneState);
            snap.ColorFilename = @"E:\Manuscript\VertexTransformPrecision\Figures\aaa.png";
            snap.WidthInInches = 3;
            snap.DotsPerInch = 600;
        }

        private double ToMeters(double value)
        {
            return _scaleWorldCoordinates ? (value * Ellipsoid.Wgs84.MaximumRadius) : value;
        }

        private double FromMeters(double value)
        {
            return _scaleWorldCoordinates ? (value / Ellipsoid.Wgs84.MaximumRadius) : value;
        }

        private static string JitterAlgorithmToString(JitterAlgorithm algorithm)
        {
            switch (algorithm)
            {
                case JitterAlgorithm.RelativeToWorld:
                    return "Relative to World [Jittery]";
                case JitterAlgorithm.RelativeToCenter:
                    return "Realtive to Center";
                case JitterAlgorithm.CPURelativeToEye:
                    return "CPU Relative to Eye";
                case JitterAlgorithm.GPURelativeToEye:
                    return "GPU Relative To Eye";
                case JitterAlgorithm.GPURelativeToEyeDSFUN90:
                    return "GPU Relative To Eye [DSFUN90]";
                case JitterAlgorithm.GPURelativeToEyeLOD:
                    return "GPU Relative To Eye [LOD]";
            }

            return string.Empty;
        }

        private void UpdateHUD()
        {
            string text;

            text = "Scale World Coordinates: " + _scaleWorldCoordinates + " ('s')\n";
            text += "Algorithm: " + JitterAlgorithmToString(_jitterAlgorithm) + " (left/right)\n";
            text += "Distance: " + string.Format(CultureInfo.CurrentCulture, "{0:N}", ToMeters(_camera.Range));

            if (_hud.Texture != null)
            {
                _hud.Texture.Dispose();
                _hud.Texture = null;
            }
            _hud.Texture = Device.CreateTexture2D(
                Device.CreateBitmapFromText(text, _hudFont),
                TextureFormat.RedGreenBlueAlpha8, false);
        }

        private void CreateCamera()
        {
            _xTranslation = FromMeters(Ellipsoid.Wgs84.Radii.X);

            Camera camera = _sceneState.Camera;
            camera.PerspectiveNearPlaneDistance = FromMeters(0.01);
            camera.PerspectiveFarPlaneDistance = FromMeters(5000000);
            camera.Target = Vector3D.UnitX * _xTranslation;
            camera.Eye = Vector3D.UnitX * _xTranslation * 1.1;

            if (_camera != null)
            {
                ((IDisposable)_camera).Dispose();
                _camera = null;
            }

            _camera = new CameraLookAtPoint(camera, _window, Ellipsoid.UnitSphere);
            _camera.Range = (camera.Eye - camera.Target).Magnitude;
            _camera.MinimumZoomRate = FromMeters(1);
            _camera.MaximumZoomRate = FromMeters(Double.MaxValue);
            _camera.ZoomFactor = 10;
            _camera.ZoomRateRangeAdjustment = 0;
            _camera.MinimumRotateRate = 1.0;
            _camera.MaximumRotateRate = 1.0;
            _camera.RotateRateRangeAdjustment = 0.0;
            _camera.RotateFactor = 0.0;
        }

        private void CreateAlgorithm()
        {
            double triangleLength = FromMeters(200000);
            double triangleDelta = FromMeters(0.5);

            Vector3D[] positions = new Vector3D[]
            {
                new Vector3D(_xTranslation, triangleDelta + 0, 0),                  // Red triangle
                new Vector3D(_xTranslation, triangleDelta + triangleLength, 0),
                new Vector3D(_xTranslation, triangleDelta + 0, triangleLength),
                new Vector3D(_xTranslation, -triangleDelta - 0, 0),                 // Green triangle
                new Vector3D(_xTranslation, -triangleDelta - 0, triangleLength),
                new Vector3D(_xTranslation, -triangleDelta - triangleLength, 0),
                new Vector3D(_xTranslation, 0, 0),                                  // Blue point
            };

            byte[] colors = new byte[]
            {
                255, 0, 0,
                255, 0, 0,
                255, 0, 0,
                0, 255, 0,
                0, 255, 0,
                0, 255, 0,
                0, 0, 255
            };

            if (_algorithm != null)
            {
                ((IDisposable)_algorithm).Dispose();
                _algorithm = null;
            }

            switch (_jitterAlgorithm)
            {
                case JitterAlgorithm.RelativeToWorld:
                    _algorithm = new RelativeToWorld(_window.Context, positions, colors);
                    break;
                case JitterAlgorithm.RelativeToCenter:
                    _algorithm = new RelativeToCenter(_window.Context, positions, colors);
                    break;
                case JitterAlgorithm.CPURelativeToEye:
                    _algorithm = new CPURelativeToEye(_window.Context, positions, colors);
                    break;
                case JitterAlgorithm.GPURelativeToEye:
                    _algorithm = new GPURelativeToEye(_window.Context, positions, colors);
                    break;
                case JitterAlgorithm.GPURelativeToEyeDSFUN90:
                    _algorithm = new GPURelativeToEyeDSFUN90(_window.Context, positions, colors);
                    break;
                case JitterAlgorithm.GPURelativeToEyeLOD:
                    _algorithm = new SceneGPURelativeToEyeLOD(_window.Context, positions, colors);
                    break;
            }
        }

        private void OnKeyUp(object sender, KeyboardKeyEventArgs e)
        {
            if (e.Key == KeyboardKey.S)
            {
                _scaleWorldCoordinates = !_scaleWorldCoordinates;

                CreateCamera();
                CreateAlgorithm();
            }
            else if ((e.Key == KeyboardKey.Left) || (e.Key == KeyboardKey.Right))
            {
                _jitterAlgorithm += (e.Key == KeyboardKey.Right) ? 1 : -1;

                if (_jitterAlgorithm < JitterAlgorithm.RelativeToWorld)
                {
                    _jitterAlgorithm = JitterAlgorithm.GPURelativeToEyeLOD;
                }
                else if (_jitterAlgorithm > JitterAlgorithm.GPURelativeToEyeLOD)
                {
                    _jitterAlgorithm = JitterAlgorithm.RelativeToWorld;
                }

                CreateAlgorithm();
            }
            else if ((e.Key == KeyboardKey.Down) || (e.Key == KeyboardKey.Up))
            {
                _camera.Range += (e.Key == KeyboardKey.Down) ? FromMeters(0.01) : FromMeters(-0.01);
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

            _algorithm.Render(context, _sceneState);
            _hud.Render(context, _sceneState);
        }

        #region IDisposable Members

        public void Dispose()
        {
            _hudFont.Dispose();
            _hud.Texture.Dispose();
            _hud.Dispose();
            _camera.Dispose();
            ((IDisposable)_algorithm).Dispose();
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
        private readonly ClearState _clearState;

        private readonly Font _hudFont;
        private readonly HeadsUpDisplay _hud;

        private double _xTranslation;
        private CameraLookAtPoint _camera;

        private bool _scaleWorldCoordinates;
        private IRenderable _algorithm;
        private JitterAlgorithm _jitterAlgorithm;
    }
}