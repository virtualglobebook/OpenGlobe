#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

namespace OpenGlobe.Scene.Terrain
{
    public abstract class RasterTerrainTile
    {
        public RasterTerrainTile(RasterTerrainSource terrainSource, RasterTerrainTileIdentifier identifier)
        {
            _terrainSource = terrainSource;
            _identifier = identifier;
        }

        public RasterTerrainSource Source
        {
            get { return _terrainSource; }
        }

        public RasterTerrainLevel Level
        {
            get { return _terrainSource.Levels[_identifier.Level]; }
        }

        public RasterTerrainTileIdentifier Identifier
        {
            get { return _identifier; }
        }

        public abstract RasterTerrainTile SouthwestChild { get; }
        public abstract RasterTerrainTile SoutheastChild { get; }
        public abstract RasterTerrainTile NorthwestChild { get; }
        public abstract RasterTerrainTile NortheastChild { get; }

        /// <summary>
        /// Gets the index in the overall level of the westernmost post in this tile.
        /// </summary>
        public abstract int West { get; }

        /// <summary>
        /// Gets the index in the overall level of the southernmost post in this tile.
        /// </summary>
        public abstract int South { get; }

        /// <summary>
        /// Gets the index in the overall level of the easternmost post in this tile.
        /// </summary>
        public abstract int East { get; }

        /// <summary>
        /// Gets the index in the overall level of the northernmost post in this tile.
        /// </summary>
        public abstract int North { get; }

        /// <summary>
        /// Gets a subset of posts from the tile and copies them into destination array.  This method should
        /// only be called when <see cref="Status"/> is <see cref="RasterTerrainTileStatus.Loaded"/>.
        /// </summary>
        /// <param name="west">The westernmost post to copy.  0 is the westernmost post in the tile.</param>
        /// <param name="south">The southernmost post to copy.  0 is the southernmost post in the tile.</param>
        /// <param name="east">The easternmost post to copy.  0 is the westernmost post in the tile.</param>
        /// <param name="north">The northernmost post to copy.  0 is the southernmost post in the tile.</param>
        /// <param name="destination">The destination array to receive the post data.</param>
        /// <param name="startIndex">The index at which to begin writing; the index in <paramref name="destination"/> to write the southwesternmost requested post.</param>
        /// <param name="stride">The number of posts in a row of the destination array.</param>
        /// <exception cref="InvalidOperationException">
        /// The tile is not loaded.
        /// </exception>
        public void GetPosts(int west, int south, int east, int north, float[] destination, int startIndex, int stride)
        {
            Level.GetPosts(West + west, South + south, West + east, South + north, destination, startIndex, stride);
        }

        private RasterTerrainTileIdentifier _identifier;
        private RasterTerrainSource _terrainSource;
    }
}
