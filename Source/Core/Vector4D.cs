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
    /// A set of 4-dimensional cartesian coordinates where the four components,
    /// <see cref="X"/>, <see cref="Y"/>, <see cref="Z"/>, and <see cref="W"/>
    /// are represented as double-precision (64-bit) floating point numbers.
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector4D : IEquatable<Vector4D>
    {
        public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Vector4D));

        public static Vector4D Zero
        {
            get { return new Vector4D(0.0, 0.0, 0.0, 0.0); }
        }

        public static Vector4D UnitX
        {
            get { return new Vector4D(1.0, 0.0, 0.0, 0.0); }
        }

        public static Vector4D UnitY
        {
            get { return new Vector4D(0.0, 1.0, 0.0, 0.0); }
        }

        public static Vector4D UnitZ
        {
            get { return new Vector4D(0.0, 0.0, 1.0, 0.0); }
        }

        public static Vector4D UnitW
        {
            get { return new Vector4D(0.0, 0.0, 0.0, 1.0); }
        }

        public static Vector4D Undefined
        {
            get { return new Vector4D(Double.NaN, Double.NaN, Double.NaN, Double.NaN); }
        }

        public Vector4D(double x, double y, double z, double w)
        {
            _x = x;
            _y = y;
            _z = z;
            _w = w;
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

        public double W
        {
            get { return _w; }
        }

        public Vector2D XY
        {
            get { return new Vector2D(X, Y); }
        }

        public Vector3D XYZ
        {
            get { return new Vector3D(X, Y, Z); }
        }

        public double MagnitudeSquared
        {
            get { return _x * _x + _y * _y + _z * _z + _w * _w; }
        }

        public double Magnitude
        {
            get { return Math.Sqrt(MagnitudeSquared); }
        }

        public bool IsUndefined
        {
            get { return Double.IsNaN(_x); }
        }

        public Vector4D Normalize(out double magnitude)
        {
            magnitude = Magnitude;
            return this / magnitude;
        }

        public Vector4D Normalize()
        {
            double magnitude;
            return Normalize(out magnitude);
        }

        public double Dot(Vector4D other)
        {
            return X * other.X + Y * other.Y + Z * other.Z + W * other.W;
        }

        public Vector4D Add(Vector4D addend)
        {
            return this + addend;
        }

        public Vector4D Subtract(Vector4D subtrahend)
        {
            return this - subtrahend;
        }

        public Vector4D Multiply(double scalar)
        {
            return this * scalar;
        }

        public Vector4D MultiplyComponents(Vector4D scale)
        {
            return new Vector4D(X * scale.X, Y * scale.Y, Z * scale.Z, W * scale.W);
        }

        public Vector4D Divide(double scalar)
        {
            return this / scalar;
        }

        public Vector4D MostOrthogonalAxis
        {
            get
            {
                double x = Math.Abs(X);
                double y = Math.Abs(Y);
                double z = Math.Abs(Z);
                double w = Math.Abs(W);

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

        public Vector4D Invert()
        {
            return -this;
        }

        public bool EqualsEpsilon(Vector4D other, double epsilon)
        {
            return (Math.Abs(_x - other._x) <= epsilon) &&
                   (Math.Abs(_y - other._y) <= epsilon) &&
                   (Math.Abs(_z - other._z) <= epsilon) &&
                   (Math.Abs(_w - other._w) <= epsilon);
        }

        public bool Equals(Vector4D other)
        {
            return _x == other._x && _y == other._y && _z == other._z && _w == other._w;
        }

        public static Vector4D operator -(Vector4D vector)
        {
            return new Vector4D(-vector.X, -vector.Y, -vector.Z, -vector.W);
        }

        public static Vector4D operator +(Vector4D left, Vector4D right)
        {
            return new Vector4D(left._x + right._x, left._y + right._y, left._z + right._z, left._w + right._w);
        }

        public static Vector4D operator -(Vector4D left, Vector4D right)
        {
            return new Vector4D(left._x - right._x, left._y - right._y, left._z - right._z, left._w - right._w);
        }

        public static Vector4D operator *(Vector4D left, double right)
        {
            return new Vector4D(left._x * right, left._y * right, left._z * right, left._w * right);
        }

        public static Vector4D operator *(double left, Vector4D right)
        {
            return right * left;
        }

        public static Vector4D operator /(Vector4D left, double right)
        {
            return new Vector4D(left._x / right, left._y / right, left._z / right, left._w / right);
        }

        public static bool operator ==(Vector4D left, Vector4D right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector4D left, Vector4D right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector4D)
            {
                return Equals((Vector4D)obj);
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

        public Vector4S ToVector4S()
        {
            return new Vector4S((float)_x, (float)_y, (float)_z, (float)_w);
        }

        private readonly double _x;
        private readonly double _y;
        private readonly double _z;
        private readonly double _w;
    }
}
