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
    public class VertexAttributeDoubleVector4 : VertexAttribute<Vector4D>
    {
        public VertexAttributeDoubleVector4(string name)
            : base(name, VertexAttributeType.DoubleVector4)
        {
        }

        public VertexAttributeDoubleVector4(string name, int capacity)
            : base(name, VertexAttributeType.DoubleVector4, capacity)
        {
        }
    }
}
