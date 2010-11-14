using System;
using System.Collections.Generic;
using System.Text;
using OpenGlobe.Core;
using System.IO;

namespace OpenGlobe.Scene.Terrain
{
    public class SimpleTerrainSource : RasterTerrainSource
    {
        public SimpleTerrainSource(string path)
        {
            _path = path;

            string[] subdirs = Directory.GetDirectories(path);
            List<int> levels = new List<int>();

            foreach (string subdir in subdirs)
            {
                int level;
                if (Int32.TryParse(Path.GetFileName(subdir), out level))
                {
                    levels.Add(level);
                }
            }

            levels.Sort();

            if (levels.Count == 0)
            {
                throw new ArgumentException("Simple terrain directory does not contain any level subdirectories.", "path");
            }

            if (levels[0] != 0)
            {
                throw new ArgumentException("The first level subdirectory is not level 0.", "path");
            }

            if (levels.Count > 1 && levels[levels.Count - 1] != levels.Count - 1)
            {
                throw new ArgumentException("One or more level subdirectories is missing.", "path");
            }

            _levels = new SimpleTerrainLevel[levels.Count];
            _levelsCollection = new RasterTerrainLevelCollection(_levels);

            double deltaLongitude = LevelZeroDeltaLongitudeDegrees;
            double deltaLatitude = LevelZeroDeltaLatitudeDegrees;
            for (int i = 0; i < _levels.Length; ++i)
            {
                _levels[i] = new SimpleTerrainLevel(this, i, deltaLongitude, deltaLatitude);
                deltaLongitude /= 2.0;
                deltaLatitude /= 2.0;
            }
        }

        public int TileLongitudePosts
        {
            get { return TileWidth; }
        }

        public int TileLatitudePosts
        {
            get { return TileHeight; }
        }

        public override GeodeticExtent Extent
        {
            get { return _extent; }
        }

        public override RasterTerrainLevelCollection Levels
        {
            get { return _levelsCollection; }
        }

        internal ushort[] GetTile(int level, int tileLongitudeIndex, int tileLatitudeIndex)
        {
            tileLatitudeIndex = (1 << level) - tileLatitudeIndex - 1;

            string path = Path.Combine(_path, level.ToString());
            path = Path.Combine(path, tileLatitudeIndex.ToString());
            path = Path.Combine(path, tileLongitudeIndex.ToString() + ".bil");

            byte[] bytes = File.ReadAllBytes(path);
            ushort[] result = new ushort[bytes.Length / 2];
            for (int i = 0; i < bytes.Length / 2; ++i)
            {
                result[i] = BitConverter.ToUInt16(bytes, i * 2);
            }

            return result;
        }

        private const int TileWidth = 512;
        private const int TileHeight = 512;
        private const int TileXPosts = 513;
        private const int TileYPosts = 513;
        private const double LevelZeroDeltaLongitudeDegrees = 16384.0;
        private const double LevelZeroDeltaLatitudeDegrees = 16384.0;

        private string _path;
        private SimpleTerrainLevel[] _levels;
        private RasterTerrainLevelCollection _levelsCollection;
        private GeodeticExtent _extent = new GeodeticExtent(-16384.0 / 2.0, -16384.0 / 2.0, 16384.0 / 2.0, 16384.0 / 2.0);
    }
}
