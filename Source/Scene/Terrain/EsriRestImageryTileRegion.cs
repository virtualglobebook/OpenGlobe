#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using OpenGlobe.Scene;

namespace OpenGlobe.Scene
{
    internal class EsriRestImageryTileRegion
    {
        public EsriRestImageryTileRegion(EsriRestImageryTile tile, int west, int south, int east, int north)
        {
            _tile = tile;
            _west = west;
            _south = south;
            _east = east;
            _north = north;
        }

        public EsriRestImageryTile Tile
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

        private EsriRestImageryTile _tile;
        private int _west;
        private int _south;
        private int _east;
        private int _north;
    }
}
