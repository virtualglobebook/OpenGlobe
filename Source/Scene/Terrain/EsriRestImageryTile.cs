#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using OpenGlobe.Scene;

namespace OpenGlobe.Scene
{
    public abstract class EsriRestImageryTile
    {
        public EsriRestImageryTile(EsriRestImagery imagerySource, EsriRestImageryTileIdentifier identifier)
        {
            _imagerySource = imagerySource;
            _identifier = identifier;
        }

        public EsriRestImagery Source
        {
            get { return _imagerySource; }
        }

        public EsriRestImageryLevel Level
        {
            get { return _imagerySource.Levels[_identifier.Level]; }
        }

        public EsriRestImageryTileIdentifier Identifier
        {
            get { return _identifier; }
        }

        public abstract EsriRestImageryTile SouthwestChild { get; }
        public abstract EsriRestImageryTile SoutheastChild { get; }
        public abstract EsriRestImageryTile NorthwestChild { get; }
        public abstract EsriRestImageryTile NortheastChild { get; }

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
        public byte[] GetImage(int west, int south, int east, int north)
        {
            return Level.GetImage(West + west, South + south, West + east, South + north);
        }

        private EsriRestImageryTileIdentifier _identifier;
        private EsriRestImagery _imagerySource;
    }
}
