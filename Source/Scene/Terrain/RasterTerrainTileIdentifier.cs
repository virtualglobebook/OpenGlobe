#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;

namespace OpenGlobe.Scene.Terrain
{
    public struct RasterTerrainTileIdentifier : IEquatable<RasterTerrainTileIdentifier>
    {
        public RasterTerrainTileIdentifier(int level, int x, int y)
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

        public bool Equals(RasterTerrainTileIdentifier other)
        {
            return _level == other._level && _x == other._x && _y == other._y;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is RasterTerrainTileIdentifier))
                return false;
            return Equals((RasterTerrainTileIdentifier)obj);
        }

        public override int GetHashCode()
        {
            return _level.GetHashCode() ^ _x.GetHashCode() ^ _y.GetHashCode();
        }

        private int _x;
        private int _y;
        private int _level;
    }
}
