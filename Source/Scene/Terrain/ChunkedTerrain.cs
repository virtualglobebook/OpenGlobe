using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenGlobe.Scene.Terrain
{
    public class ChunkedTerrain
    {
        public ChunkedTerrain(string chuFilename, string tqtFilename) :
            this(new ChunkedTerrainTree(chuFilename))
        {

        }

        public ChunkedTerrain(ChunkedTerrainTree tree)
        {
            _tree = tree;
        }

        private ChunkedTerrainTree _tree;
    }
}
