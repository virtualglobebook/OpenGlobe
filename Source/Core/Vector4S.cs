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
    /// are represented as single-precision (32-bit) floating point numbers.
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector4S : IEquatable<Vector4S>
    {
        public static Vector4S Zero
        {
            get { return new Vector4S(0.0f, 0.0f, 0.0f, 0.0f); }
        }

        public static Vector4S UnitX
        {
            get { return new Vector4S(1.0f, 0.0f, 0.0f, 0.0f); }
        }

        public static Vector4S UnitY
        {
            get { return new Vector4S(0.0f, 1.0f, 0.0f, 0.0f); }
        }

        public static Vector4S UnitZ
        {
            get { return new Vector4S(0.0f, 0.0f, 1.0f, 0.0f); }
        }

        public static Vector4S UnitW
        {
            get { return new Vector4S(0.0f, 0.0f, 0.0f, 1.0f); }
        }

        public static Vector4S Undefined
        {
            get { return new Vector4S(float.NaN, float.NaN, float.NaN, float.NaN); }
        }

        public Vector4S(float x, float y, float z, float w)
        {
            _x = x;
            _y = y;
            _z = z;
            _w = w;
        }

        public float X
        {
            get { return _x; }
        }

        public float Y
        {
            get { return _y; }
        }

        public float Z
        {
            get { return _z; }
        }

        public float W
        {
            get { return _w; }
        }

        public Vector2S XY
        {
            get { return new Vector2S(X, Y); }
        }

        public Vector3S XYZ
        {
            get { return new Vector3S(X, Y, Z); }
        }

        public float MagnitudeSquared
        {
            get { return _x * _x + _y * _y + _z * _z + _w * _w; }
        }

        public float Magnitude
        {
            get { return (float)Math.Sqrt(MagnitudeSquared); }
        }

        public bool IsUndefined
        {
            get { return float.IsNaN(_x); }
        }

        public Vector4S Normalize(out float magnitude)
        {
            magnitude = Magnitude;
            return this / magnitude;
        }

        public Vector4S Normalize()
        {
            float magnitude;
            return Normalize(out magnitude);
        }

        public float Dot(Vector4S other)
        {
            return X * other.X + Y * other.Y + Z * other.Z + W * other.W;
        }

        public Vector4S Add(Vector4S addend)
        {
            return this + addend;
        }

        public Vector4S Subtract(Vector4S subtrahend)
        {
            return this - subtrahend;
        }

        public Vector4S Multiply(float scalar)
        {
            return this * scalar;
        }

        public Vector4S MultiplyComponents(Vector4S scale)
        {
            return new Vector4S(X * scale.X, Y * scale.Y, Z * scale.Z, W * scale.W);
        }

        public Vector4S Divide(float scalar)
        {
            return this / scalar;
        }

        public Vector4S MostOrthogonalAxis
        {
            get
            {
                float x = Math.Abs(X);
                float y = Math.Abs(Y);
                float z = Math.Abs(Z);
                float w = Math.Abs(W);

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

        public Vector4S Negate()
        {
            return -this;
        }

        public bool EqualsEpsilon(Vector4S other, float epsilon)
        {
            return (Math.Abs(_x - other._x) <= epsilon) &&
                   (Math.Abs(_y - other._y) <= epsilon) &&
                   (Math.Abs(_z - other._z) <= epsilon) &&
                   (Math.Abs(_w - other._w) <= epsilon);
        }

        public bool Equals(Vector4S other)
        {
            return _x == other._x && _y == other._y && _z == other._z && _w == other._w;
        }

        public static Vector4S operator -(Vector4S vector)
        {
            return new Vector4S(-vector.X, -vector.Y, -vector.Z, -vector.W);
        }

        public static Vector4S operator +(Vector4S left, Vector4S right)
        {
            return new Vector4S(left._x + right._x, left._y + right._y, left._z + right._z, left._w + right._w);
        }

        public static Vector4S operator -(Vector4S left, Vector4S right)
        {
            return new Vector4S(left._x - right._x, left._y - right._y, left._z - right._z, left._w - right._w);
        }

        public static Vector4S operator *(Vector4S left, float right)
        {
            return new Vector4S(left._x * right, left._y * right, left._z * right, left._w * right);
        }

        public static Vector4S operator *(float left, Vector4S right)
        {
            return right * left;
        }

        public static Vector4S operator /(Vector4S left, float right)
        {
            return new Vector4S(left._x / right, left._y / right, left._z / right, left._w / right);
        }

        public static bool operator ==(Vector4S left, Vector4S right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector4S left, Vector4S right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector4S)
            {
                return Equals((Vector4S)obj);
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

        public Vector4I ToVector4I()
        {
            return new Vector4I((int)_x, (int)_y, (int)_z, (int)_w);
        }

        public Vector4H ToVector4H()
        {
            return new Vector4H(_x, _y, _z, _w);
        }

        public Vector4B ToVector4B()
        {
            return new Vector4B(Convert.ToBoolean(_x), Convert.ToBoolean(_y), Convert.ToBoolean(_z), Convert.ToBoolean(_w));
        }

        private readonly float _x;
        private readonly float _y;
        private readonly float _z;
        private readonly float _w;
    }
}
