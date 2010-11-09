#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.
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
    /// are represented as integers.
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector4I : IEquatable<Vector4I>
    {
        public static Vector4I Zero
        {
            get { return new Vector4I(0, 0, 0, 0); }
        }

        public static Vector4I UnitX
        {
            get { return new Vector4I(1, 0, 0, 0); }
        }

        public static Vector4I UnitY
        {
            get { return new Vector4I(0, 1, 0, 0); }
        }

        public static Vector4I UnitZ
        {
            get { return new Vector4I(0, 0, 1, 0); }
        }

        public static Vector4I UnitW
        {
            get { return new Vector4I(0, 0, 0, 1); }
        }

        public Vector4I(int x, int y, int z, int w)
        {
            _x = x;
            _y = y;
            _z = z;
            _w = w;
        }

        public int X
        {
            get { return _x; }
        }

        public int Y
        {
            get { return _y; }
        }

        public int Z
        {
            get { return _z; }
        }

        public int W
        {
            get { return _w; }
        }

        public Vector2I XY
        {
            get { return new Vector2I(X, Y); }
        }

        public Vector3I XYZ
        {
            get { return new Vector3I(X, Y, Z); }
        }

        public int MagnitudeSquared
        {
            get { return _x * _x + _y * _y + _z * _z + _w * _w; }
        }

        public double Magnitude
        {
            get { return Math.Sqrt(MagnitudeSquared); }
        }

        public int Dot(Vector4I other)
        {
            return X * other.X + Y * other.Y + Z * other.Z + W * other.W;
        }

        public Vector4I Add(Vector4I addend)
        {
            return this + addend;
        }

        public Vector4I Subtract(Vector4I subtrahend)
        {
            return this - subtrahend;
        }

        public Vector4I Multiply(int scalar)
        {
            return this * scalar;
        }

        public Vector4I MultiplyComponents(Vector4I scale)
        {
            return new Vector4I(X * scale.X, Y * scale.Y, Z * scale.Z, W * scale.W);
        }

        public Vector4I Divide(int scalar)
        {
            return this / scalar;
        }

        public Vector4I MostOrthogonalAxis
        {
            get
            {
                int x = Math.Abs(X);
                int y = Math.Abs(Y);
                int z = Math.Abs(Z);
                int w = Math.Abs(W);

                if ((x < y) && (x < z) && (x < w))
                {
                    return UnitX;
                }
                else if ((y < x) && (y < z) && (y < w))
                {
                    return UnitY;
                }
                else if ((z < x) && (z < y) && (z < w))
                {
                    return UnitZ;
                }
                else
                {
                    return UnitW;
                }
            }
        }

        public Vector4I Negate()
        {
            return -this;
        }

        public bool Equals(Vector4I other)
        {
            return _x == other._x && _y == other._y && _z == other._z && _w == other._w;
        }

        public static Vector4I operator -(Vector4I vector)
        {
            return new Vector4I(-vector.X, -vector.Y, -vector.Z, -vector.W);
        }

        public static Vector4I operator +(Vector4I left, Vector4I right)
        {
            return new Vector4I(left._x + right._x, left._y + right._y, left._z + right._z, left._w + right._w);
        }

        public static Vector4I operator -(Vector4I left, Vector4I right)
        {
            return new Vector4I(left._x - right._x, left._y - right._y, left._z - right._z, left._w - right._w);
        }

        public static Vector4I operator *(Vector4I left, int right)
        {
            return new Vector4I(left._x * right, left._y * right, left._z * right, left._w * right);
        }

        public static Vector4I operator *(int left, Vector4I right)
        {
            return right * left;
        }

        public static Vector4I operator /(Vector4I left, int right)
        {
            return new Vector4I(left._x / right, left._y / right, left._z / right, left._w / right);
        }

        public static bool operator ==(Vector4I left, Vector4I right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector4I left, Vector4I right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector4I)
            {
                return Equals((Vector4I)obj);
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
            return new Vector4D((double)_x, (double)_y, (double)_z, (double)_w);
        }

        public Vector4S ToVector4S()
        {
            return new Vector4S((float)_x, (float)_y, (float)_z, (float)_w);
        }

        public Vector4H ToVector4H()
        {
            return new Vector4H(_x, _y, _z, _w);
        }

        private readonly int _x;
        private readonly int _y;
        private readonly int _z;
        private readonly int _w;
    }
}
