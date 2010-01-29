#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using OpenTK;
using System;
using System.Collections.Generic;

namespace MiniGlobe.Core.Geometry
{
    public class Indices<T> : IndicesBase where T : IEquatable<T>
    {
        protected Indices(IndicesType type)
            : base(type)
        {
            _values = new List<T>();
        }

        protected Indices(IndicesType type, int capacity)
            : base(type)
        {
            _values = new List<T>(capacity);
        }

        public IList<T> Values
        {
            get { return _values; }
        }

        public void AddTriangle(TriangleIndices<T> triangle)
        {
            _values.Add(triangle.I0);
            _values.Add(triangle.I1);
            _values.Add(triangle.I2);
        }

        private List<T> _values;
    }
}
