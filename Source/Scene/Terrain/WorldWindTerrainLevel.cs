#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using OpenGlobe.Core;
using System.Drawing;

namespace OpenGlobe.Scene.Terrain
{
    public class WorldWindTerrainLevel : RasterTerrainLevel
    {
        public WorldWindTerrainLevel(WorldWindTerrainSource terrainSource, int level, double tileDeltaLongitude, double tileDeltaLatitude)
        {
            _terrainSource = terrainSource;
            _level = level;
            _tileDeltaLongitude = tileDeltaLongitude;
            _tileDeltaLatitude = tileDeltaLatitude;
            _longitudePosts = (int)Math.Round(360.0 / tileDeltaLongitude) * _terrainSource.TileLongitudePosts;
            _latitudePosts = (int)Math.Round(180.0 / tileDeltaLatitude) * _terrainSource.TileLatitudePosts;

            GeodeticExtent extent = terrainSource.Extent;
            _postDeltaLongitude = (extent.East - extent.West) / _longitudePosts;
            _postDeltaLatitude = (extent.North - extent.South) / _latitudePosts;
        }

        public override RasterTerrainSource Source
        {
            get { return _terrainSource; }
        }

        public double TileDeltaLongitude
        {
            get { return _tileDeltaLongitude; }
        }

        public double TileDeltaLatitude
        {
            get { return _tileDeltaLatitude; }
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
                tile.Posts = _terrainSource.DownloadTile(_level, tileLongitudeIndex, tileLatitudeIndex);
                _cache.Add(tile);
            }

            int latitudePosts = tileNorth - tileSouth + 1;
            int longitudePosts = tileEast - tileWest + 1;

            int writeIndex = startIndex;
            for (int j = tileSouth; j <= tileNorth; ++j)
            {
                int row = (_terrainSource.TileLatitudePosts - j - 1) * _terrainSource.TileLongitudePosts;
                for (int i = tileWest; i <= tileEast; ++i)
                {
                    destination[writeIndex] = tile.Posts[row + i];
                    ++writeIndex;
                }
                writeIndex += stride - longitudePosts;
            }
        }

        // World Wind tiles do NOT have overlapping posts at their edges.  Yet, the bounding boxes DO overlap.
        // Typically, this would imply that the posts specify the height at the CENTER of each cell while the
        // bounding box describes the EDGES of the cells.  However, in a multi-resolution scheme like this,
        // that would result in the posts at a coarser resolution not being coincident with the posts
        // at a finer resolution, which would be incredibly inconvenient for rendering (and most other
        // purposes).  So instead we assume that the first post is exactly on the western and southern
        // edges of the bounding box, and that the eastern and northern coordinates actually specify
        // the location of the next post AFTER the last past in the tile.

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
            public short[] Posts;
            public int TileLongitudeIndex;
            public int TileLatitudeIndex;
        }

        private WorldWindTerrainSource _terrainSource;
        private int _level;
        private double _tileDeltaLongitude;
        private double _tileDeltaLatitude;
        private double _postDeltaLongitude;
        private double _postDeltaLatitude;
        private int _longitudePosts;
        private int _latitudePosts;
        private List<Tile> _cache = new List<Tile>();
    }
}
