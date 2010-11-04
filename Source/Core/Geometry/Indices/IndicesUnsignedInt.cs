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

namespace OpenGlobe.Core
{
    [CLSCompliant(false)]
    public class IndicesUnsignedInt : IndicesBase
    {
        public IndicesUnsignedInt()
            : base(IndicesType.UnsignedInt)
        {
            _values = new List<uint>();
        }

        public IndicesUnsignedInt(int capacity)
            : base(IndicesType.UnsignedInt)
        {
            _values = new List<uint>(capacity);
        }

        public IList<uint> Values
        {
            get { return _values; }
        }

        public void AddTriangle(TriangleIndicesUnsignedInt triangle)
        {
            _values.Add(triangle.UI0);
            _values.Add(triangle.UI1);
            _values.Add(triangle.UI2);
        }

        private List<uint> _values;
    }
}
