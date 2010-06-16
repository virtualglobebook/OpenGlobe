#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Globalization;

namespace OpenGlobe.Renderer
{
    public struct ColorMask : IEquatable<ColorMask>
    {
        public ColorMask(bool red, bool green, bool blue, bool alpha)
        {
            _red = red;
            _green = green;
            _blue = blue;
            _alpha = alpha;
        }

        public bool Red
        {
            get { return _red; }
        }

        public bool Green
        {
            get { return _green; }
        }

        public bool Blue
        {
            get { return _blue; }
        }

        public bool Alpha
        {
            get { return _alpha; }
        }

        #region IEquatable Members

        public bool Equals(ColorMask other)
        {
            return 
                _red == other._red && 
                _green == other._green && 
                _blue == other._blue && 
                _alpha == other._alpha;
        }

        #endregion

        public static bool operator ==(ColorMask left, ColorMask right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ColorMask left, ColorMask right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            if (obj is ColorMask)
            {
                return Equals((ColorMask)obj);
            }
            return false;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "(Red: {0}, Green: {1}, Blue: {2}, Alpha: {3})", 
                _red, _green, _blue, _alpha);
        }

        public override int GetHashCode()
        {
            return _red.GetHashCode() ^ _green.GetHashCode() ^ _blue.GetHashCode() ^ _alpha.GetHashCode();
        }

        private readonly bool _red;
        private readonly bool _green;
        private readonly bool _blue;
        private readonly bool _alpha;
    }
}
