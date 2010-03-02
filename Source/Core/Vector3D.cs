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

namespace MiniGlobe.Core
{
    /// <summary>
    /// A set of 3-dimensional cartesian coordinates where the three components,
    /// <see cref="X"/>, <see cref="Y"/>, and <see cref="Z"/>, are represented as
    /// double-precision (64-bit) floating point numbers.
    /// </summary>
    public struct Vector3D : IEquatable<Vector3D>
    {
        public static Vector3D Zero
        {
            get { return new Vector3D(0.0, 0.0, 0.0); }
        }

        public static Vector3D UnitX
        {
            get { return new Vector3D(1.0, 0.0, 0.0); }
        }

        public static Vector3D UnitY
        {
            get { return new Vector3D(0.0, 1.0, 0.0); }
        }

        public static Vector3D UnitZ
        {
            get { return new Vector3D(0.0, 0.0, 1.0); }
        }

        public static Vector3D Undefined
        {
            get { return new Vector3D(Double.NaN, Double.NaN, Double.NaN); }
        }

        public Vector3D(double x, double y, double z)
        {
            _x = x;
            _y = y;
            _z = z;
        }

        public double X
        {
            get { return _x; }
        }

        public double Y
        {
            get { return _y; }
        }

        public double Z
        {
            get { return _z; }
        }

        public Vector2D XY
        {
            get { return new Vector2D(X, Y); }
        }

        public double MagnitudeSquared
        {
            get { return _x * _x + _y * _y + _z * _z; }
        }

        public double Magnitude
        {
            get { return Math.Sqrt(MagnitudeSquared); }
        }

        public bool IsUndefined
        {
            get { return Double.IsNaN(_x); }
        }

        public Vector3D Normalize(out double magnitude)
        {
            magnitude = Magnitude;
            return this / magnitude;
        }

        public Vector3D Normalize()
        {
            double magnitude;
            return Normalize(out magnitude);
        }

        public Vector3D Cross(Vector3D other)
        {
            return new Vector3D(Y * other.Z - Z * other.Y,
                                Z * other.X - X * other.Z,
                                X * other.Y - Y * other.X);
        }

        public double Dot(Vector3D other)
        {
            return X * other.X + Y * other.Y + Z * other.Z;
        }

        public Vector3D Add(Vector3D addend)
        {
            return this + addend;
        }

        public Vector3D Subtract(Vector3D subtrahend)
        {
            return this - subtrahend;
        }

        public Vector3D Multiply(double scalar)
        {
            return this * scalar;
        }

        public Vector3D Divide(double scalar)
        {
            return this / scalar;
        }

        public bool Equals(Vector3D other)
        {
            return _x == other._x && _y == other._y && _z == other._z;
        }

        public static Vector3D operator -(Vector3D vector)
        {
            return new Vector3D(-vector.X, vector.Y, vector.Z);
        }

        public static Vector3D operator +(Vector3D left, Vector3D right)
        {
            return new Vector3D(left._x + right._x, left._y + right._y, left._z + right._z);
        }

        public static Vector3D operator -(Vector3D left, Vector3D right)
        {
            return new Vector3D(left._x - right._x, left._y - right._y, left._z - right._z);
        }

        public static Vector3D operator *(Vector3D left, double right)
        {
            return new Vector3D(left._x * right, left._y * right, left._z * right);
        }

        public static Vector3D operator *(double left, Vector3D right)
        {
            return right * left;
        }

        public static Vector3D operator /(Vector3D left, double right)
        {
            return new Vector3D(left._x / right, left._y / right, left._z / right);
        }

        public static bool operator ==(Vector3D left, Vector3D right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector3D left, Vector3D right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector3D)
            {
                return Equals((Vector3D)obj);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return _x.GetHashCode() ^ _y.GetHashCode() ^ _z.GetHashCode();
        }

        private readonly double _x;
        private readonly double _y;
        private readonly double _z;
    }
}
