using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenGlobe.Scene.Terrain
{
    public abstract class RasterTerrainLevel
    {
        public abstract double PostDeltaLongitude { get; }
        public abstract double PostDeltaLatitude { get; }

        public abstract int LongitudePosts { get; }
        public abstract int LatitudePosts { get; }

        public abstract void GetPosts(int west, int south, int east, int north, short[] destination, int startIndex, int stride);

        public abstract int LongitudeToIndex(double centerLongitude);
        public abstract int LatitudeToIndex(double centerLatitude);

        public abstract double IndexToLongitude(int longitudeIndex);
        public abstract double IndexToLatitude(int latitudeIndex);
    }
}
