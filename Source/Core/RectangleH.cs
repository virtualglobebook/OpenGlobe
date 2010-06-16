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

namespace OpenGlobe.Core
{
    public struct RectangleH : IEquatable<RectangleH>
    {
        public RectangleH(Vector2H lowerLeft, Vector2H upperRight)
        {
            _lowerLeft = lowerLeft;
            _upperRight = upperRight;
        }

        public Vector2H LowerLeft { get { return _lowerLeft; } }
        public Vector2H UpperRight { get { return _upperRight; } }

        public static bool operator ==(RectangleH left, RectangleH right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(RectangleH left, RectangleH right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "({0}, {1})",
                _lowerLeft.ToString(), _upperRight.ToString());
        }

        public override int GetHashCode()
        {
            return _lowerLeft.GetHashCode() ^ _upperRight.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is RectangleH))
                return false;

            return this.Equals((RectangleH)obj);
        }

        #region IEquatable<RectangleH> Members

        public bool Equals(RectangleH other)
        {
            return
                (_lowerLeft == other._lowerLeft) &&
                (_upperRight == other._upperRight);
        }

        #endregion

        private readonly Vector2H _lowerLeft;
        private readonly Vector2H _upperRight;
    }
}
