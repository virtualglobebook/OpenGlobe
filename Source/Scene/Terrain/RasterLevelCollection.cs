#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace OpenGlobe.Scene
{
    public class RasterLevelCollection : ReadOnlyCollection<RasterLevel>
    {
        public RasterLevelCollection(IList<RasterLevel> collectionToWrap) :
            base(collectionToWrap)
        {
        }
    }
}
