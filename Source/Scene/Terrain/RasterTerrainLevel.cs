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

        public abstract int LongitudeTiles { get; }
        public abstract int LatitudeTiles { get; }

        public abstract void GetPosts(int west, int south, int east, int north, float[] destination, int startIndex, int stride);

        public abstract double LongitudeToIndex(double centerLongitude);
        public abstract double LatitudeToIndex(double centerLatitude);

        public abstract double IndexToLongitude(int longitudeIndex);
        public abstract double IndexToLatitude(int latitudeIndex);

        public abstract RasterTerrainSource Source { get; }
        public abstract int LevelID { get; }

        internal RasterTerrainTileRegion[] GetTilesInExtent(int west, int south, int east, int north)
        {
            int tileLongitudePosts = LongitudePosts / LongitudeTiles;
            int tileLatitudePosts = LatitudePosts / LatitudeTiles;

            int tileXStart = west / tileLongitudePosts;
            int tileXStop = east / tileLongitudePosts;

            if (west < 0)
            {
                --tileXStart;
            }
            if (east < 0)
            {
                --tileXStop;
            }

            int tileYStart = south / tileLatitudePosts;
            int tileYStop = north / tileLatitudePosts;

            if (south < 0)
            {
                --tileYStart;
            }
            if (north < 0)
            {
                --tileYStop;
            }

            int tileWidth = tileXStop - tileXStart + 1;
            int tileHeight = tileYStop - tileYStart + 1;

            RasterTerrainTileRegion[] result = new RasterTerrainTileRegion[tileWidth * tileHeight];
            int resultIndex = 0;

            for (int tileY = tileYStart; tileY <= tileYStop; ++tileY)
            {
                int tileYOrigin = tileY * tileLatitudePosts;

                int currentSouth = south - tileYOrigin;
                if (currentSouth < 0)
                    currentSouth = 0;

                int currentNorth = north - tileYOrigin;
                if (currentNorth >= tileLatitudePosts)
                    currentNorth = tileLatitudePosts - 1;

                for (int tileX = tileXStart; tileX <= tileXStop; ++tileX)
                {
                    int tileXOrigin = tileX * tileLongitudePosts;

                    int currentWest = west - tileXOrigin;
                    if (currentWest < 0)
                        currentWest = 0;

                    int currentEast = east - tileXOrigin;
                    if (currentEast >= tileLongitudePosts)
                        currentEast = tileLongitudePosts - 1;

                    RasterTerrainTileIdentifier tileID = new RasterTerrainTileIdentifier(LevelID, tileX, tileY);
                    RasterTerrainTile tile = Source.GetTile(tileID);
                    result[resultIndex] = new RasterTerrainTileRegion(tile, currentWest, currentSouth, currentEast, currentNorth);
                    ++resultIndex;
                }
            }

            return result;
        }
    }
}
