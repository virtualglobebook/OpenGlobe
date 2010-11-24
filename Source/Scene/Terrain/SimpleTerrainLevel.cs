using System;
using System.Collections.Generic;
using System.Text;
using OpenGlobe.Core;

namespace OpenGlobe.Scene.Terrain
{
    public class SimpleTerrainLevel : RasterTerrainLevel
    {
        public SimpleTerrainLevel(SimpleTerrainSource terrainSource, int level, double tileDeltaLongitude, double tileDeltaLatitude)
        {
            _terrainSource = terrainSource;
            _level = level;
            //_tileDeltaLongitude = tileDeltaLongitude;
            //_tileDeltaLatitude = tileDeltaLatitude;

            GeodeticExtent extent = terrainSource.Extent;
            _longitudeTiles = (int)Math.Round((extent.East - extent.West) / tileDeltaLongitude);
            _latitudeTiles = (int)Math.Round((extent.North - extent.South) / tileDeltaLatitude);

            _longitudePosts = _longitudeTiles * _terrainSource.TileLongitudePosts;
            _latitudePosts = _latitudeTiles * _terrainSource.TileLatitudePosts;

            _postDeltaLongitude = (extent.East - extent.West) / _longitudePosts;
            _postDeltaLatitude = (extent.North - extent.South) / _latitudePosts;
        }

        public override RasterTerrainSource Source
        {
            get { return _terrainSource; }
        }

        public override int LevelID
        {
            get { return _level; }
        }

        public override double PostDeltaLongitude
        {
            get { return _postDeltaLongitude; }
        }

        public override double PostDeltaLatitude
        {
            get { return _postDeltaLatitude; }
        }

        public override int LongitudePosts
        {
            get { return _longitudePosts; }
        }

        public override int LatitudePosts
        {
            get { return _latitudePosts; }
        }

        public override int LongitudeTiles
        {
            get { return _longitudeTiles; }
        }

        public override int LatitudeTiles
        {
            get { return _latitudeTiles; }
        }

        public override void GetPosts(int west, int south, int east, int north, float[] destination, int startIndex, int stride)
        {
            int tileXStart = west / _terrainSource.TileLongitudePosts;
            int tileXStop = east / _terrainSource.TileLongitudePosts;

            int tileYStart = south / _terrainSource.TileLatitudePosts;
            int tileYStop = north / _terrainSource.TileLatitudePosts;

            for (int tileY = tileYStart; tileY <= tileYStop; ++tileY)
            {
                int tileYOrigin = tileY * _terrainSource.TileLatitudePosts;

                int currentSouth = south - tileYOrigin;
                if (currentSouth < 0)
                    currentSouth = 0;

                int currentNorth = north - tileYOrigin;
                if (currentNorth >= _terrainSource.TileLatitudePosts)
                    currentNorth = _terrainSource.TileLatitudePosts - 1;

                for (int tileX = tileXStart; tileX <= tileXStop; ++tileX)
                {
                    int tileXOrigin = tileX * _terrainSource.TileLongitudePosts;

                    int currentWest = west - tileXOrigin;
                    if (currentWest < 0)
                        currentWest = 0;

                    int currentEast = east - tileXOrigin;
                    if (currentEast >= _terrainSource.TileLongitudePosts)
                        currentEast = _terrainSource.TileLongitudePosts - 1;

                    int writeIndex = startIndex + (currentSouth + tileYOrigin - south) * stride + currentWest + tileXOrigin - west;
                    GetTilePosts(tileX, tileY, currentWest, currentSouth, currentEast, currentNorth, destination, writeIndex, stride);
                }
            }
        }

        private void GetTilePosts(int tileLongitudeIndex, int tileLatitudeIndex, int tileWest, int tileSouth, int tileEast, int tileNorth, float[] destination, int startIndex, int stride)
        {
            Tile tile = _cache.Find(candidate => candidate.TileLongitudeIndex == tileLongitudeIndex && candidate.TileLatitudeIndex == tileLatitudeIndex);
            if (tile == null)
            {
                tile = new Tile();
                tile.TileLongitudeIndex = tileLongitudeIndex;
                tile.TileLatitudeIndex = tileLatitudeIndex;

                if (tileLongitudeIndex < 0 || tileLatitudeIndex < 0 ||
                    tileLongitudeIndex >= _longitudeTiles || tileLatitudeIndex >= _latitudeTiles)
                {
                    tile.Posts = new ushort[(_terrainSource.TileLongitudePosts + 1) * (_terrainSource.TileLatitudePosts + 1)];
                }
                else
                {
                    tile.Posts = _terrainSource.GetTile(_level, tileLongitudeIndex, tileLatitudeIndex);
                }
                _cache.Add(tile);
            }

            //int latitudePosts = tileNorth - tileSouth + 1;
            int longitudePosts = tileEast - tileWest + 1;

            int writeIndex = startIndex;
            for (int j = tileSouth; j <= tileNorth; ++j)
            {
                int row = (_terrainSource.TileLatitudePosts - j - 1) * (_terrainSource.TileLongitudePosts + 1);
                for (int i = tileWest; i <= tileEast; ++i)
                {
                    destination[writeIndex] = tile.Posts[row + i];
                    ++writeIndex;
                }
                writeIndex += stride - longitudePosts;
            }
        }

        public override double LongitudeToIndex(double longitude)
        {
            GeodeticExtent extent = _terrainSource.Extent;
            return (longitude - extent.West) / _postDeltaLongitude;
        }

        public override double LatitudeToIndex(double latitude)
        {
            GeodeticExtent extent = _terrainSource.Extent;
            return (latitude - extent.South) / _postDeltaLatitude;
        }

        public override double IndexToLongitude(int longitudeIndex)
        {
            GeodeticExtent extent = _terrainSource.Extent;
            return extent.West + longitudeIndex * _postDeltaLongitude;
        }

        public override double IndexToLatitude(int latitudeIndex)
        {
            GeodeticExtent extent = _terrainSource.Extent;
            return extent.South + latitudeIndex * _postDeltaLatitude;
        }

        private class Tile
        {
            public ushort[] Posts;
            public int TileLongitudeIndex;
            public int TileLatitudeIndex;
        }

        //private double _tileDeltaLongitude;
        //private double _tileDeltaLatitude;
        private int _level;
        private SimpleTerrainSource _terrainSource;
        private int _longitudePosts;
        private int _latitudePosts;
        private double _postDeltaLongitude;
        private double _postDeltaLatitude;
        private List<Tile> _cache = new List<Tile>();
        private int _longitudeTiles;
        private int _latitudeTiles;
    }
}
