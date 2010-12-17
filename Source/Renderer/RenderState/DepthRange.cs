#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

namespace OpenGlobe.Renderer
{
    public class DepthRange
    {
        public DepthRange()
        {
            Near = 0.0;
            Far = 1.0;
        }

        public double Near { get; set; }
        public double Far { get; set; }
    }
}
