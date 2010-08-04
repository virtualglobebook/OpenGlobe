#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System.Collections.Generic;
using OpenGlobe.Core.Geometry;

namespace OpenGlobe.Core
{
    public class TriangleMeshSubdivisionResult
    {
        internal TriangleMeshSubdivisionResult(IEnumerable<Vector3D> positions, IndicesInt32 indices)
        {
            _positions = Positions;
            _indices = indices;
        }

        public IEnumerable<Vector3D> Positions 
        {
            get { return _positions;  }
        }

        public IndicesInt32 Indices 
        {
            get { return _indices;  }
        }

        private readonly IEnumerable<Vector3D> _positions;
        private readonly IndicesInt32 _indices;
    }
}