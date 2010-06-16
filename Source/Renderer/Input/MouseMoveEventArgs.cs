#region License
//
// (C) Copyright 2010 Patrick Cozzi, Deron Ohlarik, and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Drawing;

namespace OpenGlobe.Renderer
{
    public class MouseMoveEventArgs : EventArgs
    {
        public MouseMoveEventArgs(Point point)
        {
            _point = point;
        }

        public Point Point
        {
            get { return _point; }
        }

        private Point _point;
    }
}
