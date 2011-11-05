using System.Collections.Generic;
using OpenGlobe.Renderer;
using OpenGlobe.Core;

namespace OpenGlobe.Scene
{
    public abstract class RasterSource
    {
        public abstract RasterLevelCollection Levels { get; }

        public abstract GeodeticExtent Extent { get; }

        public RasterTile GetTile(RasterTileIdentifier identifier)
        {
            RasterTile tile;
            if (m_activeTiles.TryGetValue(identifier, out tile))
            {
                return tile;
            }

            // New tiles are not initially active.  They become active when loaded.
            tile = new RasterTile(this, identifier);
            return tile;
        }

        public void ActivateTile(RasterTile tile)
        {
            m_activeTiles[tile.Identifier] = tile;
        }

        public void DeactivateTile(RasterTile tile)
        {
            m_activeTiles.Remove(tile.Identifier);
        }

        public abstract Texture2D LoadTileTexture(RasterTileIdentifier identifier);

        private readonly Dictionary<RasterTileIdentifier, RasterTile> m_activeTiles = new Dictionary<RasterTileIdentifier, RasterTile>();
    }
}
