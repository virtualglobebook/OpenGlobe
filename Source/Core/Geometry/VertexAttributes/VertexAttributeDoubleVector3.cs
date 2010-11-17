#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

namespace OpenGlobe.Core
{
    public class VertexAttributeDoubleVector3 : VertexAttribute<Vector3D>
    {
        public VertexAttributeDoubleVector3(string name)
            : base(name, VertexAttributeType.EmulatedDoubleVector3)
        {
        }

        public VertexAttributeDoubleVector3(string name, int capacity)
            : base(name, VertexAttributeType.EmulatedDoubleVector3, capacity)
        {
        }
    }
}
