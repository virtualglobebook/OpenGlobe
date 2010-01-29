#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using OpenTK;

namespace MiniGlobe.Core.Geometry
{
    public class IndicesInt : Indices<int>
    {
        public IndicesInt()
            : base(IndicesType.Int)
        {
        }

        public IndicesInt(int capacity)
            : base(IndicesType.Int, capacity)
        {
        }
    }
}
