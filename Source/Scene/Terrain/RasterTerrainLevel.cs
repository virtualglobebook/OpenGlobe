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

namespace OpenGlobe.Scene.Terrain
{
    public abstract class RasterTerrainLevel
    {
        public abstract double PostDeltaLongitude { get; }
        public abstract double PostDeltaLatitude { get; }

        public abstract int LongitudePosts { get; }
        public abstract int LatitudePosts { get; }

        public abstract void GetPosts(int west, int south, int east, int north, short[] destination, int startIndex, int stride);

        public abstract double LongitudeToIndex(double centerLongitude);
        public abstract double LatitudeToIndex(double centerLatitude);

        public abstract double IndexToLongitude(int longitudeIndex);
        public abstract double IndexToLatitude(int latitudeIndex);
    }
}
