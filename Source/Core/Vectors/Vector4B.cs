#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
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
    /// A set of four booleans.
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector4B : IEquatable<Vector4B>
    {
        public static Vector4B False
        {
            get { return new Vector4B(false, false, false, false); }
        }

        public static Vector4B True
        {
            get { return new Vector4B(true, true, true, true); }
        }
        
        public Vector4B(bool x, bool y, bool z, bool w)
        {
            _x = x;
            _y = y;
            _z = z;
            _w = w;
        }

        public Vector4B(Vector3B v, bool w)
        {
            _x = v.X;
            _y = v.Y;
            _z = v.Z;
            _w = w;
        }

        public Vector4B(Vector2B v, bool z, bool w)
        {
            _x = v.X;
            _y = v.Y;
            _z = z;
            _w = w;
        }

        public Vector4B(bool value)
        {
            _x = value;
            _y = value;
            _z = value;
            _w = value;
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

        public bool W
        {
            get { return _w; }
        }

        public Vector2B XY
        {
            get { return new Vector2B(X, Y); }
        }

        public Vector3B XYZ
        {
            get { return new Vector3B(X, Y, Z); }
        }

        public bool Equals(Vector4B other)
        {
            return _x == other._x && _y == other._y && _z == other._z && _w == other._w;
        }

        public static bool operator ==(Vector4B left, Vector4B right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector4B left, Vector4B right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector4B)
            {
                return Equals((Vector4B)obj);
            }
            return false;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "({0}, {1}, {2}, {3})", X, Y, Z, W);
        }

        public override int GetHashCode()
        {
            return (Convert.ToInt32(_x) * 8) + (Convert.ToInt32(_y) * 4) + (Convert.ToInt32(_z) * 2) + Convert.ToInt32(_w);
        }

        public Vector4D ToVector4D()
        {
            return new Vector4D(Convert.ToDouble(_x), Convert.ToDouble(_y), Convert.ToDouble(_z), Convert.ToDouble(_w));
        }

        public Vector4F ToVector4F()
        {
            return new Vector4F(Convert.ToSingle(_x), Convert.ToSingle(_y), Convert.ToSingle(_z), Convert.ToSingle(_w));
        }

        public Vector4I ToVector4I()
        {
            return new Vector4I(Convert.ToInt32(_x), Convert.ToInt32(_y), Convert.ToInt32(_z), Convert.ToInt32(_w));
        }

        public Vector4H ToVector4H()
        {
            return new Vector4H(Convert.ToInt32(_x), Convert.ToInt32(_y), Convert.ToInt32(_z), Convert.ToInt32(_w));
        }

        private readonly bool _x;
        private readonly bool _y;
        private readonly bool _z;
        private readonly bool _w;
    }
}
