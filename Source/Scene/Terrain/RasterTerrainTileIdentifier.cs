using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
