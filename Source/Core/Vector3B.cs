#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Runtime.InteropServices;
using System.Globalization;

namespace OpenGlobe.Core
{
    /// <summary>
    /// A set of three booleans.
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector3B : IEquatable<Vector3B>
    {
        public static Vector3B False
        {
            get { return new Vector3B(false, false, false); }
        }

        public static Vector3B True
        {
            get { return new Vector3B(true, true, true); }
        }


        public Vector3B(bool x, bool y, bool z)
        {
            _x = x;
            _y = y;
            _z = z;
        }

        public bool X
        {
            get { return _x; }
        }

        public bool Y
        {
            get { return _y; }
        }

        public bool Z
        {
            get { return _z; }
        }

        public Vector2B XY
        {
            get { return new Vector2B(X, Y); }
        }

        public bool Equals(Vector3B other)
        {
            return _x == other._x && _y == other._y && _z == other._z;
        }

        public static bool operator ==(Vector3B left, Vector3B right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector3B left, Vector3B right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector3B)
            {
                return Equals((Vector3B)obj);
            }
            return false;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "({0}, {1}, {2})", X, Y, Z);
        }

        public override int GetHashCode()
        {
            return (Convert.ToInt32(_x) * 4) + (Convert.ToInt32(_y) * 2) + Convert.ToInt32(_z);
        }

        public Vector3D ToVector3D()
        {
            return new Vector3D(Convert.ToDouble(_x), Convert.ToDouble(_y), Convert.ToDouble(_z));
        }

        public Vector3S ToVector3S()
        {
            return new Vector3S(Convert.ToSingle(_x), Convert.ToSingle(_y), Convert.ToSingle(_z));
        }

        public Vector3I ToVector3I()
        {
            return new Vector3I(Convert.ToInt32(_x), Convert.ToInt32(_y), Convert.ToInt32(_z));
        }

        public Vector3H ToVector3H()
        {
            return new Vector3H(Convert.ToInt32(_x), Convert.ToInt32(_y), Convert.ToInt32(_z));
        }

        private readonly bool _x;
        private readonly bool _y;
        private readonly bool _z;
    }
}
