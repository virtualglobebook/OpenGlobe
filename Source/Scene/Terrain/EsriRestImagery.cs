using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using OpenGlobe.Core;

namespace OpenGlobe.Scene
{
    public class EsriRestImagery
    {
        public EsriRestImagery() :
            //this(new Uri("http://services.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/"))
            //this(new Uri("http://server.arcgisonline.com/ArcGIS/rest/services/ESRI_Imagery_World_2D/MapServer/tile/"))
            this(new Uri("http://server.arcgisonline.com/ArcGIS/rest/services/I3_Imagery_Prime_World_2D/MapServer/tile/"))
        {
        }

        public EsriRestImagery(Uri baseUri)
        {
            _baseUri = baseUri;

            _levels = new EsriRestImageryLevel[NumberOfLevels];
            _levelsCollection = new EsriRestImageryLevelCollection(_levels);

            double deltaLongitude = LevelZeroDeltaLongitudeDegrees;
            double deltaLatitude = LevelZeroDeltaLatitudeDegrees;
            for (int i = 0; i < _levels.Length; ++i)
            {
                _levels[i] = new EsriRestImageryLevel(this, i, deltaLongitude, deltaLatitude);
                deltaLongitude /= 2.0;
                deltaLatitude /= 2.0;
            }
        }

        public GeodeticExtent Extent
        {
            get { return _extent; }
        }

        public int TileLongitudePosts
        {
            get { return 512; }
        }

        public int TileLatitudePosts
        {
            get { return 512; }
        }

        public EsriRestImageryLevelCollection Levels
        {
            get { return _levelsCollection; }
        }

        internal Bitmap DownloadTile(int level, int longitudeIndex, int latitudeIndex)
        {
            string cachePath = "esri";
            cachePath = Path.Combine(cachePath, level.ToString());
            cachePath = Path.Combine(cachePath, latitudeIndex.ToString());
            string cacheFilename = Path.Combine(cachePath, longitudeIndex.ToString() + ".jpg");

            if (!Directory.Exists(cachePath))
            {
                Directory.CreateDirectory(cachePath);
            }

            //int heightsToRead = TileWidth * TileHeight;
            //short[] result = new short[heightsToRead];

            if (File.Exists(cacheFilename))
            {
                return new Bitmap(cacheFilename);
            }

            //double divisor = Math.Pow(2.0, level);
            //double longitudeResolution = LevelZeroDeltaLongitudeDegrees / divisor;
            //double latitudeResolution = LevelZeroDeltaLatitudeDegrees / divisor;

            // Esri tiles are numbered from the northwest instead of from the southwest.

            StringBuilder query = new StringBuilder(_baseUri.AbsoluteUri);
            query.Append(level);
            query.Append('/');
            query.Append((1 << level) - latitudeIndex - 1);
            query.Append('/');
            query.Append(longitudeIndex);

            string queryString = query.ToString();
            ++_tilesLoaded;
            Console.WriteLine("(" + _tilesLoaded + ") Downloading " + queryString);

            WebRequest request = WebRequest.Create(queryString);
            using (WebResponse response = request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (FileStream file = new FileStream(cacheFilename, FileMode.Create, FileAccess.Write))
            {
                const int bufferSize = 4096;
                byte[] buffer = new byte[bufferSize];

                int bytesRead = stream.Read(buffer, 0, bufferSize);
                while (bytesRead > 0)
                {
                    file.Write(buffer, 0, bytesRead);
                    bytesRead = stream.Read(buffer, 0, bufferSize);
                }
            }

            return new Bitmap(cacheFilename);
        }

        private Uri _baseUri;
        private GeodeticExtent _extent = new GeodeticExtent(-180, -90, 180, 90);
        private int _tilesLoaded;
        private EsriRestImageryLevel[] _levels;
        private EsriRestImageryLevelCollection _levelsCollection;

        private const int NumberOfLevels = 20;
        private const int TileWidth = 150;
        private const int TileHeight = 150;
        private const double LevelZeroDeltaLongitudeDegrees = 180.0;
        private const double LevelZeroDeltaLatitudeDegrees = 180.0;
    }
}
