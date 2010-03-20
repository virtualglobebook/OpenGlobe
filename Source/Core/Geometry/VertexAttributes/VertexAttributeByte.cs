#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

namespace MiniGlobe.Core.Geometry
{
    public class VertexAttributeByte : VertexAttribute<byte>
    {
        public VertexAttributeByte(string name)
            : base(name, VertexAttributeType.Byte)
        {
        }

        public VertexAttributeByte(string name, int capacity)
            : base(name, VertexAttributeType.Byte, capacity)
        {
        }
    }
}
