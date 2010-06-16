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
    public class IndicesInt16 : IndicesBase
    {
        public IndicesInt16()
            : base(IndicesType.Int16)
        {
            _values = new List<short>();
        }

        public IndicesInt16(int capacity)
            : base(IndicesType.Int16)
        {
            _values = new List<short>(capacity);
        }

        public IList<short> Values
        {
            get { return _values; }
        }

        public void AddTriangle(TriangleIndicesInt16 triangle)
        {
            _values.Add(triangle.I0);
            _values.Add(triangle.I1);
            _values.Add(triangle.I2);
        }

        private List<short> _values;
    }
}
