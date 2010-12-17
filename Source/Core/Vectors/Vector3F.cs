#region License
//
// (C) Copyright 2010 Patrick Cozzi, Deron Ohlarik, and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
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
    public struct Vector3F : IEquatable<Vector3F>
    {
        public static Vector3F Zero
        {
            get { return new Vector3F(0.0f, 0.0f, 0.0f); }
        }

        public static Vector3F UnitX
        {
            get { return new Vector3F(1.0f, 0.0f, 0.0f); }
        }

        public static Vector3F UnitY
        {
            get { return new Vector3F(0.0f, 1.0f, 0.0f); }
        }

        public static Vector3F UnitZ
        {
            get { return new Vector3F(0.0f, 0.0f, 1.0f); }
        }

        public static Vector3F Undefined
        {
            get { return new Vector3F(float.NaN, float.NaN, float.NaN); }
        }

        public Vector3F(float x, float y, float z)
        {
            _x = x;
            _y = y;
            _z = z;
        }

        public Vector3F(Vector2F v, float z)
        {
            _x = v.X;
            _y = v.Y;
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

        public Vector3F Normalize(out float magnitude)
        {
            magnitude = Magnitude;
            return this / magnitude;
        }

        public Vector3F Normalize()
        {
            float magnitude;
            return Normalize(out magnitude);
        }

        public Vector3F Cross(Vector3F other)
        {
            return new Vector3F(Y * other.Z - Z * other.Y,
                                Z * other.X - X * other.Z,
                                X * other.Y - Y * other.X);
        }

        public float Dot(Vector3F other)
        {
            return X * other.X + Y * other.Y + Z * other.Z;
        }

        public Vector3F Add(Vector3F addend)
        {
            return this + addend;
        }

        public Vector3F Subtract(Vector3F subtrahend)
        {
            return this - subtrahend;
        }

        public Vector3F Multiply(float scalar)
        {
            return this * scalar;
        }

        public Vector3F MultiplyComponents(Vector3F scale)
        {
            return new Vector3F(X * scale.X, Y * scale.Y, Z * scale.Z);
        }

        public Vector3F Divide(float scalar)
        {
            return this / scalar;
        }

        public Vector3F MostOrthogonalAxis
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

        public Vector3F Negate()
        {
            return -this;
        }

        public bool EqualsEpsilon(Vector3F other, float epsilon)
        {
            return
                (Math.Abs(_x - other._x) <= epsilon) &&
                (Math.Abs(_y - other._y) <= epsilon) &&
                (Math.Abs(_z - other._z) <= epsilon);
        }

        public bool Equals(Vector3F other)
        {
            return _x == other._x && _y == other._y && _z == other._z;
        }

        public static Vector3F operator -(Vector3F vector)
        {
            return new Vector3F(-vector.X, -vector.Y, -vector.Z);
        }

        public static Vector3F operator +(Vector3F left, Vector3F right)
        {
            return new Vector3F(left._x + right._x, left._y + right._y, left._z + right._z);
        }

        public static Vector3F operator -(Vector3F left, Vector3F right)
        {
            return new Vector3F(left._x - right._x, left._y - right._y, left._z - right._z);
        }

        public static Vector3F operator *(Vector3F left, float right)
        {
            return new Vector3F(left._x * right, left._y * right, left._z * right);
        }

        public static Vector3F operator *(float left, Vector3F right)
        {
            return right * left;
        }

        public static Vector3F operator /(Vector3F left, float right)
        {
            return new Vector3F(left._x / right, left._y / right, left._z / right);
        }

        public static bool operator ==(Vector3F left, Vector3F right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector3F left, Vector3F right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector3F)
            {
                return Equals((Vector3F)obj);
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

        public Vector3I ToVector3I()
        {
            return new Vector3I((int)_x, (int)_y, (int)_z);
        }

        public Vector3H ToVector3H()
        {
            return new Vector3H(_x, _y, _z);
        }

        public Vector3B ToVector3B()
        {
            return new Vector3B(Convert.ToBoolean(_x), Convert.ToBoolean(_y), Convert.ToBoolean(_z));
        }

        private readonly float _x;
        private readonly float _y;
        private readonly float _z;
    }
}
