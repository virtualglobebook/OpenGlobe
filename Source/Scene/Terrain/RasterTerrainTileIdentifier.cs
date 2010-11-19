using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenGlobe.Scene.Terrain
{
    public struct RasterTerrainTileIdentifier : IEquatable<RasterTerrainTileIdentifier>
    {
        public RasterTerrainTileIdentifier(int x, int y)
        {
            _x = x;
            _y = y;
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
            return _x == other._x && _y == other._y;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is RasterTerrainTileIdentifier))
                return false;
            return Equals((RasterTerrainTileIdentifier)obj);
        }

        public override int GetHashCode()
        {
            return _x.GetHashCode() ^ _y.GetHashCode();
        }

        private int _x;
        private int _y;
    }
}
