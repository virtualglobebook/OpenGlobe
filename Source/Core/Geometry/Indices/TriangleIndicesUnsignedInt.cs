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

namespace OpenGlobe.Core
{
    [CLSCompliant(false)]
    public struct TriangleIndicesUnsignedInt : IEquatable<TriangleIndicesUnsignedInt>
    {
        public TriangleIndicesUnsignedInt(uint ui0, uint ui1, uint ui2)
        {
            _ui0 = ui0;
            _ui1 = ui1;
            _ui2 = ui2;
        }

        public TriangleIndicesUnsignedInt(int i0, int i1, int i2)
        {
            if (i0 < 0)
            {
                throw new ArgumentOutOfRangeException("i0");
            }

            if (i1 < 0)
            {
                throw new ArgumentOutOfRangeException("i1");
            }

            if (i2 < 0)
            {
                throw new ArgumentOutOfRangeException("i2");
            }

            _ui0 = (uint)i0;
            _ui1 = (uint)i1;
            _ui2 = (uint)i2;
        }

        public TriangleIndicesUnsignedInt(TriangleIndicesUnsignedInt other)
        {
            _ui0 = other.UI0;
            _ui1 = other.UI1;
            _ui2 = other.UI2;
        }

        public static bool operator ==(TriangleIndicesUnsignedInt left, TriangleIndicesUnsignedInt right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TriangleIndicesUnsignedInt left, TriangleIndicesUnsignedInt right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "i0: {0} i1: {1} i2: {2}", _ui0, _ui1, _ui2);
        }

        public override int GetHashCode()
        {
            return _ui0.GetHashCode() ^ _ui1.GetHashCode() ^ _ui2.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is TriangleIndicesUnsignedInt))
                return false;

            return this.Equals((TriangleIndicesUnsignedInt)obj);
        }

        #region IEquatable<TriangleIndices> Members

        public bool Equals(TriangleIndicesUnsignedInt other)
        {
            return
                (_ui0.Equals(other.UI0)) &&
                (_ui1.Equals(other.UI1)) &&
                (_ui2.Equals(other.UI2));
        }

        #endregion

        public int I0
        {
            get { return (int)_ui0; }
        }

        public int I1
        {
            get { return (int)_ui1; }
        }

        public int I2
        {
            get { return (int)_ui2; }
        }

        public uint UI0
        {
            get { return _ui0; }
        }

        public uint UI1
        {
            get { return _ui1; }
        }

        public uint UI2
        {
            get { return _ui2; }
        }

        private uint _ui0;
        private uint _ui1;
        private uint _ui2;
    }
}
