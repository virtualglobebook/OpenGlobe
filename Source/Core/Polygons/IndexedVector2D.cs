#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Globalization;

namespace OpenGlobe.Core
{
    internal struct IndexedVector2D : IEquatable<IndexedVector2D>
    {
        public IndexedVector2D(Vector2D vector, int index)
        {
            _vector = vector;
            _index = index;
        }

        public Vector2D Vector
        {
            get { return _vector; }
        }

        public int Index
        {
            get { return _index; }
        }

        public static bool operator ==(IndexedVector2D left, IndexedVector2D right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(IndexedVector2D left, IndexedVector2D right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "{0}, {1}", _vector, _index);
        }

        public override int GetHashCode()
        {
            return _vector.GetHashCode() ^ _index.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is IndexedVector2D))
                return false;

            return this.Equals((IndexedVector2D)obj);
        }

        #region IEquatable<IndexedVector2D> Members

        public bool Equals(IndexedVector2D other)
        {
            return (_vector == other._vector) && (_index == other._index);
        }

        #endregion

        private Vector2D _vector;
        private int _index;
    }
}