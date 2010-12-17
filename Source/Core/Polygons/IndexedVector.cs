#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;
using System.Globalization;

namespace OpenGlobe.Core
{
    internal struct IndexedVector<T> : IEquatable<IndexedVector<T>>
    {
        public IndexedVector(T vector, int index)
        {
            _vector = vector;
            _index = index;
        }

        public T Vector
        {
            get { return _vector; }
        }

        public int Index
        {
            get { return _index; }
        }

        public static bool operator ==(IndexedVector<T> left, IndexedVector<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(IndexedVector<T> left, IndexedVector<T> right)
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
            if (!(obj is IndexedVector<T>))
                return false;

            return this.Equals((IndexedVector<T>)obj);
        }

        #region IEquatable<IndexedVector<T>> Members

        public bool Equals(IndexedVector<T> other)
        {
            return (_vector.Equals(other._vector)) && (_index == other._index);
        }

        #endregion

        private T _vector;
        private int _index;
    }
}