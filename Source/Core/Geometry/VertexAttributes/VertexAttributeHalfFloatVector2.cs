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
    public class VertexAttributeHalfFloatVector2 : VertexAttribute<Vector2H>
    {
        public VertexAttributeHalfFloatVector2(string name)
            : base(name, VertexAttributeType.HalfFloatVector2)
        {
        }

        public VertexAttributeHalfFloatVector2(string name, int capacity)
            : base(name, VertexAttributeType.HalfFloatVector2, capacity)
        {
        }
    }
}
