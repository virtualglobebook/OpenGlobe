using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace OpenGlobe.Scene.Terrain
{
    public class EsriRestImageryLevelCollection : ReadOnlyCollection<EsriRestImageryLevel>
    {
        public EsriRestImageryLevelCollection(IList<EsriRestImageryLevel> collectionToWrap) :
            base(collectionToWrap)
        {
        }
    }
}
