#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System.Drawing;
using OpenGlobe.Core.Geometry;

namespace OpenGlobe.Renderer
{
    public enum CullFace
    {
        Front,
        Back,
        FrontAndBack
    }

    public class FacetCulling
    {
        public FacetCulling()
        {
            Enabled = true;
            Face = CullFace.Back;
            FrontFaceWindingOrder = WindingOrder.Counterclockwise;
        }

        public bool Enabled { get; set; }
        public CullFace Face { get; set; }
        public WindingOrder FrontFaceWindingOrder { get; set; }
    }
}
