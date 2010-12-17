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

namespace OpenGlobe.Renderer
{
    public class MouseButtonEventArgs : EventArgs
    {
        public MouseButtonEventArgs(Point point, MouseButton button, MouseButtonEvent buttonEvent)
        {
            _point = point;
            _button = button;
            _buttonEvent = buttonEvent;
        }

        public Point Point
        {
            get { return _point; }
        }

        public MouseButton Button
        {
            get { return _button; }
        }

        public MouseButtonEvent ButtonEvent
        {
            get { return _buttonEvent; }
        }

        private Point _point;
        private MouseButton _button;
        private MouseButtonEvent _buttonEvent;
    }
}
