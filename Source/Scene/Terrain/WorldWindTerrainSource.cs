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
using System.Net;
using System.IO;

namespace OpenGlobe.Scene.Terrain
{
    public class WorldWindTerrainSource : RasterTerrainSource
    {
        public WorldWindTerrainSource() :
            this(new Uri("http://www.nasa.network.com/elev?service=WMS&request=GetMap&version=1.3&srs=EPSG:4326&layers=mergedElevations&styles=&format=application/bil16&bgColor=-9999.0&width=150&height=150"))
        {
        }

        public WorldWindTerrainSource(Uri baseUri)
        {
            _baseUri = baseUri;

            _levels = new WorldWindTerrainLevel[NumberOfLevels];
            _levelsCollection = new RasterTerrainLevelCollection(_levels);

            double deltaLongitude = LevelZeroDeltaLongitudeDegrees;
            double deltaLatitude = LevelZeroDeltaLatitudeDegrees;
            for (int i = 0; i < _levels.Length; ++i)
            {
                _levels[i] = new WorldWindTerrainLevel(this, i, deltaLongitude, deltaLatitude);
                deltaLongitude /= 2.0;
                deltaLatitude /= 2.0;
            }
        }

        public override GeodeticExtent Extent
        {
            get { return _extent; }
        }

        public override RasterTerrainLevelCollection Levels
        {
            get { return _levelsCollection; }
        }

        public int TileLongitudePosts
        {
            get { return TileWidth; }
        }

        public int TileLatitudePosts
        {
            get { return TileHeight; }
        }

        private int _tilesLoaded;

        internal short[] DownloadTile(int level, int longitudeIndex, int latitudeIndex)
        {
            string cachePath = level.ToString();
            cachePath = Path.Combine(cachePath, longitudeIndex.ToString());
            string cacheFilename = Path.Combine(cachePath, latitudeIndex.ToString() + ".bil");

            if (!Directory.Exists(cachePath))
            {
                Directory.CreateDirectory(cachePath);
            }

            int heightsToRead = TileWidth * TileHeight;
            short[] result = new short[heightsToRead];

            if (File.Exists(cacheFilename))
            {
                byte[] data = File.ReadAllBytes(cacheFilename);
                if (data.Length == heightsToRead * sizeof(short))
                {
                    int index = 0;
                    for (int i = 0; i < data.Length; i += 2)
                    {
                        result[index] = BitConverter.ToInt16(data, i);
                        ++index;
                    }
                    return result;
                }
            }

            double divisor = Math.Pow(2.0, level);
            double longitudeResolution = LevelZeroDeltaLongitudeDegrees / divisor;
            double latitudeResolution = LevelZeroDeltaLatitudeDegrees / divisor;

            double west = -180.0 + longitudeResolution * longitudeIndex;
            double east = -180.0 + longitudeResolution * (longitudeIndex + 1);
            double south = -90.0 + latitudeResolution * latitudeIndex;
            double north = -90.0 + latitudeResolution * (latitudeIndex + 1);

            StringBuilder query = new StringBuilder(_baseUri.AbsoluteUri);
            query.Append("&bbox=");
            query.Append(west.ToString("0.###########"));
            query.Append(',');
            query.Append(south.ToString("0.###########"));
            query.Append(',');
            query.Append(east.ToString("0.###########"));
            query.Append(',');
            query.Append(north.ToString("0.###########"));
            query.Append('&');

            string queryString = query.ToString();
            ++_tilesLoaded;
            Console.WriteLine("(" + _tilesLoaded + ") Downloading " + queryString);

            WebRequest request = WebRequest.Create(queryString);
            using (WebResponse response = request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (FileStream file = new FileStream(cacheFilename, FileMode.Create, FileAccess.Write))
            {
                int index = 0;

                const int bufferSize = 4096;
                byte[] buffer = new byte[bufferSize];
                int offset = 0;

                int bytesToRead = heightsToRead * 2;

                while (bytesToRead > 0)
                {
                    int bytesRead = stream.Read(buffer, offset, bufferSize - offset);
                    file.Write(buffer, offset, bytesRead);
                    bytesRead += offset;
                    for (int i = 0; i < bytesRead; i += 2)
                    {
                        result[index] = BitConverter.ToInt16(buffer, i);
                        ++index;
                    }
                    offset = bytesRead % 2;
                    if (offset > 0)
                    {
                        bytesRead -= 1;
                        --index;
                        buffer[0] = buffer[bytesRead];
                    }
                    bytesToRead -= bytesRead;
                }

                return result;
            }
        }

        private Uri _baseUri;
        private WorldWindTerrainLevel[] _levels;
        private RasterTerrainLevelCollection _levelsCollection;
        private GeodeticExtent _extent = new GeodeticExtent(-180.0, -90.0, 180.0, 90.0);

        private const int NumberOfLevels = 12;
        private const int TileWidth = 150;
        private const int TileHeight = 150;
        private const double LevelZeroDeltaLongitudeDegrees = 20.0;
        private const double LevelZeroDeltaLatitudeDegrees = 20.0;
    }
}
