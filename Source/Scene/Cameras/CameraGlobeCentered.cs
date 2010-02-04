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

            _range = ellipsoid.MaximumRadius * 2.0;

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

        public double Azimuth
        {
            get { return _azimuth; }
            set { _azimuth = value; }
        }

        public double Elevation
        {
            get { return _elevation; }
            set { _elevation = value; }
        }

        public double Range
        {
            get { return _range; }
            set { _range = value; }
        }

        public Vector3d CenterPoint
        {
            get { return _centerPoint; }
            set { _centerPoint = value; }
        }

        public Matrix3d FixedToLocalRotation
        {
            get { return _fixedToLocalRotation; }
            set { _fixedToLocalRotation = value; }
        }

        public void ViewPoint(double longitude, double latitude, double height)
        {
            _centerPoint = _ellipsoid.DeticToVector3d(longitude, latitude, height);
            
            // Fixed to East-North-Up rotation, from Wikipedia's "Geodetic System" topic.
            double cosLon = Math.Cos(longitude);
            double cosLat = Math.Cos(latitude);
            double sinLon = Math.Sin(longitude);
            double sinLat = Math.Sin(latitude);
            _fixedToLocalRotation =
                new Matrix3d(-sinLon,            cosLon,             0.0,
                             -sinLat * cosLon,   -sinLat * sinLon,   cosLat,
                             cosLat * cosLon,    cosLat * sinLon,    sinLat);
        }

        public void UpdateParametersFromCamera()
        {
            Vector3d eyePosition = _fixedToLocalRotation * (_camera.Eye - _camera.Target);
            Vector3d up = _fixedToLocalRotation * _camera.Up;

            _range = Math.Sqrt(eyePosition.X * eyePosition.X + eyePosition.Y * eyePosition.Y + eyePosition.Z * eyePosition.Z);
            _elevation = Math.Acos(eyePosition.Z / _range);

            if (eyePosition.Xy.LengthSquared < up.Xy.LengthSquared)
            {
                // Near the poles, determine the azimuth from the Up direction instead of from the Eye position.
                if (eyePosition.Z > 0.0)
                {
                    _azimuth = Math.Atan2(-up.Y, -up.X);
                }
                else
                {
                    _azimuth = Math.Atan2(up.Y, up.X);
                }
            }
            else
            {
                _azimuth = Math.Atan2(eyePosition.Y, eyePosition.X);
            }
        }

        public void UpdateCameraFromParameters()
        {
            _camera.Target = _centerPoint;

            double rangeTimesSinElevation = _range * Math.Sin(_elevation);
            _camera.Eye = new Vector3d(rangeTimesSinElevation * Math.Cos(_azimuth),
                                       rangeTimesSinElevation * Math.Sin(_azimuth),
                                       _range * Math.Cos(_elevation));

            Vector3d right = Vector3d.Cross(_camera.Eye, Vector3d.UnitZ);
            _camera.Up = Vector3d.Normalize(Vector3d.Cross(right, _camera.Eye));

            if (Double.IsNaN(_camera.Up.X))
            {
                // Up vector is invalid because _camera.Eye is all Z (or very close to it).
                // So compute the Up vector directly assuming no Z component.
                _camera.Up = new Vector3d(-Math.Cos(_azimuth), -Math.Sin(_azimuth), 0.0);
            }

            Matrix3d localToFixed = _fixedToLocalRotation.Transpose();
            _camera.Eye = localToFixed * _camera.Eye;
            _camera.Eye += _centerPoint;
            _camera.Up = localToFixed * _camera.Up;
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

            UpdateParametersFromCamera();

            Size movement = new Size(point.X - _lastPoint.X, point.Y - _lastPoint.Y);

            if (_leftButtonDown)
            {
                Rotate(movement);
            }

            if (_rightButtonDown)
            {
                Zoom(movement);
            }

            UpdateCameraFromParameters();

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
            double approximateDistanceFromSurface = Math.Abs(_camera.Eye.Length - _ellipsoid.MinimumRadius);
            approximateDistanceFromSurface = Math.Max(approximateDistanceFromSurface, _ellipsoid.MinimumRadius / 100.0);
            double rangeWindowRatio = (double)movement.Height / (double)_window.Height;
            _range -= 5.0 * approximateDistanceFromSurface * rangeWindowRatio;
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

        private Vector3d _centerPoint = Vector3d.Zero;
        private Matrix3d _fixedToLocalRotation = Matrix3d.Identity;
    }
}