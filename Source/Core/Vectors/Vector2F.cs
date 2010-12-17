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
using System.Globalization;
using System.Runtime.InteropServices;

namespace OpenGlobe.Core
{
    /// <summary>
    /// A set of 2-dimensional cartesian coordinates where the two components,
    /// <see cref="X"/> and <see cref="Y"/>, are represented as
    /// single-precision (32-bit) floating point numbers.
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector2F : IEquatable<Vector2F>
    {
        public static Vector2F Zero
        {
            get { return new Vector2F(0.0f, 0.0f); }
        }

        public static Vector2F UnitX
        {
            get { return new Vector2F(1.0f, 0.0f); }
        }

        public static Vector2F UnitY
        {
            get { return new Vector2F(0.0f, 1.0f); }
        }

        public static Vector2F Undefined
        {
            get { return new Vector2F(float.NaN, float.NaN); }
        }

        public Vector2F(float x, float y)
        {
            _x = x;
            _y = y;
        }

        public float X
        {
            get { return _x; }
        }

        public float Y
        {
            get { return _y; }
        }

        public float MagnitudeSquared
        {
            get { return _x * _x + _y * _y; }
        }

        public float Magnitude
        {
            get { return (float)Math.Sqrt(MagnitudeSquared); }
        }

        public bool IsUndefined
        {
            get { return float.IsNaN(_x); }
        }

        public Vector2F Normalize(out float magnitude)
        {
            magnitude = Magnitude;
            return this / magnitude;
        }

        public Vector2F Normalize()
        {
            float magnitude;
            return Normalize(out magnitude);
        }

        public float Dot(Vector2F other)
        {
            return X * other.X + Y * other.Y;
        }

        public Vector2F Add(Vector2F addend)
        {
            return this + addend;
        }

        public Vector2F Subtract(Vector2F subtrahend)
        {
            return this - subtrahend;
        }

        public Vector2F Multiply(float scalar)
        {
            return this * scalar;
        }

        public Vector2F Divide(float scalar)
        {
            return this / scalar;
        }

        public Vector2F Negate()
        {
            return -this;
        }

        public bool EqualsEpsilon(Vector2F other, float epsilon)
        {
            return
                (Math.Abs(_x - other._x) <= epsilon) &&
                (Math.Abs(_y - other._y) <= epsilon);
        }

        public bool Equals(Vector2F other)
        {
            return _x == other._x && _y == other._y;
        }

        public static Vector2F operator -(Vector2F vector)
        {
            return new Vector2F(-vector.X, -vector.Y);
        }

        public static Vector2F operator +(Vector2F left, Vector2F right)
        {
            return new Vector2F(left._x + right._x, left._y + right._y);
        }

        public static Vector2F operator -(Vector2F left, Vector2F right)
        {
            return new Vector2F(left._x - right._x, left._y - right._y);
        }

        public static Vector2F operator *(Vector2F left, float right)
        {
            return new Vector2F(left._x * right, left._y * right);
        }

        public static Vector2F operator *(float left, Vector2F right)
        {
            return right * left;
        }

        public static Vector2F operator /(Vector2F left, float right)
        {
            return new Vector2F(left._x / right, left._y / right);
        }

        public static bool operator ==(Vector2F left, Vector2F right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector2F left, Vector2F right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector2F)
            {
                return Equals((Vector2F)obj);
            }
            return false;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "({0}, {1})", X, Y);
        }

        public override int GetHashCode()
        {
            return _x.GetHashCode() ^ _y.GetHashCode();
        }

        public Vector2D ToVector2D()
        {
            return new Vector2D(_x, _y);
        }

        public Vector2H ToVector2H()
        {
            return new Vector2H(_x, _y);
        }

        private readonly float _x;
        private readonly float _y;
    }
}
