#region License
//
// (C) Copyright 2010 Patrick Cozzi, Deron Ohlarik, and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using MiniGlobe.Renderer;
using System.Drawing;
using System;
using OpenTK;
using MiniGlobe.Core.Geometry;

namespace MiniGlobe.Scene
{
    public sealed class CameraGlobeCentered : IDisposable
    {
        public CameraGlobeCentered(Camera camera, MiniGlobeWindow window, Ellipsoid ellipsoid)
        {
            if (camera == null)
            {
                throw new ArgumentNullException("camera");
            }
            if (window == null)
            {
                throw new ArgumentNullException("window");
            }

            _camera = camera;
            _window = window;
            _ellipsoid = ellipsoid;

            _window.Mouse.ButtonDown += MouseDown;
            _window.Mouse.ButtonUp += MouseUp;
            _window.Mouse.Move += MouseMove;
        }

        public void Dispose()
        {
            if (_window != null)
            {
                _window.Mouse.ButtonDown -= MouseDown;
                _window.Mouse.ButtonUp -= MouseUp;
                _window.Mouse.Move -= MouseMove;
                _window = null;
            }
        }

        public Camera Camera
        {
            get { return _camera; }
        }

        public MiniGlobeWindow Window
        {
            get { return _window; }
        }

        public Ellipsoid Ellipsoid
        {
            get { return _ellipsoid; }
        }

        public void MouseDown(MouseButton button, Point point)
        {
            if (button == MouseButton.Left)
            {
                _leftButtonDown = true;
            }
            else if (button == MouseButton.Right)
            {
                _rightButtonDown = true;
            }

            _lastPoint = point;
        }

        public void MouseUp(MouseButton button, Point point)
        {
            if (button == MouseButton.Left)
            {
                _leftButtonDown = false;
            }
            else if (button == MouseButton.Right)
            {
                _rightButtonDown = false;
            }
        }

        public void MouseMove(Point point)
        {
            if (!_leftButtonDown && !_rightButtonDown)
            {
                return;
            }

            if (_window == null)
            {
                throw new ObjectDisposedException("CameraGlobeCentered");
            }

            CameraToElements();

            Size movement = new Size(point.X - _lastPoint.X, point.Y - _lastPoint.Y);

            if (_leftButtonDown)
            {
                Rotate(movement);
            }

            if (_rightButtonDown)
            {
                Zoom(movement);
            }

            ElementsToCamera();

            _lastPoint = point;
        }

        private void MouseDown(object sender, MouseButtonEventArgs e)
        {
            MouseDown(e.Button, e.Point);
        }

        private void MouseUp(object sender, MouseButtonEventArgs e)
        {
            MouseUp(e.Button, e.Point);
        }

        private void MouseMove(object sender, MouseMoveEventArgs e)
        {
            MouseMove(e.Point);
        }

        private void Rotate(Size movement)
        {
            double azimuthWindowRatio = (double)movement.Width / (double)_window.Width;
            double elevationWindowRatio = (double)movement.Height / (double)_window.Height;

            _azimuth -= azimuthWindowRatio * 2.0 * Math.PI;
            _elevation -= elevationWindowRatio * Math.PI;

            while (_azimuth > Math.PI)
            {
                _azimuth -= 2.0 * Math.PI;
            }
            while (_azimuth < -Math.PI)
            {
                _azimuth += 2.0 * Math.PI;
            }

            if (_elevation < 0.0)
            {
                _elevation = 0.0;
            }
            else if (_elevation > Math.PI)
            {
                _elevation = Math.PI;
            }
        }

        private void Zoom(Size movement)
        {
            double approximateDistanceFromSurface = _camera.Eye.Length - _ellipsoid.MinimumRadius;
            double rangeWindowRatio = (double)movement.Height / (double)_window.Height;
            _range += 5.0 * approximateDistanceFromSurface * rangeWindowRatio;
        }

        private void CameraToElements()
        {
            Vector3d eyePosition = _camera.Eye;

            _range = eyePosition.Length;
            _elevation = Math.Acos(eyePosition.Z / _range);

            if (eyePosition.Xy.LengthSquared == 0.0)
            {
                _azimuth = Math.Atan2(-_camera.Up.Y, -_camera.Up.X);
            }
            else
            {
                _azimuth = Math.Atan2(eyePosition.Y, eyePosition.X);
            }
        }

        private void ElementsToCamera()
        {
            double rangeTimesSinElevation = _range * Math.Sin(_elevation);
            _camera.Eye = new Vector3d(rangeTimesSinElevation * Math.Cos(_azimuth),
                                       rangeTimesSinElevation * Math.Sin(_azimuth),
                                       _range * Math.Cos(_elevation));

            Vector3d up;
            if (_camera.Eye.Xy.LengthSquared == 0.0)
            {
                // Near a pole, so capture the azimuth directly in the up direction.
                up = new Vector3d(-Math.Cos(_azimuth), -Math.Sin(_azimuth), 0.0);
            }
            else
            {
                Vector3d look = _camera.Eye - _camera.Target;
                Vector3d right = Vector3d.Cross(look, Vector3d.UnitZ);
                up = Vector3d.Cross(right, look);
                up.Normalize();
            }
            
            _camera.Up = up;
        }

        private Camera _camera;
        private MiniGlobeWindow _window;
        private Ellipsoid _ellipsoid;

        private bool _leftButtonDown;
        private bool _rightButtonDown;
        private Point _lastPoint;

        private double _azimuth;
        private double _elevation;
        private double _range;
    }
}