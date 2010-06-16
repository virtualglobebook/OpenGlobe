#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
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
