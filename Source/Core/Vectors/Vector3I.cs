#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;
using System.Runtime.InteropServices;
using System.Globalization;

namespace OpenGlobe.Core
{
    /// <summary>
    /// A set of 3-dimensional cartesian coordinates where the three components,
    /// <see cref="X"/>, <see cref="Y"/>, and <see cref="Z"/>, are represented as
    /// 32-bit integers.
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector3I : IEquatable<Vector3I>
    {
        public static Vector3I Zero
        {
            get { return new Vector3I(0, 0, 0); }
        }

        public static Vector3I UnitX
        {
            get { return new Vector3I(1, 0, 0); }
        }

        public static Vector3I UnitY
        {
            get { return new Vector3I(0, 1, 0); }
        }

        public static Vector3I UnitZ
        {
            get { return new Vector3I(0, 0, 1); }
        }

        public Vector3I(int x, int y, int z)
        {
            _x = x;
            _y = y;
            _z = z;
        }

        public Vector3I(Vector2I v, int z)
        {
            _x = v.X;
            _y = v.Y;
            _z = z;
        }

        public Vector3I(int value)
        {
            _x = value;
            _y = value;
            _z = value;
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

        public Vector2I XY
        {
            get { return new Vector2I(X, Y); }
        }

        public int MagnitudeSquared
        {
            get { return _x * _x + _y * _y + _z * _z; }
        }

        public double Magnitude
        {
            get { return Math.Sqrt(MagnitudeSquared); }
        }

        public Vector3I Cross(Vector3I other)
        {
            return new Vector3I(Y * other.Z - Z * other.Y,
                                Z * other.X - X * other.Z,
                                X * other.Y - Y * other.X);
        }

        public int Dot(Vector3I other)
        {
            return X * other.X + Y * other.Y + Z * other.Z;
        }

        public Vector3I Add(Vector3I addend)
        {
            return this + addend;
        }

        public Vector3I Subtract(Vector3I subtrahend)
        {
            return this - subtrahend;
        }

        public Vector3I Multiply(int scalar)
        {
            return this * scalar;
        }

        public Vector3I MultiplyComponents(Vector3I scale)
        {
            return new Vector3I(X * scale.X, Y * scale.Y, Z * scale.Z);
        }

        public Vector3I Divide(int scalar)
        {
            return this / scalar;
        }

        public Vector3I MostOrthogonalAxis
        {
            get
            {
                int x = Math.Abs(X);
                int y = Math.Abs(Y);
                int z = Math.Abs(Z);

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

        public Vector3I Negate()
        {
            return -this;
        }

        public bool Equals(Vector3I other)
        {
            return _x == other._x && _y == other._y && _z == other._z;
        }

        public static Vector3I operator -(Vector3I vector)
        {
            return new Vector3I(-vector.X, -vector.Y, -vector.Z);
        }

        public static Vector3I operator +(Vector3I left, Vector3I right)
        {
            return new Vector3I(left._x + right._x, left._y + right._y, left._z + right._z);
        }

        public static Vector3I operator -(Vector3I left, Vector3I right)
        {
            return new Vector3I(left._x - right._x, left._y - right._y, left._z - right._z);
        }

        public static Vector3I operator *(Vector3I left, int right)
        {
            return new Vector3I(left._x * right, left._y * right, left._z * right);
        }

        public static Vector3I operator *(int left, Vector3I right)
        {
            return right * left;
        }

        public static Vector3I operator /(Vector3I left, int right)
        {
            return new Vector3I(left._x / right, left._y / right, left._z / right);
        }

        public static bool operator >(Vector3I left, Vector3I right)
        {
            return (left.X > right.X) && (left.Y > right.Y) && (left.Z > right.Z);
        }

        public static bool operator >=(Vector3I left, Vector3I right)
        {
            return (left.X >= right.X) && (left.Y >= right.Y) && (left.Z >= right.Z);
        }

        public static bool operator <(Vector3I left, Vector3I right)
        {
            return (left.X < right.X) && (left.Y < right.Y) && (left.Z < right.Z);
        }

        public static bool operator <=(Vector3I left, Vector3I right)
        {
            return (left.X <= right.X) && (left.Y <= right.Y) && (left.Z <= right.Z);
        }

        public static bool operator ==(Vector3I left, Vector3I right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector3I left, Vector3I right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector3I)
            {
                return Equals((Vector3I)obj);
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
            return new Vector3D((double)_x, (double)_y, (double)_z);
        }

        public Vector3F ToVector3F()
        {
            return new Vector3F((float)_x, (float)_y, (float)_z);
        }

        public Vector3H ToVector3H()
        {
            return new Vector3H(_x, _y, _z);
        }

        public Vector3B ToVector3B()
        {
            return new Vector3B(Convert.ToBoolean(_x), Convert.ToBoolean(_y), Convert.ToBoolean(_z));
        }

        private readonly int _x;
        private readonly int _y;
        private readonly int _z;
    }
}
