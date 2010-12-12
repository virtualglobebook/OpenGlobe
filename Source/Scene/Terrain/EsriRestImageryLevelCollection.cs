using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace OpenGlobe.Scene
{
    public class EsriRestImageryLevelCollection : ReadOnlyCollection<EsriRestImageryLevel>
    {
        public EsriRestImageryLevelCollection(IList<EsriRestImageryLevel> collectionToWrap) :
            base(collectionToWrap)
        {
        }
    }
}
