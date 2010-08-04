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
    internal class SubdivisionTriangle
    {
        public SubdivisionTriangle(SubdivisionTriangleEdge e0, SubdivisionTriangleEdge e1, SubdivisionTriangleEdge e2)
        {
            _e0 = e0;
            _e1 = e1;
            _e2 = e2;
        }

        public SubdivisionTriangleEdge Edge0
        {
            get { return _e0; }
        }

        public SubdivisionTriangleEdge Edge1
        {
            get { return _e1; }
        }

        public SubdivisionTriangleEdge Edge2
        {
            get { return _e2; }
        }
        
        private readonly SubdivisionTriangleEdge _e0;
        private readonly SubdivisionTriangleEdge _e1;
        private readonly SubdivisionTriangleEdge _e2;
    }
}