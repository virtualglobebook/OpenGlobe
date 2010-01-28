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

namespace MiniGlobe.Renderer.GL32
{
    internal static class SizesGL32
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
                return Marshal.SizeOf(new byte());
            }
            else if (type == VertexAttributeComponentType.Short)
            {
                return Marshal.SizeOf(new short());
            }
            else if (type == VertexAttributeComponentType.UnsignedShort)
            {
                return Marshal.SizeOf(new ushort());
            }
            else if (type == VertexAttributeComponentType.Int)
            {
                return Marshal.SizeOf(new int());
            }
            else if (type == VertexAttributeComponentType.UnsignedInt)
            {
                return Marshal.SizeOf(new uint());
            }
            else if (type == VertexAttributeComponentType.Float)
            {
                return Marshal.SizeOf(new float());
            }
            else if (type == VertexAttributeComponentType.Double)
            {
                return Marshal.SizeOf(new double());
            }

            
            Debug.Assert(type == VertexAttributeComponentType.HalfFloat);
            return Marshal.SizeOf(new Half());
        }
    }
}
