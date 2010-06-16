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
    /// A set of 3-dimensional cartesian coordinates where the three components,
    /// <see cref="X"/>, <see cref="Y"/>, and <see cref="Z"/>, are represented as
    /// single-precision (32-bit) floating point numbers.
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector3S : IEquatable<Vector3S>
    {
        public static Vector3S Zero
        {
            get { return new Vector3S(0.0f, 0.0f, 0.0f); }
        }

        public static Vector3S UnitX
        {
            get { return new Vector3S(1.0f, 0.0f, 0.0f); }
        }

        public static Vector3S UnitY
        {
            get { return new Vector3S(0.0f, 1.0f, 0.0f); }
        }

        public static Vector3S UnitZ
        {
            get { return new Vector3S(0.0f, 0.0f, 1.0f); }
        }

        public static Vector3S Undefined
        {
            get { return new Vector3S(float.NaN, float.NaN, float.NaN); }
        }

        public Vector3S(float x, float y, float z)
        {
            _x = x;
            _y = y;
            _z = z;
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

        public Vector2D XY
        {
            get { return new Vector2D(X, Y); }
        }

        public float MagnitudeSquared
        {
            get { return _x * _x + _y * _y + _z * _z; }
        }

        public float Magnitude
        {
            get { return (float)Math.Sqrt(MagnitudeSquared); }
        }

        public bool IsUndefined
        {
            get { return Double.IsNaN(_x); }
        }

        public Vector3S Normalize(out float magnitude)
        {
            magnitude = Magnitude;
            return this / magnitude;
        }

        public Vector3S Normalize()
        {
            float magnitude;
            return Normalize(out magnitude);
        }

        public Vector3S Cross(Vector3S other)
        {
            return new Vector3S(Y * other.Z - Z * other.Y,
                                Z * other.X - X * other.Z,
                                X * other.Y - Y * other.X);
        }

        public float Dot(Vector3S other)
        {
            return X * other.X + Y * other.Y + Z * other.Z;
        }

        public Vector3S Add(Vector3S addend)
        {
            return this + addend;
        }

        public Vector3S Subtract(Vector3S subtrahend)
        {
            return this - subtrahend;
        }

        public Vector3S Multiply(float scalar)
        {
            return this * scalar;
        }

        public Vector3S MultiplyComponents(Vector3S scale)
        {
            return new Vector3S(X * scale.X, Y * scale.Y, Z * scale.Z);
        }

        public Vector3S Divide(float scalar)
        {
            return this / scalar;
        }

        public Vector3S MostOrthogonalAxis
        {
            get
            {
                float x = Math.Abs(X);
                float y = Math.Abs(Y);
                float z = Math.Abs(Z);

                if ((x < y) && (x < z))
                {
                    return UnitX;
                }
                else if ((y < x) && (y < z))
                {
                    return UnitY;
                }
                else
                {
                    return UnitZ;
                }
            }
        }

        public Vector3S Negate()
        {
            return -this;
        }

        public bool EqualsEpsilon(Vector3S other, float epsilon)
        {
            return
                (Math.Abs(_x - other._x) <= epsilon) &&
                (Math.Abs(_y - other._y) <= epsilon) &&
                (Math.Abs(_z - other._z) <= epsilon);
        }

        public bool Equals(Vector3S other)
        {
            return _x == other._x && _y == other._y && _z == other._z;
        }

        public static Vector3S operator -(Vector3S vector)
        {
            return new Vector3S(-vector.X, -vector.Y, -vector.Z);
        }

        public static Vector3S operator +(Vector3S left, Vector3S right)
        {
            return new Vector3S(left._x + right._x, left._y + right._y, left._z + right._z);
        }

        public static Vector3S operator -(Vector3S left, Vector3S right)
        {
            return new Vector3S(left._x - right._x, left._y - right._y, left._z - right._z);
        }

        public static Vector3S operator *(Vector3S left, float right)
        {
            return new Vector3S(left._x * right, left._y * right, left._z * right);
        }

        public static Vector3S operator *(float left, Vector3S right)
        {
            return right * left;
        }

        public static Vector3S operator /(Vector3S left, float right)
        {
            return new Vector3S(left._x / right, left._y / right, left._z / right);
        }

        public static bool operator ==(Vector3S left, Vector3S right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector3S left, Vector3S right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector3S)
            {
                return Equals((Vector3S)obj);
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

        public Vector3H ToVector3H()
        {
            return new Vector3H(_x, _y, _z);
        }

        private readonly float _x;
        private readonly float _y;
        private readonly float _z;
    }
}
