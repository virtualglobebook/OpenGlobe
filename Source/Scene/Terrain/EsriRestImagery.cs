using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using OpenGlobe.Core;
using OpenGlobe.Scene;
using System.Collections.Generic;
using OpenGlobe.Renderer;
using System.Drawing.Imaging;

namespace OpenGlobe.Scene
{
    public class EsriRestImagery : RasterSource
    {
        public EsriRestImagery() :
            this(new Uri("https://wi.maptiles.arcgis.com/arcgis/rest/services/World_Imagery/MapServer/tile/"))
        {
        }

        public EsriRestImagery(Uri baseUri)
        {
            _baseUri = baseUri;

            _levels = new RasterLevel[NumberOfLevels];
            _levelsCollection = new RasterLevelCollection(_levels);

            double deltaLongitude = LevelZeroDeltaLongitudeDegrees;
            double deltaLatitude = LevelZeroDeltaLatitudeDegrees;
            for (int i = 0; i < _levels.Length; ++i)
            {
                int longitudePosts = (int)Math.Round(360.0 / deltaLongitude) * TileLongitudePosts + 1;
                int latitudePosts = (int)Math.Round(180.0 / deltaLatitude) * TileLatitudePosts + 1;
                _levels[i] = new RasterLevel(this, i, _extent, longitudePosts, latitudePosts, TileLongitudePosts, TileLatitudePosts);
                deltaLongitude /= 2.0;
                deltaLatitude /= 2.0;
            }
        }

        public override GeodeticExtent Extent
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

        public override RasterLevelCollection Levels
        {
            get { return _levelsCollection; }
        }

        public override Texture2D LoadTileTexture(RasterTileIdentifier identifier)
        {
            int level = identifier.Level;
            int longitudeIndex = identifier.X;
            int latitudeIndex = identifier.Y;

            string cachePath = "esri";
            cachePath = Path.Combine(cachePath, level.ToString());
            cachePath = Path.Combine(cachePath, latitudeIndex.ToString());
            string cacheFilename = Path.Combine(cachePath, longitudeIndex.ToString() + ".jpg");

            if (!Directory.Exists(cachePath))
            {
                Directory.CreateDirectory(cachePath);
            }

            if (!File.Exists(cacheFilename))
            {
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
            }

            Bitmap bitmap = new Bitmap(cacheFilename);
            return Device.CreateTexture2DRectangle(bitmap, TextureFormat.RedGreenBlue8);
        }

        private Uri _baseUri;
        private GeodeticExtent _extent = new GeodeticExtent(-180, -90, 180, 90);
        private int _tilesLoaded;
        private RasterLevel[] _levels;
        private RasterLevelCollection _levelsCollection;

        private const int NumberOfLevels = 16;
        private const double LevelZeroDeltaLongitudeDegrees = 180.0;
        private const double LevelZeroDeltaLatitudeDegrees = 180.0;
    }
}
