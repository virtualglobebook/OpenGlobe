#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using OpenGlobe.Core;
using System.Threading;

namespace OpenGlobe.Scene
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
            m_activeTiles[tile.Identifier] = tile;
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

            public override RasterTerrainTile SouthwestChild
            {
                get
                {
                    RasterTerrainTileIdentifier current = Identifier;
                    RasterTerrainTileIdentifier child = new RasterTerrainTileIdentifier(current.Level + 1, current.X * 2, current.Y * 2);
                    return Source.GetTile(child);
                }
            }

            public override RasterTerrainTile SoutheastChild
            {
                get
                {
                    RasterTerrainTileIdentifier current = Identifier;
                    RasterTerrainTileIdentifier child = new RasterTerrainTileIdentifier(current.Level + 1, current.X * 2 + 1, current.Y * 2);
                    return Source.GetTile(child);
                }
            }

            public override RasterTerrainTile NorthwestChild
            {
                get
                {
                    RasterTerrainTileIdentifier current = Identifier;
                    RasterTerrainTileIdentifier child = new RasterTerrainTileIdentifier(current.Level + 1, current.X * 2, current.Y * 2 + 1);
                    return Source.GetTile(child);
                }
            }

            public override RasterTerrainTile NortheastChild
            {
                get
                {
                    RasterTerrainTileIdentifier current = Identifier;
                    RasterTerrainTileIdentifier child = new RasterTerrainTileIdentifier(current.Level + 1, current.X * 2 + 1, current.Y * 2 + 1);
                    return Source.GetTile(child);
                }
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
        }
    }
}
