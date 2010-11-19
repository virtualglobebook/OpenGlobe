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

        protected /*abstract*/ RasterTerrainTile CreateTile(RasterTerrainTileIdentifier identifier)
        {
            return null;
        }

        private Dictionary<RasterTerrainTileIdentifier, RasterTerrainTile> m_activeTiles = new Dictionary<RasterTerrainTileIdentifier, RasterTerrainTile>();
}
}
