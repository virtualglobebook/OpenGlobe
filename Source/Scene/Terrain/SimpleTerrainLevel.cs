using System;
using System.Collections.Generic;
using System.Text;

namespace OpenGlobe.Scene.Terrain
{
    public class SimpleTerrainLevel : RasterTerrainLevel
    {
        public override double PostDeltaLongitude
        {
            get { throw new NotImplementedException(); }
        }

        public override double PostDeltaLatitude
        {
            get { throw new NotImplementedException(); }
        }

        public override int LongitudePosts
        {
            get { throw new NotImplementedException(); }
        }

        public override int LatitudePosts
        {
            get { throw new NotImplementedException(); }
        }

        public override void GetPosts(int west, int south, int east, int north, float[] destination, int startIndex, int stride)
        {
            throw new NotImplementedException();
        }

        public override double LongitudeToIndex(double centerLongitude)
        {
            throw new NotImplementedException();
        }

        public override double LatitudeToIndex(double centerLatitude)
        {
            throw new NotImplementedException();
        }

        public override double IndexToLongitude(int longitudeIndex)
        {
            throw new NotImplementedException();
        }

        public override double IndexToLatitude(int latitudeIndex)
        {
            throw new NotImplementedException();
        }
    }
}
