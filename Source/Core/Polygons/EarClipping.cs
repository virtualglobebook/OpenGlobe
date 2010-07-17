#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System.Collections.Generic;
using System;

namespace OpenGlobe.Core
{
    public static class EarClipping
    {
        public static IList<Vector2D> Triangulate(IEnumerable<Vector2D> positions)
        {
            int count = SimplePolygonAlgorithms.PolygonCount(positions);

            return null;
        }
   }
}