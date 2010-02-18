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
using MiniGlobe.Core;
using MiniGlobe.Core.Geometry;

namespace MiniGlobe.Scene
{
    /// <summary>
    /// A camera that always faces a particular point.  The location of the camera, from which
    /// it views the point, is specified by an <see cref="Azimuth"/>, <see cref="Elevation"/>,
    /// and <see cref="Range"/>.  <see cref="FixedToLocalRotation"/> defines the transformation
    /// from the <see cref="Camera">Camera's</see> axes to a local set of axes in which the azimuth,
    /// elevation, and range are defined.
    /// </summary>
    public sealed class CameraLookAtPoint : IDisposable
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="camera">The renderer camera that is to be manipulated by the new instance.</param>
        /// <param name="window">The window in which the scene is drawn.</param>
        /// <param name="ellipsoid">The ellipsoid defining the shape of the globe.</param>
        public CameraLookAtPoint(Camera camera, MiniGlobeWindow window, Ellipsoid ellipsoid)
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

        /// <summary>
        /// Disposes the camera.  After it is disposed, the camera will not longer respond to
        /// input events.
        /// </summary>
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

        /// <summary>
        /// Gets the renderer camera that is manipulated by this instance.
        /// </summary>
        public Camera Camera
        {
            get { return _camera; }
        }
        
        /// <summary>
        /// Gets the window in which the scene is drawn.
        /// </summary>
        public MiniGlobeWindow Window
        {
            get { return _window; }
        }

        /// <summary>
        /// Gets the ellipsoid defining the shape of the globe.
        /// </summary>
        public Ellipsoid Ellipsoid
        {
            get { return _ellipsoid; }
        }

        /// <summary>
        /// Gets or sets the azimuth angle defining the position of the camera, in radians.  Azimuth is defined
        /// as the angle between the positive X-axis and the projection of the camera position into the
        /// X-Y plane.  Azimuth is positive toward the Y-axis.
        /// </summary>
        public double Azimuth
        {
            get { return _azimuth; }
            set { _azimuth = value; UpdateCameraFromParameters(); }
        }

        /// <summary>
        /// Gets or sets the elevation angle defining the position of the camera, in radians.  Elevation is
        /// defined as the angle of the camera out of the X-Y plane.  A positive elevation angle means a
        /// positive Z coordinate, and a negative elevation angle means a negative Z coordinate.
        /// </summary>
        public double Elevation
        {
            get { return _elevation; }
            set { _elevation = value; UpdateCameraFromParameters(); }
        }

        /// <summary>
        /// Gets or sets the camera's distance from the <see cref="CenterPoint"/>, in meters.
        /// </summary>
        public double Range
        {
            get { return _range; }
            set { _range = value; UpdateCameraFromParameters(); }
        }

        /// <summary>
        /// Gets or sets the center point of the camera.  The camera always faces this point even as
        /// <see cref="Azimuth"/>, <see cref="Elevation"/>, and <see cref="Range"/> change.
        /// </summary>
        public Vector3d CenterPoint
        {
            get { return _centerPoint; }
            set { _centerPoint = value; UpdateCameraFromParameters(); }
        }

        /// <summary>
        /// Gets or sets the transformation from the <see cref="Ellipsoid"/> fixed axes to
        /// the local axes in which the <see cref="Azimuth"/> and <see cref="Elevation"/> are defined.
        /// </summary>
        public Matrix3d FixedToLocalRotation
        {
            get { return _fixedToLocalRotation; }
            set { _fixedToLocalRotation = value; UpdateCameraFromParameters(); }
        }

        /// <summary>
        /// Sets the <see cref="CenterPoint"/> and <see cref="FixedToLocalRotation"/> properties so that the
        /// camera is looking at a given longitude, latitude, and height and is oriented in that point's local
        /// East-North-Up frame.  This method does not change the <see cref="Azimuth"/>, <see cref="Elevation"/>,
        /// or <see cref="Range"/> properties, but the existing values of those properties are interpreted in the
        /// new reference frame.
        /// </summary>
        /// <param name="longitude">The longitude of the point to look at, in radians.</param>
        /// <param name="latitude">The latitude of the point to look at, in radians.</param>
        /// <param name="height">The height of the point to look at, in meters above the <see cref="Ellipsoid"/> surface.</param>
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

            UpdateCameraFromParameters();
        }

        /// <summary>
        /// Updates <see cref="Azimuth"/>, <see cref="Elevation"/>, and <see cref="Range"/>
        /// properties based on the current position of the renderer <see cref="Camera"/>.
        /// </summary>
        public void UpdateParametersFromCamera()
        {
            Vector3d eyePosition = _fixedToLocalRotation * (_camera.Eye - _camera.Target);
            Vector3d up = _fixedToLocalRotation * _camera.Up;

            _range = Math.Sqrt(eyePosition.X * eyePosition.X + eyePosition.Y * eyePosition.Y + eyePosition.Z * eyePosition.Z);
            _elevation = Math.Asin(eyePosition.Z / _range);

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

        private void UpdateCameraFromParameters()
        {
            _camera.Target = _centerPoint;

            double rangeTimesSinElevation = _range * Math.Cos(_elevation);
            _camera.Eye = new Vector3d(rangeTimesSinElevation * Math.Cos(_azimuth),
                                       rangeTimesSinElevation * Math.Sin(_azimuth),
                                       _range * Math.Sin(_elevation));

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

        /// <summary>
        /// Simulates a press of a mouse button at a particular point in client window coordinates.
        /// </summary>
        /// <param name="button">The mouse button that is pressed.</param>
        /// <param name="point">The point at which the mouse button was pressed, in client window coordinates.</param>
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

        /// <summary>
        /// Simulates a release of a mouse button at a particular point in client window coordinates.
        /// </summary>
        /// <param name="button">The mouse button that was released.</param>
        /// <param name="point">The point at which the mouse button was released, in client window coordinates.</param>
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

        /// <summary>
        /// Simulates a mouse move to a particular point in client window coordinates.
        /// </summary>
        /// <param name="point">The point to which the mouse moved, in client window coordinates.</param>
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

            _azimuth -= azimuthWindowRatio * Trig.TwoPI;
            _elevation += elevationWindowRatio * Math.PI;

            while (_azimuth > Math.PI)
            {
                _azimuth -= Trig.TwoPI;
            }
            while (_azimuth < -Math.PI)
            {
                _azimuth += Trig.TwoPI;
            }

            if (_elevation < -Trig.HalfPI)
            {
                _elevation = -Trig.HalfPI;
            }
            else if (_elevation > Trig.HalfPI)
            {
                _elevation = Trig.HalfPI;
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