#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Globalization;

namespace OpenGlobe.Core.Geometry
{
    public struct TriangleIndicesByte : IEquatable<TriangleIndicesByte>
    {
        public TriangleIndicesByte(byte i0, byte i1, byte i2)
        {
            _i0 = i0;
            _i1 = i1;
            _i2 = i2;
        }

        public TriangleIndicesByte(TriangleIndicesByte other)
        {
            _i0 = other.I0;
            _i1 = other.I1;
            _i2 = other.I2;
        }

        public static bool operator ==(TriangleIndicesByte left, TriangleIndicesByte right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TriangleIndicesByte left, TriangleIndicesByte right)
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
            if (!(obj is TriangleIndicesByte))
                return false;

            return this.Equals((TriangleIndicesByte)obj);
        }

        #region IEquatable<TriangleIndices> Members

        public bool Equals(TriangleIndicesByte other)
        {
            return
                (_i0.Equals(other.I0)) &&
                (_i1.Equals(other.I1)) &&
                (_i2.Equals(other.I2));
        }

        #endregion

        public byte I0
        {
            get { return _i0; }
        }

        public byte I1
        {
            get { return _i1; }
        }

        public byte I2
        {
            get { return _i2; }
        }

        private byte _i0;
        private byte _i1;
        private byte _i2;
    }
}
