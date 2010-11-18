#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Runtime.InteropServices;
using System.Globalization;

namespace OpenGlobe.Core
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct EmulatedVector3D : IEquatable<EmulatedVector3D>
    {
        public EmulatedVector3D(Vector3D v)
        {
            _high = v.ToVector3F();
            _low = (v - _high.ToVector3D()).ToVector3F();
        }

        public Vector3F High
        {
            get { return _high; }
        }

        public Vector3F Low
        {
            get { return _low; }
        }

        public bool Equals(EmulatedVector3D other)
        {
            return _high == other._high && _low == other._low;
        }

        public static bool operator ==(EmulatedVector3D left, EmulatedVector3D right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(EmulatedVector3D left, EmulatedVector3D right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            if (obj is EmulatedVector3D)
            {
                return Equals((EmulatedVector3D)obj);
            }
            return false;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "({0}, {1})", High, Low);
        }

        public override int GetHashCode()
        {
            return _high.GetHashCode() ^ _low.GetHashCode();
        }

        private readonly Vector3F _high;
        private readonly Vector3F _low;
    }
}
