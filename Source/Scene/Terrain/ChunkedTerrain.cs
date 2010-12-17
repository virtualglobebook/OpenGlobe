#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

namespace OpenGlobe.Scene
{
    public class ChunkedTerrain
    {
        public ChunkedTerrain(string chuFilename, string tqtFilename) :
            this(new ChunkedTerrainTree(chuFilename))
        {

        }

        public ChunkedTerrain(ChunkedTerrainTree tree)
        {
            //_tree = tree;
        }

        //private ChunkedTerrainTree _tree;
    }
}
