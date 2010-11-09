using System;
using System.Collections.Generic;
using System.Text;
using OpenGlobe.Core;

namespace OpenGlobe.Scene.Terrain
{
    public class SimpleTerrainSource : RasterTerrainSource
    {
        public override GeodeticExtent Extent
        {
            get { throw new NotImplementedException(); }
        }

        public override RasterTerrainLevelCollection Levels
        {
            get { throw new NotImplementedException(); }
        }
    }
}
