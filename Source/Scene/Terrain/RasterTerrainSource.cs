using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenGlobe.Core;

namespace OpenGlobe.Scene.Terrain
{
    public abstract class RasterTerrainSource
    {
        public abstract GeodeticExtent Extent { get; }
        public abstract RasterTerrainLevelCollection Levels { get; }
    }
}
