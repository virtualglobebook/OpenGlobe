#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System.Collections.Generic;

namespace OpenGlobe.Core.Geometry
{
    public class IndicesInt32 : IndicesBase
    {
        public IndicesInt32()
            : base(IndicesType.Int32)
        {
            _values = new List<int>();
        }

        public IndicesInt32(int capacity)
            : base(IndicesType.Int32)
        {
            _values = new List<int>(capacity);
        }

        public IList<int> Values
        {
            get { return _values; }
        }

        public void AddTriangle(TriangleIndicesInt32 triangle)
        {
            _values.Add(triangle.I0);
            _values.Add(triangle.I1);
            _values.Add(triangle.I2);
        }

        private List<int> _values;
    }
}
