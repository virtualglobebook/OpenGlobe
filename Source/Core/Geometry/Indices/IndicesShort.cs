#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

namespace MiniGlobe.Core.Geometry
{
    public class IndicesShort : Indices<short>
    {
        public IndicesShort()
            : base(IndicesType.Short)
        {
        }

        public IndicesShort(int capacity)
            : base(IndicesType.Short, capacity)
        {
        }
    }
}
