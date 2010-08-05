#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

namespace OpenGlobe.Core
{
    internal class SubdivisionEdge
    {
        public SubdivisionEdge(Edge edge)
        {
            _edge = edge;
        }

        public Edge Edge { get { return _edge; } }

        public SubdivisionEdge LeftSplit { get; set; }
        public SubdivisionEdge RightSplit { get; set; }

        public bool IsSplit
        {
            get { return (LeftSplit != null) && (RightSplit != null); }
        }

        private readonly Edge _edge;
    }
}