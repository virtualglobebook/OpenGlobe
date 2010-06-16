#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

namespace OpenGlobe.Core.Geometry
{
    public class VertexAttributeFloat : VertexAttribute<float>
    {
        public VertexAttributeFloat(string name)
            : base(name, VertexAttributeType.Float)
        {
        }

        public VertexAttributeFloat(string name, int capacity)
            : base(name, VertexAttributeType.Float, capacity)
        {
        }
    }
}
