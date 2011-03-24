#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;

namespace OpenGlobe.Scene
{
    public struct EsriRestImageryTileIdentifier : IEquatable<EsriRestImageryTileIdentifier>
    {
        public EsriRestImageryTileIdentifier(int level, int x, int y)
        {
            _level = level;
            _x = x;
            _y = y;
        }

        public int Level
        {
            get { return _level; }
        }

        public int X
        {
            get { return _x; }
        }

        public int Y
        {
            get { return _y; }
        }

        public bool Equals(EsriRestImageryTileIdentifier other)
        {
            return _level == other._level && _x == other._x && _y == other._y;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is EsriRestImageryTileIdentifier))
                return false;
            return Equals((EsriRestImageryTileIdentifier)obj);
        }

        public override int GetHashCode()
        {
            return _level.GetHashCode() ^ _x.GetHashCode() ^ _y.GetHashCode();
        }

        public override string ToString()
        {
            return "Level: " + _level + " X: " + _x + " Y: " + _y;
        }


        private int _x;
        private int _y;
        private int _level;
    }
}
