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
    public class VertexAttributeFloatVector2 : VertexAttribute<Vector2F>
    {
        public VertexAttributeFloatVector2(string name)
            : base(name, VertexAttributeType.FloatVector2)
        {
        }

        public VertexAttributeFloatVector2(string name, int capacity)
            : base(name, VertexAttributeType.FloatVector2, capacity)
        {
        }
    }
}
