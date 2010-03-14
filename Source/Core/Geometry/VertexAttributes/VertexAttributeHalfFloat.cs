#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

namespace MiniGlobe.Core.Geometry
{
    public class VertexAttributeHalfFloat : VertexAttribute<Half>
    {
        public VertexAttributeHalfFloat(string name)
            : base(name, VertexAttributeType.HalfFloat)
        {
        }

        public VertexAttributeHalfFloat(string name, int capacity)
            : base(name, VertexAttributeType.HalfFloat, capacity)
        {
        }
    }
}
