#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

namespace OpenGlobe.Core
{
    public class VertexAttributeHalfFloatVector3 : VertexAttribute<Vector3H>
    {
        public VertexAttributeHalfFloatVector3(string name)
            : base(name, VertexAttributeType.HalfFloatVector3)
        {
        }

        public VertexAttributeHalfFloatVector3(string name, int capacity)
            : base(name, VertexAttributeType.HalfFloatVector3, capacity)
        {
        }
    }
}
