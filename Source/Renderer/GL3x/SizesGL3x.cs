#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System.Runtime.InteropServices;
using System.Diagnostics;
using OpenGlobe.Renderer;
using OpenTK;

namespace OpenGlobe.Renderer
{
    internal static class VertexArraySizes
    {
        public static int SizeOf(IndexBufferDataType type)
        {
            switch (type)
            {
                case IndexBufferDataType.UnsignedByte:
                    return sizeof(byte);
                case IndexBufferDataType.UnsignedShort:
                    return sizeof(ushort);
            }

            Debug.Assert(type == IndexBufferDataType.UnsignedInt);
            return sizeof(uint);
        }

        public static int SizeOf(VertexAttributeComponentType type)
        {
            switch (type)
            {
                case VertexAttributeComponentType.Byte:
                case VertexAttributeComponentType.UnsignedByte:
                    return sizeof(byte);
                case VertexAttributeComponentType.Short:
                    return sizeof(short);
                case VertexAttributeComponentType.UnsignedShort:
                    return sizeof(ushort);
                case VertexAttributeComponentType.Int:
                    return sizeof(int);
                case VertexAttributeComponentType.UnsignedInt:
                    return sizeof(uint);
                case VertexAttributeComponentType.Float:
                    return sizeof(float);
                case VertexAttributeComponentType.Double:
                    return sizeof(double);
            }

            Debug.Assert(type == VertexAttributeComponentType.HalfFloat);
            return OpenGlobe.Core.SizeInBytes<Half>.Value;
        }
    }
}
