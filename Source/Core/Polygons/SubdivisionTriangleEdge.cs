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
    internal class SubdivisionTriangleEdge
    {
        public SubdivisionTriangleEdge(SubdivisionEdge edge, bool flipDirection)
        {
            _edge = edge;
            _flipDirection = flipDirection;
        }

        public SubdivisionEdge Edge
        {
            get { return _edge; }
        }

        public bool FlipDirection
        {
            get { return _flipDirection; }
        }

        private readonly SubdivisionEdge _edge;
        private readonly bool _flipDirection;
    }
}