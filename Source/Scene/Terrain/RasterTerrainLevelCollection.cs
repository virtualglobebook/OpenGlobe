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
using System.Collections.ObjectModel;

namespace OpenGlobe.Scene
{
    public class RasterTerrainLevelCollection : ReadOnlyCollection<RasterTerrainLevel>
    {
        public RasterTerrainLevelCollection(IList<RasterTerrainLevel> collectionToWrap) :
            base(collectionToWrap)
        {
        }
    }
}
