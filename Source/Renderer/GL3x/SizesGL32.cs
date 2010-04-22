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
using MiniGlobe.Renderer;
using OpenTK;

namespace MiniGlobe.Renderer.GL3x
{
    internal static class SizesGL3x
    {
        public static int SizeOf(IndexBufferDataType type)
        {
            if (type == IndexBufferDataType.UnsignedByte)
            {
                return sizeof(byte);
            }
            else if (type == IndexBufferDataType.UnsignedShort)
            {
                return sizeof(ushort);
            }

            Debug.Assert(type == IndexBufferDataType.UnsignedInt);
            return sizeof(uint);
        }

        public static int SizeOf(VertexAttributeComponentType type)
        {
            if ((type == VertexAttributeComponentType.Byte) ||
                (type == VertexAttributeComponentType.UnsignedByte))
            {
                return sizeof(byte);
            }
            else if (type == VertexAttributeComponentType.Short)
            {
                return sizeof(short);
            }
            else if (type == VertexAttributeComponentType.UnsignedShort)
            {
                return sizeof(ushort);
            }
            else if (type == VertexAttributeComponentType.Int)
            {
                return sizeof(int);
            }
            else if (type == VertexAttributeComponentType.UnsignedInt)
            {
                return sizeof(uint);
            }
            else if (type == VertexAttributeComponentType.Float)
            {
                return sizeof(float);
            }
            else if (type == VertexAttributeComponentType.Double)
            {
                return sizeof(double);
            }

            
            Debug.Assert(type == VertexAttributeComponentType.HalfFloat);
            return MiniGlobe.Core.SizeInBytes<Half>.Value;
        }
    }
}
