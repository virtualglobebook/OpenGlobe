using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace OpenGlobe.Scene.Terrain
{
    public class RasterTerrainLevelCollection : ReadOnlyCollection<RasterTerrainLevel>
    {
        public RasterTerrainLevelCollection(IList<RasterTerrainLevel> collectionToWrap) :
            base(collectionToWrap)
        {
        }
    }
}
