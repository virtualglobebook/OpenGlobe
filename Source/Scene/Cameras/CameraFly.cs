#region License
//
// (C) Copyright 2010 Patrick Cozzi, Deron Ohlarik, and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using OpenGlobe.Core;
using OpenGlobe.Renderer;
using OpenTK;

namespace OpenGlobe.Scene
{
    public class CameraFly : IDisposable
    {
        public CameraFly(Camera camera, GraphicsWindow window)
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

            InputEnabled = true;
        }

        /// <summary>
        /// Disposes the camera.  After it is disposed, the camera should not be used.
        /// </summary>
        public void Dispose()
        {
            if (_window != null)
            {
                InputEnabled = false;
                _window = null;
            }
        }

        public Vector3D Position
        {
            get { return _position; }
            set { _position = value; }
        }

        public Vector3D Look
        {
            get { return new Vector3D(_look.X, _look.Y, _look.Z); }
            set { _look = new Vector3d(value.X, value.Y, value.Z); }
        }

        public Vector3D Up
        {
            get { return new Vector3D(_up.X, _up.Y, _up.Z); }
            set { _up = new Vector3d(value.X, value.Y, value.Z); }
        }

        public double MovementRate
        {
            get { return _movementRate; }
            set { _movementRate = value; }
        }

        /// <summary>
        /// Gets a value indicating if mouse and keyboard input is enabled or disabled.  If the value of this property
        /// is <see langword="true" />, the camera will respond to mouse and keyboard events.  If it is <see langword="false" />,
        /// mouse and keyboard events will be ignored.
        /// </summary>
        public bool InputEnabled
        {
            get { return _inputEnabled; }
            set
            {
                CheckDisposed();

                if (value != _inputEnabled)
                {
                    _inputEnabled = value;
                    if (_inputEnabled)
                    {
                        _window.Mouse.ButtonDown += MouseDown;
                        _window.Mouse.ButtonUp += MouseUp;
                        _window.Mouse.Move += MouseMove;
                        _window.Keyboard.KeyDown += KeyDown;
                        _window.Keyboard.KeyUp += KeyUp;
                        _window.PreRenderFrame += PreRenderFrame;
                    }
                    else
                    {
                        _window.Mouse.ButtonDown -= MouseDown;
                        _window.Mouse.ButtonUp -= MouseUp;
                        _window.Mouse.Move -= MouseMove;
                        _window.Keyboard.KeyDown -= KeyDown;
                        _window.Keyboard.KeyUp -= KeyUp;
                        _window.PreRenderFrame -= PreRenderFrame;
                    }
                }
            }
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

        private void KeyDown(object sender, KeyboardKeyEventArgs e)
        {
            m_keyDownTime.Add(e.Key, Stopwatch.GetTimestamp());
        }

        private void KeyUp(object sender, KeyboardKeyEventArgs e)
        {
            m_keyDownTime.Remove(e.Key);
        }

        private void PreRenderFrame()
        {
            UpdateParametersFromCamera();
            HandleKey(KeyboardKey.Up, MoveForward);
            HandleKey(KeyboardKey.Down, MoveBackward);
            HandleKey(KeyboardKey.Left, MoveLeft);
            HandleKey(KeyboardKey.Right, MoveRight);
            HandleKey(KeyboardKey.Delete, RollLeft);
            HandleKey(KeyboardKey.PageDown, RollRight);
            UpdateCameraFromParameters();
        }

        private void HandleKey(KeyboardKey key, Action<double> action)
        {
            long startTime;
            if (m_keyDownTime.TryGetValue(key, out startTime))
            {
                long now = Stopwatch.GetTimestamp();
                long moveTime = now - startTime;
                action((double)moveTime / Stopwatch.Frequency);
                m_keyDownTime[key] = now;
            }
        }

        private void MoveForward(double seconds)
        {
            double distance = MovementRate * seconds;
            _position += _camera.Forward * distance;
        }

        private void MoveBackward(double seconds)
        {
            double distance = MovementRate * seconds;
            _position -= _camera.Forward * distance;
        }

        private void MoveLeft(double seconds)
        {
            Vector3D right = _camera.Forward.Cross(_camera.Up);
            double distance = MovementRate * seconds;
            _position -= right * distance;
        }

        private void MoveRight(double seconds)
        {
            Vector3D right = _camera.Forward.Cross(_camera.Up);
            double distance = MovementRate * seconds;
            _position += right * distance;
        }

        private void RollLeft(double seconds)
        {
            Quaterniond rotation = Quaterniond.FromAxisAngle(_look, -seconds);

            _up = Vector3d.Transform(_up, rotation);
            _right = Vector3d.Cross(_look, _up);

            _up.Normalize();
            _right.Normalize();
        }

        private void RollRight(double seconds)
        {
            Quaterniond rotation = Quaterniond.FromAxisAngle(_look, seconds);

            _up = Vector3d.Transform(_up, rotation);
            _right = Vector3d.Cross(_look, _up);

            _up.Normalize();
            _right.Normalize();
        }

        private void Rotate(Size movement)
        {
            CheckDisposed();

            double horizontalWindowRatio = (double)movement.Width / (double)_window.Width;
            double verticalWindowRatio = (double)movement.Height / (double)_window.Height;

            // Horizontal movement is rotation around the Up-axis
            // Vertical movement is rotation around the Right-axis
            Quaterniond horizontalRotation = Quaterniond.FromAxisAngle(_up, -horizontalWindowRatio);
            Quaterniond verticalRotation = Quaterniond.FromAxisAngle(_right, verticalWindowRatio);

            _look = Vector3d.Transform(_look, horizontalRotation);
            _look = Vector3d.Transform(_look, verticalRotation);
            _up = Vector3d.Transform(_up, horizontalRotation);
            _up = Vector3d.Transform(_up, verticalRotation);
            _right = Vector3d.Cross(_look, _up);

            _look.Normalize();
            _up.Normalize();
            _right.Normalize();
        }

        private void CheckDisposed()
        {
            if (_window == null)
            {
                throw new ObjectDisposedException(typeof(CameraLookAtPoint).Name);
            }
        }

        /// <summary>
        /// Updates <see cref="Azimuth"/>, <see cref="Elevation"/>, and <see cref="Range"/>
        /// properties based on the current position of the renderer <see cref="Camera"/>.
        /// </summary>
        public void UpdateParametersFromCamera()
        {
            _position = _camera.Eye;
            _look = new Vector3d(_camera.Forward.X, _camera.Forward.Y, _camera.Forward.Z);
            _up = new Vector3d(_camera.Up.X, _camera.Up.Y, _camera.Up.Z);
            _right = new Vector3d(_camera.Right.X, _camera.Right.Y, _camera.Right.Z);
        }

        private void UpdateCameraFromParameters()
        {
            _camera.Eye = _position;
            _camera.Target = _position + Look;
            _camera.Up = Up;
        }

        private Camera _camera;
        private GraphicsWindow _window;

        private bool _inputEnabled;
        private bool _leftButtonDown;
        private bool _rightButtonDown;
        private Dictionary<KeyboardKey, long> m_keyDownTime = new Dictionary<KeyboardKey, long>();
        private Point _lastPoint;

        private Vector3D _position;
        private Vector3d _look;
        private Vector3d _up;
        private Vector3d _right;
        private double _movementRate = 300.0;
    }
}
