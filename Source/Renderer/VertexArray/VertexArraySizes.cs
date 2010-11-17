#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System.Diagnostics;
using OpenGlobe.Core;

namespace OpenGlobe.Renderer
{
    internal static class VertexArraySizes
    {
        public static int SizeOf(IndexBufferDatatype type)
        {
            switch (type)
            {
                case IndexBufferDatatype.UnsignedShort:
                    return sizeof(ushort);
            }

            Debug.Assert(type == IndexBufferDatatype.UnsignedInt);
            return sizeof(uint);
        }

        public static int SizeOf(ComponentDatatype type)
        {
            switch (type)
            {
                case ComponentDatatype.Byte:
                case ComponentDatatype.UnsignedByte:
                    return sizeof(byte);
                case ComponentDatatype.Short:
                    return sizeof(short);
                case ComponentDatatype.UnsignedShort:
                    return sizeof(ushort);
                case ComponentDatatype.Int:
                    return sizeof(int);
                case ComponentDatatype.UnsignedInt:
                    return sizeof(uint);
                case ComponentDatatype.Float:
                    return sizeof(float);
            }

            Debug.Assert(type == ComponentDatatype.HalfFloat);
            return OpenGlobe.Core.SizeInBytes<Half>.Value;
        }
    }
}