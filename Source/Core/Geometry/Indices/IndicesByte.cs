#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System.Collections.Generic;

namespace OpenGlobe.Core
{
    public class IndicesByte : IndicesBase
    {
        public IndicesByte()
            : base(IndicesType.Byte)
        {
            _values = new List<byte>();
        }

        public IndicesByte(int capacity)
            : base(IndicesType.Byte)
        {
            _values = new List<byte>(capacity);
        }

        public IList<byte> Values
        {
            get { return _values; }
        }

        public void AddTriangle(TriangleIndicesByte triangle)
        {
            _values.Add(triangle.I0);
            _values.Add(triangle.I1);
            _values.Add(triangle.I2);
        }

        private List<byte> _values;
    }
}
