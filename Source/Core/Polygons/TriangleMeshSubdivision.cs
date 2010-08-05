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
    public static class TriangleMeshSubdivision
    {
        public static TriangleMeshSubdivisionResult Compute(IEnumerable<Vector3D> positions, IndicesInt32 indices)
        {
            // TODO
            double granularity = Trig.ToRadians(1);
            
            if (positions == null)
            {
                throw new ArgumentNullException("positions");
            }

            if (indices == null)
            {
                throw new ArgumentNullException("positions");
            }

            if (indices.Values.Count < 3)
            {
                throw new ArgumentOutOfRangeException("indices", "At least three indices are required.");
            }

            if (indices.Values.Count % 3 != 0)
            {
                throw new ArgumentException("indices", "The number of indices must be divisable by three.");
            }

            //
            // Use two queues:  one for triangles that need (or might need) to be 
            // subdivided and other for triangles that are fully subdivided.
            //
            Queue<TriangleIndicesInt32> triangles = new Queue<TriangleIndicesInt32>(indices.Values.Count / 3);
            Queue<TriangleIndicesInt32> done = new Queue<TriangleIndicesInt32>(indices.Values.Count / 3);

            IList<int> indicesValues = indices.Values;
            for (int i = 0; i < indicesValues.Count; i += 3)
            {
                triangles.Enqueue(new TriangleIndicesInt32(indicesValues[i], indicesValues[i + 1], indicesValues[i + 2]));
            }

            //
            // New positions due to edge splits are appended to the positions list.
            //
            IList<Vector3D> subdividedPositions = CollectionAlgorithms.CopyEnumerableToList(positions);

            //
            // Subdivide triangles until we run out
            //
            while (triangles.Count != 0)
            {
                TriangleIndicesInt32 triangle = triangles.Dequeue();

                Vector3D v0 = subdividedPositions[triangle.I0];
                Vector3D v1 = subdividedPositions[triangle.I1];
                Vector3D v2 = subdividedPositions[triangle.I2];

                Vector3D nv0 = v0.Normalize();
                Vector3D nv1 = v1.Normalize();
                Vector3D nv2 = v2.Normalize();

                double d0 = Math.Acos(nv0.Dot(nv1));
                double d1 = Math.Acos(nv1.Dot(nv2));
                double d2 = Math.Acos(nv2.Dot(nv0));
             
                double max = Math.Max(d0, Math.Max(d1, d2));

                if (max > granularity)
                {
                    if (d0 == max)
                    {
                        subdividedPositions.Add((v0 + v1) * 0.5);
                        int i = subdividedPositions.Count - 1;

                        triangles.Enqueue(new TriangleIndicesInt32(triangle.I0, i, triangle.I2));
                        triangles.Enqueue(new TriangleIndicesInt32(i, triangle.I1, triangle.I2));
                    }
                    else if (d1 == max)
                    {
                        subdividedPositions.Add((v1 + v2) * 0.5);
                        int i = subdividedPositions.Count - 1;

                        triangles.Enqueue(new TriangleIndicesInt32(triangle.I1, i, triangle.I0));
                        triangles.Enqueue(new TriangleIndicesInt32(i, triangle.I2, triangle.I0));
                    }
                    else if (d2 == max)
                    {
                        subdividedPositions.Add((v2 + v0) * 0.5);
                        int i = subdividedPositions.Count - 1;

                        triangles.Enqueue(new TriangleIndicesInt32(triangle.I2, i, triangle.I1));
                        triangles.Enqueue(new TriangleIndicesInt32(i, triangle.I0, triangle.I1));
                    }
                }
                else
                {
                    done.Enqueue(triangle);
                }
            }

            //
            // New indicies
            //
            IndicesInt32 subdividedIndices = new IndicesInt32(done.Count * 3);
            foreach (TriangleIndicesInt32 t in done)
            {
                subdividedIndices.AddTriangle(t);
            }

            return new TriangleMeshSubdivisionResult(subdividedPositions, subdividedIndices);
        }
    }
}