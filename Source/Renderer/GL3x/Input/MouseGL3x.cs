#region License
//
// (C) Copyright 2010 Patrick Cozzi, Deron Ohlarik, and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using OpenTK.Input;

namespace OpenGlobe.Renderer
{
    public class MouseGL3x : Mouse
    {
        public MouseGL3x(MouseDevice mouse)
        {
            _mouse = mouse;

            _mouse.ButtonDown += OpenTKButtonDown;
            _mouse.ButtonUp += OpenTKButtonUp;
            _mouse.Move += OpenTKMove;
        }

        private void OpenTKButtonDown(object sender, OpenTK.Input.MouseButtonEventArgs e)
        {
            MouseButton button;
            if (e.Button == OpenTK.Input.MouseButton.Left)
            {
                button = MouseButton.Left;
            }
            else if (e.Button == OpenTK.Input.MouseButton.Middle)
            {
                button = MouseButton.Middle;
            }
            else if (e.Button == OpenTK.Input.MouseButton.Right)
            {
                button = MouseButton.Right;
            }
            else
            {
                return;
            }

            OnButtonDown(e.Position, button);
        }

        private void OpenTKButtonUp(object sender, OpenTK.Input.MouseButtonEventArgs e)
        {
            MouseButton button;
            if (e.Button == OpenTK.Input.MouseButton.Left)
            {
                button = MouseButton.Left;
            }
            else if (e.Button == OpenTK.Input.MouseButton.Middle)
            {
                button = MouseButton.Middle;
            }
            else if (e.Button == OpenTK.Input.MouseButton.Right)
            {
                button = MouseButton.Right;
            }
            else
            {
                return;
            }

            OnButtonUp(e.Position, button);
        }

        private void OpenTKMove(object sender, OpenTK.Input.MouseMoveEventArgs e)
        {
            OnMove(e.Position);
        }

        private MouseDevice _mouse;
    }
}
