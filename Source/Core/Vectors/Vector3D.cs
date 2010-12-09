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
    /// double-precision (64-bit) floating point numbers.
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
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

        public Vector3D(Vector2D v, double z)
        {
            _x = v.X;
            _y = v.Y;
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

        public Vector3D MultiplyComponents(Vector3D scale)
        {
            return new Vector3D(X * scale.X, Y * scale.Y, Z * scale.Z);
        }

        public Vector3D Divide(double scalar)
        {
            return this / scalar;
        }

        public Vector3D MostOrthogonalAxis
        {
            get
            {
                double x = Math.Abs(X);
                double y = Math.Abs(Y);
                double z = Math.Abs(Z);

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

        public double AngleBetween(Vector3D other)
        {
            return Math.Acos(Normalize().Dot(other.Normalize()));
        }

        public Vector3D RotateAroundAxis(Vector3D axis, double theta)
        {
            double u = axis.X;
            double v = axis.Y;
            double w = axis.Z;

            double cosTheta = Math.Cos(theta);
            double sinTheta = Math.Sin(theta);

            double ms = axis.MagnitudeSquared;
            double m = Math.Sqrt(ms);

            return new Vector3D(
                ((u * (u * _x + v * _y + w * _z)) + 
                (((_x * (v * v + w * w)) - (u * (v * _y + w * _z))) * cosTheta) + 
                (m * ((-w * _y) + (v * _z)) * sinTheta)) / ms,

                ((v * (u * _x + v * _y + w * _z)) + 
                (((_y * (u * u + w * w)) - (v * (u * _x + w * _z))) * cosTheta) + 
                (m * ((w * _x) - (u * _z)) * sinTheta)) / ms,

                ((w * (u * _x + v * _y + w * _z)) + 
                (((_z * (u * u + v * v)) - (w * (u * _x + v * _y))) * cosTheta) + 
                (m * (-(v * _x) + (u * _y)) * sinTheta)) / ms);
        }

        public Vector3D Negate()
        {
            return -this;
        }

        public bool EqualsEpsilon(Vector3D other, double epsilon)
        {
            return
                (Math.Abs(_x - other._x) <= epsilon) &&
                (Math.Abs(_y - other._y) <= epsilon) &&
                (Math.Abs(_z - other._z) <= epsilon);
        }

        public bool Equals(Vector3D other)
        {
            return _x == other._x && _y == other._y && _z == other._z;
        }

        public static Vector3D operator -(Vector3D vector)
        {
            return new Vector3D(-vector.X, -vector.Y, -vector.Z);
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

        public static bool operator >(Vector3D left, Vector3D right)
        {
            return (left.X > right.X) && (left.Y > right.Y) && (left.Z > right.Z);
        }

        public static bool operator >=(Vector3D left, Vector3D right)
        {
            return (left.X >= right.X) && (left.Y >= right.Y) && (left.Z >= right.Z);
        }

        public static bool operator <(Vector3D left, Vector3D right)
        {
            return (left.X < right.X) && (left.Y < right.Y) && (left.Z < right.Z);
        }

        public static bool operator <=(Vector3D left, Vector3D right)
        {
            return (left.X <= right.X) && (left.Y <= right.Y) && (left.Z <= right.Z);
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

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "({0}, {1}, {2})", X, Y, Z);
        }

        public override int GetHashCode()
        {
            return _x.GetHashCode() ^ _y.GetHashCode() ^ _z.GetHashCode();
        }

        public Vector3F ToVector3F()
        {
            return new Vector3F((float)_x, (float)_y, (float)_z);
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

        private readonly double _x;
        private readonly double _y;
        private readonly double _z;
    }
}
