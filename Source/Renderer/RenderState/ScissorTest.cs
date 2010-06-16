#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System.Drawing;

namespace OpenGlobe.Renderer
{
    public class ScissorTest
    {
        public ScissorTest()
        {
            Enabled = false;
            Rectangle = new Rectangle(0, 0, 0, 0);
        }

        public bool Enabled { get; set; }
        public Rectangle Rectangle { get; set; }
    }
}
