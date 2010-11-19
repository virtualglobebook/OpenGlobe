using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenGlobe.Scene.Terrain
{
    public abstract class RasterTerrainTile
    {
        public RasterTerrainTile(RasterTerrainLevel level, RasterTerrainTileIdentifier identifier)
        {
            _level = level;
            _identifier = identifier;
        }

        public RasterTerrainLevel Level
        {
            get { return _level; }
        }

        public RasterTerrainTileIdentifier Identifier
        {
            get { return _identifier; }
        }

        public abstract RasterTerrainTileStatus Status { get; }

        public abstract RasterTerrainTile SouthwestChild { get; }
        public abstract RasterTerrainTile SoutheastChild { get; }
        public abstract RasterTerrainTile NorthwestChild { get; }
        public abstract RasterTerrainTile NortheastChild { get; }

        /// <summary>
        /// Loads the tile into system memory, reading it from disk or a network server as necessary.
        /// This method does not return until the tile is loaded.
        /// </summary>
        public abstract void Load();

        /// <summary>
        /// Gets a subset of posts from the tile and copies them into destination array.  This method should
        /// only be called when <see cref="Status"/> is <see cref="RasterTerrainTileStatus.Loaded"/>.
        /// </summary>
        /// <param name="west">The westmost post to copy.</param>
        /// <param name="south">The southernmost post to copy.</param>
        /// <param name="east">The easternmost post to copy.</param>
        /// <param name="north">The northernmost post to copy.</param>
        /// <param name="destination">The destination array to receive the post data.</param>
        /// <param name="stride">The number of posts in a row of the destination array.</param>
        /// <exception cref="InvalidOperationException">
        /// The tile is not loaded.
        /// </exception>
        public abstract void GetPosts(int west, int south, int east, int north, float[] destination, int stride);

        private RasterTerrainTileIdentifier _identifier;
        private RasterTerrainLevel _level;
    }
}
