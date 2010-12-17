#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;
using System.Collections.Generic;

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