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
    public class VertexAttributeFloatVector4 : VertexAttribute<Vector4F>
    {
        public VertexAttributeFloatVector4(string name)
            : base(name, VertexAttributeType.FloatVector4)
        {
        }

        public VertexAttributeFloatVector4(string name, int capacity)
            : base(name, VertexAttributeType.FloatVector4, capacity)
        {
        }
    }
}
