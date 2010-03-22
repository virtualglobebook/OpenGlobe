#region License
//
// (C) Copyright 2010 Patrick Cozzi, Deron Ohlarik, and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Globalization;

namespace MiniGlobe.Core
{
    /// <summary>
    /// A set of 3-dimensional cartesian coordinates where the three components,
    /// <see cref="X"/>, <see cref="Y"/>, and <see cref="Z"/>, are represented as
    /// half-precision (16-bit) floating point numbers.
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector3H : IEquatable<Vector3H>
    {
        public static Vector3H Zero
        {
            get { return new Vector3H(0.0, 0.0, 0.0); }
        }

        public static Vector3H UnitX
        {
            get { return new Vector3H(1.0, 0.0, 0.0); }
        }

        public static Vector3H UnitY
        {
            get { return new Vector3H(0.0, 1.0, 0.0); }
        }

        public static Vector3H UnitZ
        {
            get { return new Vector3H(0.0, 0.0, 1.0); }
        }

        public static Vector3H Undefined
        {
            get { return new Vector3H(Half.NaN, Half.NaN, Half.NaN); }
        }

        public Vector3H(Half x, Half y, Half z)
        {
            _x = x;
            _y = y;
            _z = z;
        }

        public Vector3H(float x, float y, float z)
        {
            _x = new Half(x);
            _y = new Half(y);
            _z = new Half(z);
        }

        public Vector3H(double x, double y, double z)
        {
            _x = new Half(x);
            _y = new Half(y);
            _z = new Half(z);
        }

        public Half X
        {
            get { return _x; }
        }

        public Half Y
        {
            get { return _y; }
        }

        public Half Z
        {
            get { return _z; }
        }

        public Vector2H XY
        {
            get { return new Vector2H(X, Y); }
        }

        public bool IsUndefined
        {
            get { return Double.IsNaN(_x); }
        }

        public bool Equals(Vector3H other)
        {
            return _x == other._x && _y == other._y && _z == other._z;
        }

        public static bool operator ==(Vector3H left, Vector3H right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector3H left, Vector3H right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector3H)
            {
                return Equals((Vector3H)obj);
            }
            return false;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "({0}, {1}, {2})", X, Y, Z);
        }

        public override int GetHashCode()
        {
            return _x.GetHashCode() ^ _y.GetHashCode() ^ _z.GetHashCode();
        }

        public Vector3D ToVector3D()
        {
            return new Vector3D(_x, _y, _z);
        }

        public Vector3S ToVector3S()
        {
            return new Vector3S(_x, _y, _z);
        }

        private readonly Half _x;
        private readonly Half _y;
        private readonly Half _z;
    }
}
