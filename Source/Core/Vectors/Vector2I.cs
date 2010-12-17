#region License
//
// (C) Copyright 2010 Patrick Cozzi, Deron Ohlarik, and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace OpenGlobe.Core
{
    /// <summary>
    /// A set of 2-dimensional cartesian coordinates where the two components,
    /// <see cref="X"/> and <see cref="Y"/>, are represented as 32-bit integers.
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector2I : IEquatable<Vector2I>
    {
        public static Vector2I Zero
        {
            get { return new Vector2I(0, 0); }
        }

        public static Vector2I UnitX
        {
            get { return new Vector2I(1, 0); }
        }

        public static Vector2I UnitY
        {
            get { return new Vector2I(0, 1); }
        }

        public Vector2I(int x, int y)
        {
            _x = x;
            _y = y;
        }

        public int X
        {
            get { return _x; }
        }

        public int Y
        {
            get { return _y; }
        }

        public Vector2I Add(Vector2I addend)
        {
            return this + addend;
        }

        public Vector2I Subtract(Vector2I subtrahend)
        {
            return this - subtrahend;
        }

        public Vector2I Multiply(int scalar)
        {
            return this * scalar;
        }

        public Vector2I Negate()
        {
            return -this;
        }

        public bool Equals(Vector2I other)
        {
            return _x == other._x && _y == other._y;
        }

        public static Vector2I operator -(Vector2I vector)
        {
            return new Vector2I(-vector.X, -vector.Y);
        }

        public static Vector2I operator +(Vector2I left, Vector2I right)
        {
            return new Vector2I(left._x + right._x, left._y + right._y);
        }

        public static Vector2I operator -(Vector2I left, Vector2I right)
        {
            return new Vector2I(left._x - right._x, left._y - right._y);
        }

        public static Vector2I operator *(Vector2I left, int right)
        {
            return new Vector2I(left._x * right, left._y * right);
        }

        public static Vector2I operator *(int left, Vector2I right)
        {
            return right * left;
        }

        public static bool operator ==(Vector2I left, Vector2I right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector2I left, Vector2I right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector2I)
            {
                return Equals((Vector2I)obj);
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

        public Vector2F ToVector2F()
        {
            return new Vector2F(_x, _y);
        }

        public Vector2H ToVector2H()
        {
            return new Vector2H(_x, _y);
        }

        public Vector2B ToVector2B()
        {
            return new Vector2B(Convert.ToBoolean(_x), Convert.ToBoolean(_y));
        }

        private readonly int _x;
        private readonly int _y;
    }
}
