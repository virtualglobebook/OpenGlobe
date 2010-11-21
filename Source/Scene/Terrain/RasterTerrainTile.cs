using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public bool IsLoaded
        {
            get { return true; }
        }

        public abstract RasterTerrainTileStatus Status { get; }

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
        /// Loads the tile into system memory, reading it from disk or a network server as necessary.
        /// This method does not return until the tile is loaded.
        /// </summary>
        public abstract void Load();

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
        public abstract void GetPosts(int west, int south, int east, int north, float[] destination, int startIndex, int stride);

        private RasterTerrainTileIdentifier _identifier;
        private RasterTerrainSource _terrainSource;
    }
}
