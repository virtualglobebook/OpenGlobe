#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Globalization;

namespace OpenGlobe.Core.Geometry
{
    public struct TriangleIndicesInt16 : IEquatable<TriangleIndicesInt16>
    {
        public TriangleIndicesInt16(short i0, short i1, short i2)
        {
            _i0 = i0;
            _i1 = i1;
            _i2 = i2;
        }

        public TriangleIndicesInt16(TriangleIndicesInt16 other)
        {
            _i0 = other.I0;
            _i1 = other.I1;
            _i2 = other.I2;
        }

        public static bool operator ==(TriangleIndicesInt16 left, TriangleIndicesInt16 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TriangleIndicesInt16 left, TriangleIndicesInt16 right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "i0: {0} i1: {1} i2: {2}", _i0, _i1, _i2);
        }

        public override int GetHashCode()
        {
            return _i0.GetHashCode() ^ _i1.GetHashCode() ^ _i2.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is TriangleIndicesInt16))
                return false;

            return this.Equals((TriangleIndicesInt16)obj);
        }

        #region IEquatable<TriangleIndices> Members

        public bool Equals(TriangleIndicesInt16 other)
        {
            return
                (_i0.Equals(other.I0)) &&
                (_i1.Equals(other.I1)) &&
                (_i2.Equals(other.I2));
        }

        #endregion

        public short I0
        {
            get { return _i0; }
        }

        public short I1
        {
            get { return _i1; }
        }

        public short I2
        {
            get { return _i2; }
        }

        private short _i0;
        private short _i1;
        private short _i2;
    }
}
