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
