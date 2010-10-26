#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Collections.Generic;

namespace OpenGlobe.Core.Geometry
{
    [CLSCompliant(false)]
    public class IndicesUnsignedShort : IndicesBase
    {
        public IndicesUnsignedShort()
            : base(IndicesType.UnsignedShort)
        {
            _values = new List<ushort>();
        }

        public IndicesUnsignedShort(int capacity)
            : base(IndicesType.UnsignedShort)
        {
            _values = new List<ushort>(capacity);
        }

        public IList<ushort> Values
        {
            get { return _values; }
        }

        public void AddTriangle(TriangleIndicesUnsignedShort triangle)
        {
            _values.Add(triangle.UI0);
            _values.Add(triangle.UI1);
            _values.Add(triangle.UI2);
        }

        private List<ushort> _values;
    }
}
