#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;
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
                case IndexBufferDatatype.UnsignedInt:
                    return sizeof(uint);
            }

            throw new ArgumentException("type");
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
                case ComponentDatatype.HalfFloat:
                    return SizeInBytes<Half>.Value;
            }

            throw new ArgumentException("type");
        }
    }
}