#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
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
