#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Collections.Generic;
using OpenGlobe.Core.Geometry;

namespace OpenGlobe.Core
{
    [CLSCompliant(false)]
    public class TriangleMeshSubdivisionResult
    {
        internal TriangleMeshSubdivisionResult(ICollection<Vector3D> positions, IndicesUnsignedInt indices)
        {
            _positions = positions;
            _indices = indices;
        }

        public ICollection<Vector3D> Positions 
        {
            get { return _positions;  }
        }

        public IndicesUnsignedInt Indices 
        {
            get { return _indices;  }
        }

        private readonly ICollection<Vector3D> _positions;
        private readonly IndicesUnsignedInt _indices;
    }
}