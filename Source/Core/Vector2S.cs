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
using System.Globalization;
using System.Runtime.InteropServices;

namespace MiniGlobe.Core
{
    /// <summary>
    /// A set of 2-dimensional cartesian coordinates where the two components,
    /// <see cref="X"/> and <see cref="Y"/>, are represented as
    /// float-precision (64-bit) floating point numbers.
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector2S : IEquatable<Vector2S>
    {
        public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Vector2S));

        public static Vector2S Zero
        {
            get { return new Vector2S(0.0f, 0.0f); }
        }

        public static Vector2S UnitX
        {
            get { return new Vector2S(1.0f, 0.0f); }
        }

        public static Vector2S UnitY
        {
            get { return new Vector2S(0.0f, 1.0f); }
        }

        public static Vector2S Undefined
        {
            get { return new Vector2S(float.NaN, float.NaN); }
        }

        public Vector2S(float x, float y)
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

        public Vector2S Normalize(out float magnitude)
        {
            magnitude = Magnitude;
            return this / magnitude;
        }

        public Vector2S Normalize()
        {
            float magnitude;
            return Normalize(out magnitude);
        }

        public float Dot(Vector2S other)
        {
            return X * other.X + Y * other.Y;
        }

        public Vector2S Add(Vector2S addend)
        {
            return this + addend;
        }

        public Vector2S Subtract(Vector2S subtrahend)
        {
            return this - subtrahend;
        }

        public Vector2S Multiply(float scalar)
        {
            return this * scalar;
        }

        public Vector2S Divide(float scalar)
        {
            return this / scalar;
        }

        public bool Equals(Vector2S other)
        {
            return _x == other._x && _y == other._y;
        }

        public static Vector2S operator -(Vector2S vector)
        {
            return new Vector2S(-vector.X, -vector.Y);
        }

        public static Vector2S operator +(Vector2S left, Vector2S right)
        {
            return new Vector2S(left._x + right._x, left._y + right._y);
        }

        public static Vector2S operator -(Vector2S left, Vector2S right)
        {
            return new Vector2S(left._x - right._x, left._y - right._y);
        }

        public static Vector2S operator *(Vector2S left, float right)
        {
            return new Vector2S(left._x * right, left._y * right);
        }

        public static Vector2S operator *(float left, Vector2S right)
        {
            return right * left;
        }

        public static Vector2S operator /(Vector2S left, float right)
        {
            return new Vector2S(left._x / right, left._y / right);
        }

        public static bool operator ==(Vector2S left, Vector2S right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector2S left, Vector2S right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector2S)
            {
                return Equals((Vector2S)obj);
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

        private readonly float _x;
        private readonly float _y;
    }
}
