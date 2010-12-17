#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
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
