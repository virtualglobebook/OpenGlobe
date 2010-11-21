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
using OpenGlobe.Core;

namespace OpenGlobe.Scene.Terrain
{
    public abstract class RasterTerrainSource
    {
        public abstract GeodeticExtent Extent { get; }
        public abstract RasterTerrainLevelCollection Levels { get; }

        public RasterTerrainTile GetTile(RasterTerrainTileIdentifier identifier)
        {
            RasterTerrainTile tile;
            if (m_activeTiles.TryGetValue(identifier, out tile))
            {
                return tile;
            }

            // New tiles are not initially active.  They become active when loaded.
            tile = CreateTile(identifier);
            return tile;
        }

        public void ActivateTile(RasterTerrainTile tile)
        {
            m_activeTiles.Add(tile.Identifier, tile);
        }

        public void DeactivateTile(RasterTerrainTile tile)
        {
            m_activeTiles.Remove(tile.Identifier);
        }

        protected virtual RasterTerrainTile CreateTile(RasterTerrainTileIdentifier identifier)
        {
            return new DefaultRasterTerrainTile(this, identifier);
        }

        private Dictionary<RasterTerrainTileIdentifier, RasterTerrainTile> m_activeTiles = new Dictionary<RasterTerrainTileIdentifier, RasterTerrainTile>();

        private class DefaultRasterTerrainTile : RasterTerrainTile
        {
            public DefaultRasterTerrainTile(RasterTerrainSource terrainSource, RasterTerrainTileIdentifier identifier) :
                base(terrainSource, identifier)
            {
            }

            public override RasterTerrainTileStatus Status
            {
                get { throw new NotImplementedException(); }
            }

            public override RasterTerrainTile SouthwestChild
            {
                get { throw new NotImplementedException(); }
            }

            public override RasterTerrainTile SoutheastChild
            {
                get { throw new NotImplementedException(); }
            }

            public override RasterTerrainTile NorthwestChild
            {
                get { throw new NotImplementedException(); }
            }

            public override RasterTerrainTile NortheastChild
            {
                get { throw new NotImplementedException(); }
            }

            public override int West
            {
                get { return (Level.LongitudePosts / Level.LongitudeTiles) * Identifier.X; }
            }

            public override int South
            {
                get { return (Level.LatitudePosts / Level.LatitudeTiles) * Identifier.Y; }
            }

            public override int East
            {
                get { return (Level.LongitudePosts / Level.LongitudeTiles) * (Identifier.X + 1) - 1; }
            }

            public override int North
            {
                get { return (Level.LatitudePosts / Level.LatitudeTiles) * (Identifier.Y + 1) - 1; }
            }

            public override void Load()
            {
                throw new NotImplementedException();
            }

            public override void GetPosts(int west, int south, int east, int north, float[] destination, int startIndex, int stride)
            {
                Level.GetPosts(West + west, South + south, West + east, South + north, destination, startIndex, stride);
            }
        }
    }
}
