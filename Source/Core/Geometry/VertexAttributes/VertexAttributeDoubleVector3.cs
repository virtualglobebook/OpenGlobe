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
    public class VertexAttributeDoubleVector3 : VertexAttribute<Vector3D>
    {
        public VertexAttributeDoubleVector3(string name)
            : base(name, VertexAttributeType.DoubleVector3)
        {
        }

        public VertexAttributeDoubleVector3(string name, int capacity)
            : base(name, VertexAttributeType.DoubleVector3, capacity)
        {
        }
    }
}
