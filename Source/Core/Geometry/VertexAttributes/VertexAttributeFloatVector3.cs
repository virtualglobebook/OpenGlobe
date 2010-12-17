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
    public class VertexAttributeFloatVector3 : VertexAttribute<Vector3F>
    {
        public VertexAttributeFloatVector3(string name)
            : base(name, VertexAttributeType.FloatVector3)
        {
        }

        public VertexAttributeFloatVector3(string name, int capacity)
            : base(name, VertexAttributeType.FloatVector3, capacity)
        {
        }
    }
}
