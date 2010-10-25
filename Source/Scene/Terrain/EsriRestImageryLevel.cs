using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenGlobe.Core;
using System.Drawing;

namespace OpenGlobe.Scene.Terrain
{
    public class EsriRestImageryLevel
    {
        public EsriRestImageryLevel(EsriRestImagery imagerySource, int level, double tileDeltaLongitude, double tileDeltaLatitude)
        {
            _imagerySource = imagerySource;
            _level = level;
            _tileDeltaLongitude = tileDeltaLongitude;
            _tileDeltaLatitude = tileDeltaLatitude;
            _longitudePosts = (int)Math.Round(360.0 / tileDeltaLongitude) * imagerySource.TileLongitudePosts;
            _latitudePosts = (int)Math.Round(180.0 / tileDeltaLatitude) * imagerySource.TileLatitudePosts;

            GeodeticExtent extent = imagerySource.Extent;
            _postDeltaLongitude = (extent.East - extent.West) / _longitudePosts;
            _postDeltaLatitude = (extent.North - extent.South) / _latitudePosts;
        }

        public double TileDeltaLongitude
        {
            get { return _tileDeltaLongitude; }
        }

        public double TileDeltaLatitude
        {
            get { return _tileDeltaLatitude; }
        }

        public double PostDeltaLongitude
        {
            get { return _postDeltaLongitude; }
        }

        public double PostDeltaLatitude
        {
            get { return _postDeltaLatitude; }
        }

        public int LongitudePosts
        {
            get { return _longitudePosts; }
        }

        public int LatitudePosts
        {
            get { return _latitudePosts; }
        }

        public void GetImage(int west, int south, int east, int north, Bitmap destination, int startX, int startY)
        {
            using (Graphics g = Graphics.FromImage(destination))
            {
                g.SetClip(new Rectangle(startX, startY, destination.Width - startX, destination.Height - startY));

                int tileXStart = west / _imagerySource.TileLongitudePosts;
                int tileXStop = east / _imagerySource.TileLongitudePosts;

                int tileYStart = south / _imagerySource.TileLatitudePosts;
                int tileYStop = north / _imagerySource.TileLatitudePosts;

                for (int tileY = tileYStart; tileY <= tileYStop; ++tileY)
                {
                    int tileYOrigin = tileY * _imagerySource.TileLatitudePosts;

                    int currentSouth = south - tileYOrigin;
                    if (currentSouth < 0)
                        currentSouth = 0;

                    int currentNorth = north - tileYOrigin;
                    if (currentNorth >= _imagerySource.TileLatitudePosts)
                        currentNorth = _imagerySource.TileLatitudePosts - 1;

                    for (int tileX = tileXStart; tileX <= tileXStop; ++tileX)
                    {
                        int tileXOrigin = tileX * _imagerySource.TileLongitudePosts;

                        int currentWest = west - tileXOrigin;
                        if (currentWest < 0)
                            currentWest = 0;

                        int currentEast = east - tileXOrigin;
                        if (currentEast >= _imagerySource.TileLongitudePosts)
                            currentEast = _imagerySource.TileLongitudePosts - 1;

                        int writeX = startX + currentWest + tileXOrigin - west;
                        int writeY = startY + currentSouth + tileYOrigin - south;
                        GetTilePosts(tileX, tileY, currentWest, currentSouth, currentEast, currentNorth, g, writeX, writeY);
                    }
                }
            }
        }

        private void GetTilePosts(int tileLongitudeIndex, int tileLatitudeIndex, int tileWest, int tileSouth, int tileEast, int tileNorth, Graphics destination, int startX, int startY)
        {
            Tile tile = _cache.Find(candidate => candidate.TileLongitudeIndex == tileLongitudeIndex && candidate.TileLatitudeIndex == tileLatitudeIndex);
            if (tile == null)
            {
                tile = new Tile();
                tile.TileLongitudeIndex = tileLongitudeIndex;
                tile.TileLatitudeIndex = tileLatitudeIndex;
                tile.Image = _imagerySource.DownloadTile(_level, tileLongitudeIndex, tileLatitudeIndex);
                _cache.Add(tile);
            }

            destination.DrawImageUnscaled(tile.Image, startX, startY);
        }

        public double LongitudeToIndex(double longitude)
        {
            GeodeticExtent extent = _imagerySource.Extent;
            return (longitude - extent.West) / _postDeltaLongitude;
        }

        public double LatitudeToIndex(double latitude)
        {
            GeodeticExtent extent = _imagerySource.Extent;
            return (latitude - extent.South) / _postDeltaLatitude;
        }

        public double IndexToLongitude(int longitudeIndex)
        {
            GeodeticExtent extent = _imagerySource.Extent;
            return extent.West + longitudeIndex * _postDeltaLongitude;
        }

        public double IndexToLatitude(int latitudeIndex)
        {
            GeodeticExtent extent = _imagerySource.Extent;
            return extent.South + latitudeIndex * _postDeltaLatitude;
        }

        private class Tile
        {
            public Bitmap Image;
            public int TileLongitudeIndex;
            public int TileLatitudeIndex;
        }

        private EsriRestImagery _imagerySource;
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
