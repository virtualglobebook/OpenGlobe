#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace OpenGlobe.Core
{
    /// <summary>
    /// A set two booleans.
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector2B : IEquatable<Vector2B>
    {
        public static Vector2B False
        {
            get { return new Vector2B(false, false); }
        }

        public static Vector2B True
        {
            get { return new Vector2B(true, true); }
        }

        public Vector2B(bool x, bool y)
        {
            _x = x;
            _y = y;
        }

        public bool X
        {
            get { return _x; }
        }

        public bool Y
        {
            get { return _y; }
        }

        public bool Equals(Vector2B other)
        {
            return _x == other._x && _y == other._y;
        }

        public static bool operator ==(Vector2B left, Vector2B right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector2B left, Vector2B right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector2B)
            {
                return Equals((Vector2B)obj);
            }
            return false;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "({0}, {1})", X, Y);
        }

        public override int GetHashCode()
        {
            return (Convert.ToInt32(_x) * 2) + Convert.ToInt32(_y);
        }

        public Vector2D ToVector2D()
        {
            return new Vector2D(Convert.ToDouble(_x), Convert.ToDouble(_y));
        }

        public Vector2S ToVector2S()
        {
            return new Vector2S(Convert.ToSingle(_x), Convert.ToSingle(_y));
        }

        public Vector2I ToVector2I()
        {
            return new Vector2I(Convert.ToInt32(_x), Convert.ToInt32(_y));
        }

        public Vector2H ToVector2H()
        {
            return new Vector2H(Convert.ToInt32(_x), Convert.ToInt32(_y));
        }

        private readonly bool _x;
        private readonly bool _y;
    }
}
