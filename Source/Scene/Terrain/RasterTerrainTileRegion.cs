#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

namespace OpenGlobe.Scene
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
