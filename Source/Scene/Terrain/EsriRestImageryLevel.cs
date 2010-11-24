using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using OpenGlobe.Core;

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

        public byte[] GetImage(int west, int south, int east, int north)
        {
            int longitudePixels = east - west + 1;
            int latitudePixels = north - south + 1;

            // Align each row on a 4-byte boundary
            int rowStride = longitudePixels * BytesPerPixel;
            rowStride += (4 - rowStride % 4) % 4;
            byte[] buffer = new byte[rowStride * latitudePixels];

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

                    int writeX = currentWest + tileXOrigin - west;
                    int writeY = currentSouth + tileYOrigin - south;
                    int writeIndex = writeY * rowStride + writeX * BytesPerPixel;
                    GetTilePosts(tileX, tileY, currentWest, currentSouth, currentEast, currentNorth, buffer, writeIndex, rowStride);
                }
            }

            return buffer;
        }

        private void GetTilePosts(int tileLongitudeIndex, int tileLatitudeIndex, int tileWest, int tileSouth, int tileEast, int tileNorth, byte[] buffer, int startIndex, int stride)
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

            //int postsIndex = tileSouth * _imagerySource.TileLongitudePosts + tileWest;
            //int latitudePosts = tileNorth - tileSouth + 1;
            int longitudePosts = tileEast - tileWest + 1;

            //Rectangle rectangle = new Rectangle(tileWest, tile.Image.Height - tileNorth - 1, longitudePosts, latitudePosts);
            Rectangle rectangle = new Rectangle(0, 0, tile.Image.Width, tile.Image.Height);
            BitmapData bmpData = tile.Image.LockBits(rectangle, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            IntPtr scan0 = bmpData.Scan0;
            long readPointer = scan0.ToInt64();

            int writeIndex = startIndex;
            for (int j = tileSouth; j <= tileNorth; ++j)
            {
                int row = (_imagerySource.TileLatitudePosts - j - 1) * bmpData.Stride;
                Marshal.Copy(new IntPtr(readPointer + row + tileWest * BytesPerPixel), buffer, writeIndex, longitudePosts * BytesPerPixel);
                writeIndex += stride;
            }

            tile.Image.UnlockBits(bmpData);
        }
        
#if Mercator
        // Esri uses the Bing Maps / Google Earth tiling scheme, which is described here:
        // http://msdn.microsoft.com/en-us/library/bb259689.aspx

        private const double MaximumLatitude = 85.05112878;
        private const double MinimumLatitude = -85.05112878;

        public double LatitudeToIndex(double latitude)
        {
            if (latitude > MaximumLatitude)
                latitude = MaximumLatitude;
            else if (latitude < MinimumLatitude)
                latitude = MinimumLatitude;

            double sinLatitude = Math.Sin(latitude * Math.PI / 180.0);
            double y = 0.5 - Math.Log((1 + sinLatitude) / (1 - sinLatitude)) / (4 * Math.PI);
            int mapSize = 256 << _level;
            return mapSize - y * mapSize - 1;
        }

        public double IndexToLatitude(int latitudeIndex)
        {
            int mapSize = 256 << _level;
            latitudeIndex = mapSize - latitudeIndex - 1;
            double y = latitudeIndex / mapSize;
            return 90.0 - 360.0 * Math.Atan(Math.Exp(-y * 2 * Math.PI)) / Math.PI;
        }
#else
        public double LatitudeToIndex(double latitude)
        {
            GeodeticExtent extent = _imagerySource.Extent;
            return (latitude - extent.South) / _postDeltaLatitude;
        }

        public double IndexToLatitude(int latitudeIndex)
        {
            GeodeticExtent extent = _imagerySource.Extent;
            return extent.South + latitudeIndex * _postDeltaLatitude;
        }
#endif

        public double LongitudeToIndex(double longitude)
        {
            GeodeticExtent extent = _imagerySource.Extent;
            return (longitude - extent.West) / _postDeltaLongitude;
        }

        public double IndexToLongitude(int longitudeIndex)
        {
            GeodeticExtent extent = _imagerySource.Extent;
            return extent.West + longitudeIndex * _postDeltaLongitude;
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

        private const int BytesPerPixel = 3;
    }
}
