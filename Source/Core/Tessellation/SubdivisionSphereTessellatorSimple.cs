#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Collections.Generic;
using OpenGlobe.Core.Geometry;

namespace OpenGlobe.Core.Tessellation
{
    public static class SubdivisionSphereTessellatorSimple
    {
        public static Mesh Compute(int numberOfSubdivisions)
        {
            if (numberOfSubdivisions < 0)
            {
                throw new ArgumentOutOfRangeException("numberOfSubdivisions");
            }

            Mesh mesh = new Mesh();
            mesh.PrimitiveType = PrimitiveType.Triangles;
            mesh.FrontFaceWindingOrder = WindingOrder.Counterclockwise;

            VertexAttributeDoubleVector3 positionsAttribute = new VertexAttributeDoubleVector3(
                "position", SubdivisionUtility.NumberOfVertices(numberOfSubdivisions));
            mesh.Attributes.Add(positionsAttribute);

            IndicesInt32 indices = new IndicesInt32(3 * SubdivisionUtility.NumberOfTriangles(numberOfSubdivisions));
            mesh.Indices = indices;

            //
            // Initial tetrahedron
            //
            double negativeRootTwoOverThree = -Math.Sqrt(2.0) / 3.0;
            const double negativeOneThird = -1.0 / 3.0;
            double rootSixOverThree = Math.Sqrt(6.0) / 3.0;

            IList<Vector3D> positions = positionsAttribute.Values;
            positions.Add(new Vector3D(0, 0, 1));
            positions.Add(new Vector3D(0, (2.0 * Math.Sqrt(2.0)) / 3.0, negativeOneThird));
            positions.Add(new Vector3D(-rootSixOverThree, negativeRootTwoOverThree, negativeOneThird));
            positions.Add(new Vector3D(rootSixOverThree, negativeRootTwoOverThree, negativeOneThird));

            Subdivide(positions, indices, new TriangleIndicesInt32(0, 1, 2), numberOfSubdivisions);
            Subdivide(positions, indices, new TriangleIndicesInt32(0, 2, 3), numberOfSubdivisions);
            Subdivide(positions, indices, new TriangleIndicesInt32(0, 3, 1), numberOfSubdivisions);
            Subdivide(positions, indices, new TriangleIndicesInt32(1, 3, 2), numberOfSubdivisions);

            return mesh;
        }

        private static void Subdivide(IList<Vector3D> positions, IndicesInt32 indices, TriangleIndicesInt32 triangle, int level)
        {
            if (level > 0)
            {
                positions.Add(((positions[triangle.I0] + positions[triangle.I1]) * 0.5).Normalize());
                positions.Add(((positions[triangle.I1] + positions[triangle.I2]) * 0.5).Normalize());
                positions.Add(((positions[triangle.I2] + positions[triangle.I0]) * 0.5).Normalize());

                int i01 = positions.Count - 3;
                int i12 = positions.Count - 2;
                int i20 = positions.Count - 1;

                //
                // Subdivide input triangle into four triangles
                //
                --level;
                Subdivide(positions, indices, new TriangleIndicesInt32(triangle.I0, i01, i20), level);
                Subdivide(positions, indices, new TriangleIndicesInt32(i01, triangle.I1, i12), level);
                Subdivide(positions, indices, new TriangleIndicesInt32(i01, i12, i20), level);
                Subdivide(positions, indices, new TriangleIndicesInt32(i20, i12, triangle.I2), level);
            }
            else
            {
                indices.AddTriangle(triangle);
            }
        }
    }
}
