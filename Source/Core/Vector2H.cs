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
    /// half-precision (16-bit) floating point numbers.
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector2H : IEquatable<Vector2H>
    {
        public static Vector2H Zero
        {
            get { return new Vector2H(0.0f, 0.0f); }
        }

        public static Vector2H UnitX
        {
            get { return new Vector2H(1.0f, 0.0f); }
        }

        public static Vector2H UnitY
        {
            get { return new Vector2H(0.0f, 1.0f); }
        }

        public static Vector2H Undefined
        {
            get { return new Vector2H(Half.NaN, Half.NaN); }
        }

        public Vector2H(Half x, Half y)
        {
            _x = x;
            _y = y;
        }

        public Vector2H(float x, float y)
        {
            _x = new Half(x);
            _y = new Half(y);
        }

        public Vector2H(double x, double y)
        {
            _x = new Half(x);
            _y = new Half(y);
        }

        public Half X
        {
            get { return _x; }
        }

        public Half Y
        {
            get { return _y; }
        }

        public bool IsUndefined
        {
            get { return _x.IsNaN; }
        }

        public bool Equals(Vector2H other)
        {
            return _x == other._x && _y == other._y;
        }

        public static bool operator ==(Vector2H left, Vector2H right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector2H left, Vector2H right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector2H)
            {
                return Equals((Vector2H)obj);
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

        public Vector2I ToVector2I()
        {
            return new Vector2I(Convert.ToInt32(_x), Convert.ToInt32(_y));
        }

        public Vector2B ToVector2B()
        {
            return new Vector2B(Convert.ToBoolean(_x), Convert.ToBoolean(_y));
        }

        private readonly Half _x;
        private readonly Half _y;
    }
}
