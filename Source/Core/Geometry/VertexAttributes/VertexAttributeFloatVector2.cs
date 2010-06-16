#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

namespace OpenGlobe.Core.Geometry
{
    public class VertexAttributeFloatVector2 : VertexAttribute<Vector2S>
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
