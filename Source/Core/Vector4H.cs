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

namespace OpenGlobe.Core
{
    /// <summary>
    /// A set of 4-dimensional cartesian coordinates where the four components,
    /// <see cref="X"/>, <see cref="Y"/>, <see cref="Z"/>, and <see cref="W"/>
    /// are represented as half-precision (16-bit) floating point numbers.
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector4H : IEquatable<Vector4H>
    {
        public static Vector4H Zero
        {
            get { return new Vector4H(0.0, 0.0, 0.0, 0.0); }
        }

        public static Vector4H UnitX
        {
            get { return new Vector4H(1.0, 0.0, 0.0, 0.0); }
        }

        public static Vector4H UnitY
        {
            get { return new Vector4H(0.0, 1.0, 0.0, 0.0); }
        }

        public static Vector4H UnitZ
        {
            get { return new Vector4H(0.0, 0.0, 1.0, 0.0); }
        }

        public static Vector4H UnitW
        {
            get { return new Vector4H(0.0, 0.0, 0.0, 1.0); }
        }

        public static Vector4H Undefined
        {
            get { return new Vector4H(Half.NaN, Half.NaN, Half.NaN, Half.NaN); }
        }

        public Vector4H(Half x, Half y, Half z, Half w)
        {
            _x = x;
            _y = y;
            _z = z;
            _w = w;
        }

        public Vector4H(Vector3H v, Half w)
        {
            _x = v.X;
            _y = v.Y;
            _z = v.Z;
            _w = w;
        }

        public Vector4H(Vector2H v, Half z, Half w)
        {
            _x = v.X;
            _y = v.Y;
            _z = z;
            _w = w;
        }

        public Vector4H(float x, float y, float z, float w)
        {
            _x = new Half(x);
            _y = new Half(y);
            _z = new Half(z);
            _w = new Half(w);
        }

        public Vector4H(double x, double y, double z, double w)
        {
            _x = new Half(x);
            _y = new Half(y);
            _z = new Half(z);
            _w = new Half(w);
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

        public Half W
        {
            get { return _w; }
        }

        public Vector2H XY
        {
            get { return new Vector2H(X, Y); }
        }

        public Vector3H XYZ
        {
            get { return new Vector3H(X, Y, Z); }
        }

        public bool IsUndefined
        {
            get { return Double.IsNaN(_x); }
        }

        public bool Equals(Vector4H other)
        {
            return _x == other._x && _y == other._y && _z == other._z && _w == other._w;
        }

        public static bool operator ==(Vector4H left, Vector4H right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector4H left, Vector4H right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector4H)
            {
                return Equals((Vector4H)obj);
            }
            return false;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "({0}, {1}, {2}, {3})", X, Y, Z, W);
        }

        public override int GetHashCode()
        {
            return _x.GetHashCode() ^ _y.GetHashCode() ^ _z.GetHashCode() ^ _w.GetHashCode();
        }

        public Vector4D ToVector4D()
        {
            return new Vector4D(_x, _y, _z, _w);
        }

        public Vector4F ToVector4F()
        {
            return new Vector4F(_x, _y, _z, _w);
        }

        public Vector4I ToVector4I()
        {
            return new Vector4I((int)_x.ToDouble(), (int)_y.ToDouble(), (int)_z.ToDouble(), (int)_w.ToDouble());
        }

        public Vector4B ToVector4B()
        {
            return new Vector4B(Convert.ToBoolean(_x), Convert.ToBoolean(_y), Convert.ToBoolean(_z), Convert.ToBoolean(_w));
        }

        private readonly Half _x;
        private readonly Half _y;
        private readonly Half _z;
        private readonly Half _w;
    }
}
