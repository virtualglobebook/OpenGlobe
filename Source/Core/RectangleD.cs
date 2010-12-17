#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;
using System.Globalization;

namespace OpenGlobe.Core
{
    public struct RectangleD : IEquatable<RectangleD>
    {
        public RectangleD(Vector2D lowerLeft, Vector2D upperRight)
        {
            _lowerLeft = lowerLeft;
            _upperRight = upperRight;
        }

        public Vector2D LowerLeft { get { return _lowerLeft; } }
        public Vector2D UpperRight { get { return _upperRight; } }

        public static bool operator ==(RectangleD left, RectangleD right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(RectangleD left, RectangleD right)
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
            if (!(obj is RectangleD))
                return false;

            return this.Equals((RectangleD)obj);
        }

        #region IEquatable<RectangleD> Members

        public bool Equals(RectangleD other)
        {
            return
                (_lowerLeft == other._lowerLeft) &&
                (_upperRight == other._upperRight);
        }

        #endregion

        private readonly Vector2D _lowerLeft;
        private readonly Vector2D _upperRight;
    }
}
