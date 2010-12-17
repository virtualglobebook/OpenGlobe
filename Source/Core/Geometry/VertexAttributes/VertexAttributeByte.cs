#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

namespace OpenGlobe.Core
{
    public class VertexAttributeByte : VertexAttribute<byte>
    {
        public VertexAttributeByte(string name)
            : base(name, VertexAttributeType.UnsignedByte)
        {
        }

        public VertexAttributeByte(string name, int capacity)
            : base(name, VertexAttributeType.UnsignedByte, capacity)
        {
        }
    }
}
