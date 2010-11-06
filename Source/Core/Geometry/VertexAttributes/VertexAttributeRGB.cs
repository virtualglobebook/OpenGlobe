#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Drawing;

namespace OpenGlobe.Core
{
    public class VertexAttributeRGB : VertexAttribute<byte>
    {
        public VertexAttributeRGB(string name)
            : base(name, VertexAttributeType.UnsignedByte)
        {
        }

        public VertexAttributeRGB(string name, int capacity)
            : base(name, VertexAttributeType.UnsignedByte, capacity * 3)
        {
            if (capacity > int.MaxValue / 3)
            {
                throw new ArgumentOutOfRangeException("capacity", "capacity causes int overflow.");
            }
        }

        public void AddColor(Color color)
        {
            Values.Add(color.R);
            Values.Add(color.G);
            Values.Add(color.B);
        }
    }
}