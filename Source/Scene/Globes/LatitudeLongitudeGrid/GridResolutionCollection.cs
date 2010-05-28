#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System.Collections.Generic;
using System.Collections.ObjectModel;
using MiniGlobe.Core;

namespace MiniGlobe.Scene
{
    public class GridResolutionCollection : Collection<GridResolution>
    {
        public GridResolutionCollection(IList<GridResolution> list)
            : base(list)
        {
        }
    }
}