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

namespace OpenGlobe.Core
{
    /// <summary>
    /// A set of 2-dimensional cartesian coordinates where the two components,
    /// <see cref="X"/> and <see cref="Y"/>, are represented as
    /// double-precision (64-bit) floating point numbers.
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector2D : IEquatable<Vector2D>
    {
        public static Vector2D Zero
        {
            get { return new Vector2D(0.0, 0.0); }
        }

        public static Vector2D UnitX
        {
            get { return new Vector2D(1.0, 0.0); }
        }

        public static Vector2D UnitY
        {
            get { return new Vector2D(0.0, 1.0); }
        }

        public static Vector2D Undefined
        {
            get { return new Vector2D(Double.NaN, Double.NaN); }
        }

        public Vector2D(double x, double y)
        {
            _x = x;
            _y = y;
        }

        public double X
        {
            get { return _x; }
        }

        public double Y
        {
            get { return _y; }
        }

        public double MagnitudeSquared
        {
            get { return _x * _x + _y * _y; }
        }

        public double Magnitude
        {
            get { return Math.Sqrt(MagnitudeSquared); }
        }

        public bool IsUndefined
        {
            get { return Double.IsNaN(_x); }
        }

        public Vector2D Normalize(out double magnitude)
        {
            magnitude = Magnitude;
            return this / magnitude;
        }

        public Vector2D Normalize()
        {
            double magnitude;
            return Normalize(out magnitude);
        }

        public double Dot(Vector2D other)
        {
            return X * other.X + Y * other.Y;
        }

        public Vector2D Add(Vector2D addend)
        {
            return this + addend;
        }

        public Vector2D Subtract(Vector2D subtrahend)
        {
            return this - subtrahend;
        }

        public Vector2D Multiply(double scalar)
        {
            return this * scalar;
        }

        public Vector2D Divide(double scalar)
        {
            return this / scalar;
        }

        public Vector2D Negate()
        {
            return -this;
        }

        public bool EqualsEpsilon(Vector2D other, double epsilon)
        {
            return
                (Math.Abs(_x - other._x) <= epsilon) &&
                (Math.Abs(_y - other._y) <= epsilon);
        }

        public bool Equals(Vector2D other)
        {
            return _x == other._x && _y == other._y;
        }

        public static Vector2D operator -(Vector2D vector)
        {
            return new Vector2D(-vector.X, -vector.Y);
        }

        public static Vector2D operator +(Vector2D left, Vector2D right)
        {
            return new Vector2D(left._x + right._x, left._y + right._y);
        }

        public static Vector2D operator -(Vector2D left, Vector2D right)
        {
            return new Vector2D(left._x - right._x, left._y - right._y);
        }

        public static Vector2D operator *(Vector2D left, double right)
        {
            return new Vector2D(left._x * right, left._y * right);
        }

        public static Vector2D operator *(double left, Vector2D right)
        {
            return right * left;
        }

        public static Vector2D operator /(Vector2D left, double right)
        {
            return new Vector2D(left._x / right, left._y / right);
        }

        public static bool operator ==(Vector2D left, Vector2D right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector2D left, Vector2D right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector2D)
            {
                return Equals((Vector2D)obj);
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

        public Vector2S ToVector2S()
        {
            return new Vector2S((float)_x, (float)_y);
        }

        public Vector2H ToVector2H()
        {
            return new Vector2H(_x, _y);
        }

        private readonly double _x;
        private readonly double _y;
    }
}
