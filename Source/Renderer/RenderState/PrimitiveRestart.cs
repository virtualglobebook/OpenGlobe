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
    public class PrimitiveRestart
    {
        public PrimitiveRestart()
        {
            Enabled = false;
            Index = 0;
        }

        public bool Enabled { get; set; }
        public int Index{ get; set; }
    }
}
