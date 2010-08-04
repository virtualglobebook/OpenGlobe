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
            Queue<SubdivisionTriangle> triangles = new Queue<SubdivisionTriangle>(indices.Values.Count / 3);
            Queue<SubdivisionTriangle> done = new Queue<SubdivisionTriangle>(indices.Values.Count / 3);

            //
            // New positions due to edge splits are appended to the positions list.
            //
            IList<Vector3D> subdividedPositions = CollectionAlgorithms.EnumerableToList(positions);

            //
            // Ensure shared edges are not duplicated
            //
            Dictionary<Edge, SubdivisionEdge> edges = new Dictionary<Edge, SubdivisionEdge>();

            IList<int> indicesValues = indices.Values;
            for (int i = 0; i < indicesValues.Count; i += 3)
            {
                triangles.Enqueue(new SubdivisionTriangle(
                    CreateEdge(edges, indicesValues[i], indicesValues[i + 1]),
                    CreateEdge(edges, indicesValues[i + 1], indicesValues[i + 2]),
                    CreateEdge(edges, indicesValues[i + 2], indicesValues[i])));
            }

            //
            // Subdivide triangles until we run out
            //
            while (triangles.Count != 0)
            {
                SubdivisionTriangle triangle = triangles.Dequeue();

                if (true)
                {
                    done.Enqueue(triangle);
                }
                else
                {
                }
            }

            //
            // New indicies
            //
            IndicesInt32 subdividedIndices = new IndicesInt32(done.Count * 3);
            foreach (SubdivisionTriangle t in done)
            {
                if (!t.Edge0.FlipDirection && !t.Edge1.FlipDirection)
                {
                    subdividedIndices.AddTriangle(new TriangleIndicesInt32(
                        t.Edge0.Edge.Edge.Index0,
                        t.Edge0.Edge.Edge.Index1,
                        t.Edge1.Edge.Edge.Index1));
                }
                else if (!t.Edge0.FlipDirection && t.Edge1.FlipDirection)
                {
                    subdividedIndices.AddTriangle(new TriangleIndicesInt32(
                        t.Edge0.Edge.Edge.Index0,
                        t.Edge0.Edge.Edge.Index1,
                        t.Edge1.Edge.Edge.Index0));
                }
                else if (t.Edge0.FlipDirection && !t.Edge1.FlipDirection)
                {
                    subdividedIndices.AddTriangle(new TriangleIndicesInt32(
                        t.Edge0.Edge.Edge.Index1,
                        t.Edge0.Edge.Edge.Index0,
                        t.Edge1.Edge.Edge.Index1));
                }
                else
                {
                    subdividedIndices.AddTriangle(new TriangleIndicesInt32(
                        t.Edge0.Edge.Edge.Index1,
                        t.Edge0.Edge.Edge.Index0,
                        t.Edge1.Edge.Edge.Index0));
                }
            }

            return new TriangleMeshSubdivisionResult(subdividedPositions, subdividedIndices);
        }

        private static SubdivisionTriangleEdge CreateEdge(Dictionary<Edge, SubdivisionEdge> edges, int i0, int i1)
        {
            Edge edge = new Edge(Math.Min(i0, i1), Math.Max(i0, i1));

            SubdivisionEdge subdivisionEdge;
            if (!edges.TryGetValue(edge, out subdivisionEdge))
            {
                subdivisionEdge = new SubdivisionEdge(edge);
                edges.Add(edge, subdivisionEdge);
            }

            return new SubdivisionTriangleEdge(subdivisionEdge, (i1 < i0));
        }
    }
}