#region License
//
// (C) Copyright 2010 Patrick Cozzi, Deron Ohlarik, and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using MiniGlobe.Core;
using MiniGlobe.Renderer;
using OpenTK;

namespace MiniGlobe.Scene
{
    public class CameraFly : IDisposable
    {
        public CameraFly(Camera camera, MiniGlobeWindow window)
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

        public double Roll
        {
            get { return _roll; }
            set { _roll = value; }
        }

        public double Pitch
        {
            get { return _pitch; }
            set { _pitch = value; }
        }

        public double Yaw
        {
            get { return _yaw; }
            set { _yaw = value; }
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
            _roll -= seconds;
            while (_roll > Math.PI)
            {
                _roll -= Trig.TwoPI;
            }
            while (_roll < -Math.PI)
            {
                _roll += Trig.TwoPI;
            }
        }

        private void RollRight(double seconds)
        {
            _roll += seconds;
            while (_roll > Math.PI)
            {
                _roll -= Trig.TwoPI;
            }
            while (_roll < -Math.PI)
            {
                _roll += Trig.TwoPI;
            }
        }

        private void Rotate(Size movement)
        {
            CheckDisposed();

            double yawWindowRatio = (double)movement.Width / (double)_window.Width;
            double pitchWindowRatio = (double)movement.Height / (double)_window.Height;

            Quaterniond rotateX = Quaterniond.FromAxisAngle(Vector3d.UnitX, _roll);
            Vector3d yawPitch = Vector3d.Transform(new Vector3d(0.0, pitchWindowRatio, yawWindowRatio), rotateX);
            pitchWindowRatio = yawPitch.Y;
            yawWindowRatio = yawPitch.Z;

            _yaw -= yawWindowRatio * Math.PI;
            _pitch -= pitchWindowRatio * Math.PI;

            if (_pitch < -Trig.HalfPI)
            {
                _pitch = -Trig.HalfPI + (-Trig.HalfPI - _pitch);
                _yaw += Math.PI;
                _roll += Math.PI;
            }
            else if (_pitch > Trig.HalfPI)
            {
                _pitch = Trig.HalfPI - (_pitch - Trig.HalfPI);
                _yaw += Math.PI;
                _roll += Math.PI;
            }

            while (_yaw > Math.PI)
            {
                _yaw -= Trig.TwoPI;
            }
            while (_yaw < -Math.PI)
            {
                _yaw += Trig.TwoPI;
            }

            while (_roll > Math.PI)
            {
                _roll -= Trig.TwoPI;
            }
            while (_roll < -Math.PI)
            {
                _roll += Trig.TwoPI;
            }
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
            if (first)
            {
                first = false;
                _position = _camera.Eye;
                Vector3D look = _camera.Forward;
                _yaw = Math.Atan2(look.Y, look.X);
                _pitch = Math.Asin(look.Z / look.Magnitude);
            }
        }
        private bool first = true;

        private void UpdateCameraFromParameters()
        {
            _camera.Eye = _position;

            Quaterniond rotateX = Quaterniond.FromAxisAngle(Vector3d.UnitX, _roll);
            Quaterniond rotateY = Quaterniond.FromAxisAngle(Vector3d.UnitY, _pitch);
            Quaterniond rotateZ = Quaterniond.FromAxisAngle(Vector3d.UnitZ, _yaw);

            Vector3d look = Vector3d.Transform(Vector3d.UnitX, rotateX);
            look = Vector3d.Transform(look, rotateY);
            look = Vector3d.Transform(look, rotateZ);

            Vector3d up = Vector3d.Transform(Vector3d.UnitZ, rotateX);
            up = Vector3d.Transform(up, rotateY);
            up = Vector3d.Transform(up, rotateZ);

            look.Normalize();
            up.Normalize();

            _camera.Target = _camera.Eye + new Vector3D(look.X, look.Y, look.Z);
            _camera.Up = new Vector3D(up.X, up.Y, up.Z);
        }

        private Camera _camera;
        private MiniGlobeWindow _window;

        private bool _inputEnabled;
        private bool _leftButtonDown;
        private bool _rightButtonDown;
        private Dictionary<KeyboardKey, long> m_keyDownTime = new Dictionary<KeyboardKey, long>();
        private Point _lastPoint;

        private Vector3D _position;
        private double _roll;
        private double _pitch;
        private double _yaw;
        private double _movementRate = 300.0;
    }
}
