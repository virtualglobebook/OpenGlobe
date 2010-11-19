using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenGlobe.Scene.Terrain
{
    internal class RasterTerrainTileRegion
    {
        public RasterTerrainTileRegion(RasterTerrainTile tile, int west, int south, int east, int north)
        {
            _tile = tile;
            _west = west;
            _south = south;
            _east = east;
            _north = north;
        }

        public RasterTerrainTile Tile
        {
            get { return _tile; }
        }

        public int West
        {
            get { return _west; }
        }

        public int South
        {
            get { return _south; }
        }

        public int East
        {
            get { return _east; }
        }

        public int North
        {
            get { return _north; }
        }

        private RasterTerrainTile _tile;
        private int _west;
        private int _south;
        private int _east;
        private int _north;
    }
}
