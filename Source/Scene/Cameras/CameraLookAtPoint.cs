#region License
//
// (C) Copyright 2010 Patrick Cozzi, Deron Ohlarik, and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;
using System.Drawing;
using OpenGlobe.Core;
using OpenGlobe.Renderer;

namespace OpenGlobe.Scene
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
        public CameraLookAtPoint(Camera camera, GraphicsWindow window, Ellipsoid ellipsoid)
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

            _centerPoint = camera.Target;

            _zoomFactor = 5.0;
            _zoomRateRangeAdjustment = ellipsoid.MaximumRadius;
            _maximumZoomRate = Double.MaxValue;
            _minimumZoomRate = ellipsoid.MaximumRadius / 100.0;

            _rotateFactor = 1.0 / ellipsoid.MaximumRadius;
            _rotateRateRangeAdjustment = ellipsoid.MaximumRadius;
            _maximumRotateRate = 1.0;
            _minimumRotateRate = 1.0 / 5000.0;

            // TODO: Should really be:
            // _range = (camera.Eye - camera.Target).Magnitude;
            _range = ellipsoid.MaximumRadius * 2.0;

            MouseEnabled = true;
        }
        
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="camera">The renderer camera that is to be manipulated by the new instance.</param>
        /// <param name="window">The window in which the scene is drawn.</param>
        /// <param name="ellipsoid">The ellipsoid defining the shape of the globe.</param>
        public CameraLookAtPoint(Camera camera, GraphicsWindow window) :
            this(camera, window, Ellipsoid.UnitSphere)
        {
        }

        /// <summary>
        /// Disposes the camera.  After it is disposed, the camera should not be used.
        /// </summary>
        public void Dispose()
        {
            if (_window != null)
            {
                MouseEnabled = false;
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
        public GraphicsWindow Window
        {
            get { return _window; }
        }

        /// <summary>
        /// Gets or sets the factor used to compute the rate at which the camera zooms in response to mouse events.
        /// The zoom rate is the distance by which the <see cref="Range"/> is adjusted when the mouse moves a distance equal to the
        /// <see cref="GraphicsWindow.Height"/> of the <see cref="Window"/>.  It is computed as follows:
        /// <code>ZoomRate = <see cref="ZoomFactor"/> * (<see cref="Range"/> - <see cref="ZoomRateRangeAdjustment"/>)</code>.
        /// When the mouse is moved across a fraction of the window, the range is adjusted by the corresponding fraction of the
        /// zoom rate.
        /// </summary>
        public double ZoomFactor
        {
            get { return _zoomFactor; }
            set { _zoomFactor = value; }
        }

        /// <summary>
        /// Gets or sets the distance that is subtracted from the <see cref="Range"/> when computing the rate at which the camera zooms in
        /// response to mouse events.  The zoom rate is the distance by which the <see cref="Range"/> is adjusted when the mouse moves a
        /// distance equal to the <see cref="GraphicsWindow.Height"/> of the <see cref="Window"/>.  It is computed as follows:
        /// <code>ZoomRate = <see cref="ZoomFactor"/> * (<see cref="Range"/> - <see cref="ZoomRateRangeAdjustment"/>)</code>.
        /// When the mouse is moved a distance equal to a fraction of the height of the window, the range is adjusted by the
        /// corresponding fraction of the zoom rate.  The <see cref="ZoomRateRangeAdjustment"/> is useful, for example, when the camera
        /// is centered on the center of a globe. This property can be set to the radius of the globe so that the zoom rate corresponds to the
        /// distance from the surface of the globe rather than the distance to the center of the globe.
        /// </summary>
        public double ZoomRateRangeAdjustment
        {
            get { return _zoomRateRangeAdjustment; }
            set { _zoomRateRangeAdjustment = value; }
        }

        public double RotateRateRangeAdjustment
        {
            get { return _rotateRateRangeAdjustment; }
            set { _rotateRateRangeAdjustment = value; }
        }

        public double RotateFactor
        {
            get { return _rotateFactor; }
            set { _rotateFactor = value; }
        }

        public double MinimumRotateRate
        {
            get { return _minimumRotateRate; }
            set { _minimumRotateRate = value; }
        }

        public double MaximumRotateRate
        {
            get { return _maximumRotateRate; }
            set { _maximumRotateRate = value; }
        }

        /// <summary>
        /// Gets or sets the maximum rate at which the camera zooms in response to mouse events, regardless of the current
        /// <see cref="Range"/>.  See <see cref="ZoomFactor"/> and <see cref="ZoomRateRangeAdjustment"/> for an explanation
        /// of how the zoom rate is computed.
        /// </summary>
        public double MaximumZoomRate
        {
            get { return _maximumZoomRate; }
            set { _maximumZoomRate = value; }
        }

        /// <summary>
        /// Gets or sets the minimum rate at which the camera zooms in response to mouse events, regardless of the current
        /// <see cref="Range"/>.  See <see cref="ZoomFactor"/> and <see cref="ZoomRateRangeAdjustment"/> for an explanation
        /// of how the zoom rate is computed.
        /// </summary>
        public double MinimumZoomRate
        {
            get { return _minimumZoomRate; }
            set { _minimumZoomRate = value; }
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
        public Vector3D CenterPoint
        {
            get { return _centerPoint; }
            set { _centerPoint = value; UpdateCameraFromParameters(); }
        }

        /// <summary>
        /// Gets or sets the transformation from the <see cref="Ellipsoid"/> fixed axes to
        /// the local axes in which the <see cref="Azimuth"/> and <see cref="Elevation"/> are defined.
        /// </summary>
        public Matrix3D FixedToLocalRotation
        {
            get { return _fixedToLocalRotation; }
            set { _fixedToLocalRotation = value; UpdateCameraFromParameters(); }
        }

        /// <summary>
        /// Gets a value indicating if the mouse is enabled or disabled.  If the value of this property
        /// is <see langword="true" />, the camera will respond to mouse events.  If it is <see langword="false" />,
        /// mouse events will be ignored.
        /// </summary>
        public bool MouseEnabled
        {
            get { return _mouseEnabled; }
            set
            {
                CheckDisposed();

                if (value != _mouseEnabled)
                {
                    _mouseEnabled = value;
                    if (_mouseEnabled)
                    {
                        _window.Mouse.ButtonDown += MouseDown;
                        _window.Mouse.ButtonUp += MouseUp;
                        _window.Mouse.Move += MouseMove;
                    }
                    else
                    {
                        _window.Mouse.ButtonDown -= MouseDown;
                        _window.Mouse.ButtonUp -= MouseUp;
                        _window.Mouse.Move -= MouseMove;
                    }
                }
            }
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
        public void ViewPoint(Ellipsoid ellipsoid, Geodetic3D geographic)
        {
            _centerPoint = ellipsoid.ToVector3D(geographic);
            
            // Fixed to East-North-Up rotation, from Wikipedia's "Geodetic System" topic.
            double cosLon = Math.Cos(geographic.Longitude);
            double cosLat = Math.Cos(geographic.Latitude);
            double sinLon = Math.Sin(geographic.Longitude);
            double sinLat = Math.Sin(geographic.Latitude);
            _fixedToLocalRotation =
                new Matrix3D(-sinLon,            cosLon,             0.0,
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
            Vector3D eyePosition = _fixedToLocalRotation * (_camera.Eye - _camera.Target);
            Vector3D up = _fixedToLocalRotation * _camera.Up;

            _range = Math.Sqrt(eyePosition.X * eyePosition.X + eyePosition.Y * eyePosition.Y + eyePosition.Z * eyePosition.Z);
            _elevation = Math.Asin(eyePosition.Z / _range);

            if (eyePosition.XY.MagnitudeSquared < up.XY.MagnitudeSquared)
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
            _camera.Eye = new Vector3D(rangeTimesSinElevation * Math.Cos(_azimuth),
                                       rangeTimesSinElevation * Math.Sin(_azimuth),
                                       _range * Math.Sin(_elevation));

            Vector3D right = _camera.Eye.Cross(Vector3D.UnitZ);
            _camera.Up = right.Cross(_camera.Eye).Normalize();

            if (_camera.Up.IsUndefined)
            {
                // Up vector is invalid because _camera.Eye is all Z (or very close to it).
                // So compute the Up vector directly assuming no Z component.
                _camera.Up = new Vector3D(-Math.Cos(_azimuth), -Math.Sin(_azimuth), 0.0);
            }

            Matrix3D localToFixed = _fixedToLocalRotation.Transpose();
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
            CheckDisposed();

            double rotateRate = _rotateFactor * (_range - _rotateRateRangeAdjustment);
            if (rotateRate > _maximumRotateRate)
            {
                rotateRate = _maximumRotateRate;
            }
            if (rotateRate < _minimumRotateRate)
            {
                rotateRate = _minimumRotateRate;
            }

            double azimuthWindowRatio = (double)movement.Width / (double)_window.Width;
            double elevationWindowRatio = (double)movement.Height / (double)_window.Height;

            _azimuth -= rotateRate * azimuthWindowRatio * Trig.TwoPi;
            _elevation += rotateRate * elevationWindowRatio * Math.PI;

            while (_azimuth > Math.PI)
            {
                _azimuth -= Trig.TwoPi;
            }
            while (_azimuth < -Math.PI)
            {
                _azimuth += Trig.TwoPi;
            }

            if (_elevation < -Trig.PiOverTwo)
            {
                _elevation = -Trig.PiOverTwo;
            }
            else if (_elevation > Trig.PiOverTwo)
            {
                _elevation = Trig.PiOverTwo;
            }
        }

        private void Zoom(Size movement)
        {
            CheckDisposed();

            double zoomRate = _zoomFactor * (_range - _zoomRateRangeAdjustment);
            if (zoomRate > _maximumZoomRate)
            {
                zoomRate = _maximumZoomRate;
            }
            if (zoomRate < _minimumZoomRate)
            {
                zoomRate = _minimumZoomRate;
            }

            double rangeWindowRatio = (double)movement.Height / (double)_window.Height;
            _range -= zoomRate * rangeWindowRatio;
        }

        private void CheckDisposed()
        {
            if (_window == null)
            {
                throw new ObjectDisposedException(typeof(CameraLookAtPoint).Name);
            }
        }

        private Camera _camera;
        private GraphicsWindow _window;

        private double _zoomFactor;
        private double _zoomRateRangeAdjustment;
        private double _maximumZoomRate;
        private double _minimumZoomRate;

        private double _rotateFactor;
        private double _rotateRateRangeAdjustment;
        private double _minimumRotateRate;
        private double _maximumRotateRate;

        private bool _leftButtonDown;
        private bool _rightButtonDown;
        private Point _lastPoint;

        private double _azimuth;
        private double _elevation;
        private double _range;

        private bool _mouseEnabled;

        private Vector3D _centerPoint = Vector3D.Zero;
        private Matrix3D _fixedToLocalRotation = Matrix3D.Identity;
    }
}