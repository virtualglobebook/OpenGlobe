#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

namespace OpenGlobe.Renderer
{
    public class StencilTest
    {
        public StencilTest()
        {
            Enabled = false;
            FrontFace = new StencilTestFace();
            BackFace = new StencilTestFace();
        }

        public bool Enabled { get; set; }
        public StencilTestFace FrontFace { get; set; }
        public StencilTestFace BackFace { get; set; }
    }
}
