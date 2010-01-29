#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using OpenTK;
using System;
using System.Collections.Generic;
using MiniGlobe.Core.Geometry;

namespace MiniGlobe.Core.Tessellation
{
    [Flags]
    public enum QuadCubeEllipsoidVertexAttributes
    {
        Position = 1,
        Normal = 2,
        TextureCoordinate = 4,
        All = Position | Normal | TextureCoordinate
    }
    
    public static class QuadCubeEllipsoidTessellator
    {
        internal class QuadCubeMesh
        {
            public Ellipsoid Ellipsoid { get; set; }
            public int NumberOfPartitions { get; set; }
            public IList<Vector3d> Positions { get; set; }
            public IList<Vector3h> Normals { get; set; }
            public IList<Vector2h> TextureCoordinate { get; set; }
            public IndicesInt Indices { get; set; }
        }

        public static Mesh Compute(Ellipsoid ellipsoid, int numberOfPartitions, QuadCubeEllipsoidVertexAttributes vertexAttributes)
        {
            if (numberOfPartitions < 0)
            {
                throw new ArgumentOutOfRangeException("numberOfPartitions");
            }

            if ((vertexAttributes & QuadCubeEllipsoidVertexAttributes.Position) != QuadCubeEllipsoidVertexAttributes.Position)
            {
                throw new ArgumentException("Positions must be provided.", "vertexAttributes");
            }

            Mesh mesh = new Mesh();
            mesh.PrimitiveType = PrimitiveType.Triangles;
            mesh.FrontFaceWindingOrder = WindingOrder.Counterclockwise;

            VertexAttributeDoubleVector3 positionsAttribute = new VertexAttributeDoubleVector3(
                "position", NumberOfVertices(numberOfPartitions));
            mesh.Attributes.Add(positionsAttribute);

            IndicesInt indices = new IndicesInt(3 * NumberOfTriangles(numberOfPartitions));
            mesh.Indices = indices;

            QuadCubeMesh quadCubeMesh = new QuadCubeMesh();
            quadCubeMesh.Ellipsoid = ellipsoid;
            quadCubeMesh.NumberOfPartitions = numberOfPartitions;
            quadCubeMesh.Positions = positionsAttribute.Values;
            quadCubeMesh.Indices = indices;

            if ((vertexAttributes & QuadCubeEllipsoidVertexAttributes.Normal) == QuadCubeEllipsoidVertexAttributes.Normal)
            {
                VertexAttributeHalfFloatVector3 normalsAttribute = new VertexAttributeHalfFloatVector3("normal");
                mesh.Attributes.Add(normalsAttribute);
                quadCubeMesh.Normals = normalsAttribute.Values;
            }

            if ((vertexAttributes & QuadCubeEllipsoidVertexAttributes.TextureCoordinate) == QuadCubeEllipsoidVertexAttributes.TextureCoordinate)
            {
                VertexAttributeHalfFloatVector2 textureCoordinateAttribute = new VertexAttributeHalfFloatVector2("textureCoordinate");
                mesh.Attributes.Add(textureCoordinateAttribute);
                quadCubeMesh.TextureCoordinate = textureCoordinateAttribute.Values;
            }

            //
            // Initial cube.  In the plane, z = -1:
            //
            //                   +y
            //                    |
            //             Q2     * p3     Q1
            //                  / | \
            //              p0 *--+--* p2   +x
            //                  \ | /
            //             Q3     * p1     Q4
            //                    |
            //
            // Similarly, p4 to p7 are in the plane z = 1.
            //
            quadCubeMesh.Positions.Add(new Vector3d(-1, 0, -1));
            quadCubeMesh.Positions.Add(new Vector3d(0, -1, -1));
            quadCubeMesh.Positions.Add(new Vector3d(1, 0, -1));
            quadCubeMesh.Positions.Add(new Vector3d(0, 1, -1));
            quadCubeMesh.Positions.Add(new Vector3d(-1, 0, 1));
            quadCubeMesh.Positions.Add(new Vector3d(0, -1, 1));
            quadCubeMesh.Positions.Add(new Vector3d(1, 0, 1));
            quadCubeMesh.Positions.Add(new Vector3d(0, 1, 1));

            //
            // Edges
            //
            // 0 -> 1, 1 -> 2, 2 -> 3, 3 -> 0.  Plane z = -1
            // 4 -> 5, 5 -> 6, 6 -> 7, 7 -> 4.  Plane z = 1
            // 0 -> 4, 1 -> 5, 2 -> 6, 3 -> 7.  From plane z = -1 to plane z - 1
            //
            int[] edge0to1 = AddEdgePositions(0, 1, quadCubeMesh);
            int[] edge1to2 = AddEdgePositions(1, 2, quadCubeMesh);
            int[] edge2to3 = AddEdgePositions(2, 3, quadCubeMesh);
            int[] edge3to0 = AddEdgePositions(3, 0, quadCubeMesh);

            int[] edge4to5 = AddEdgePositions(4, 5, quadCubeMesh);
            int[] edge5to6 = AddEdgePositions(5, 6, quadCubeMesh);
            int[] edge6to7 = AddEdgePositions(6, 7, quadCubeMesh);
            int[] edge7to4 = AddEdgePositions(7, 4, quadCubeMesh);

            int[] edge0to4 = AddEdgePositions(0, 4, quadCubeMesh);
            int[] edge1to5 = AddEdgePositions(1, 5, quadCubeMesh);
            int[] edge2to6 = AddEdgePositions(2, 6, quadCubeMesh);
            int[] edge3to7 = AddEdgePositions(3, 7, quadCubeMesh);

            AddFaceTriangles(edge0to4, edge0to1, edge1to5, edge4to5, quadCubeMesh); // Q3 Face
            AddFaceTriangles(edge1to5, edge1to2, edge2to6, edge5to6, quadCubeMesh); // Q4 Face
            AddFaceTriangles(edge2to6, edge2to3, edge3to7, edge6to7, quadCubeMesh); // Q1 Face
            AddFaceTriangles(edge3to7, edge3to0, edge0to4, edge7to4, quadCubeMesh); // Q2 Face
            AddFaceTriangles(ReversedArray(edge7to4), edge4to5, edge5to6, ReversedArray(edge6to7), quadCubeMesh); // Plane z = 1
            AddFaceTriangles(edge1to2, ReversedArray(edge0to1), ReversedArray(edge3to0), edge2to3, quadCubeMesh); // Plane z = -1

            CubeToEllipsoid(quadCubeMesh);
            return mesh;
        }

        private static int[] AddEdgePositions(int i0, int i1, QuadCubeMesh quadCubeMesh)
        {
            IList<Vector3d> positions = quadCubeMesh.Positions;
            int numberOfPartitions = quadCubeMesh.NumberOfPartitions;

            int[] indices = new int[2 + (numberOfPartitions - 1)];
            indices[0] = i0;
            indices[indices.Length - 1] = i1;

            Vector3d origin = positions[i0];
            Vector3d direction = positions[i1] - positions[i0];

            for (int i = 1; i < numberOfPartitions; ++i)
            {
                double delta = i / (double)numberOfPartitions;

                indices[i] = positions.Count;
                positions.Add(origin + (delta * direction));
            }

            return indices;
        }

        private static void AddFaceTriangles(
            int[] leftBottomToTop,
            int[] bottomLeftToRight,
            int[] rightBottomToTop,
            int[] topLeftToRight,
            QuadCubeMesh quadCubeMesh)
        {
            IList<Vector3d> positions = quadCubeMesh.Positions;
            IndicesInt indices = quadCubeMesh.Indices;
            int numberOfPartitions = quadCubeMesh.NumberOfPartitions;

            Vector3d origin = positions[bottomLeftToRight[0]];
            Vector3d x = positions[bottomLeftToRight[bottomLeftToRight.Length - 1]] - origin;
            Vector3d y = positions[topLeftToRight[0]] - origin;

            int[] bottomIndicesBuffer = new int[numberOfPartitions + 1];
            int[] topIndicesBuffer = new int[numberOfPartitions + 1];

            int[] bottomIndices = bottomLeftToRight;
            int[] topIndices = topIndicesBuffer;

            for (int j = 1; j <= numberOfPartitions; ++j)
            {
                if (j != numberOfPartitions)
                {
                    if (j != 1)
                    {
                        //
                        // This copy could be avoided by ping ponging buffers.
                        //
                        topIndicesBuffer.CopyTo(bottomIndicesBuffer, 0);
                        bottomIndices = bottomIndicesBuffer;
                    }

                    topIndicesBuffer[0] = leftBottomToTop[j];
                    topIndicesBuffer[numberOfPartitions] = rightBottomToTop[j];

                    double deltaY = j / (double)numberOfPartitions;
                    Vector3d offsetY = deltaY * y;

                    for (int i = 1; i < numberOfPartitions; ++i)
                    {
                        double deltaX = i / (double)numberOfPartitions;
                        Vector3d offsetX = deltaX * x;

                        topIndicesBuffer[i] = quadCubeMesh.Positions.Count;
                        positions.Add(origin + offsetX + offsetY);
                    }
                }
                else
                {
                    if (j != 1)
                    {
                        bottomIndices = topIndicesBuffer;
                    }
                    topIndices = topLeftToRight;
                }

                for (int i = 0; i < numberOfPartitions; ++i)
                {
                    indices.AddTriangle(new TriangleIndices<int>(
                        bottomIndices[i], bottomIndices[i + 1], topIndices[i + 1]));
                    indices.AddTriangle(new TriangleIndices<int>(
                        bottomIndices[i], topIndices[i + 1], topIndices[i]));
                }
            }
        }

        private static void CubeToEllipsoid(QuadCubeMesh quadCubeMesh)
        {
            IList<Vector3d> positions = quadCubeMesh.Positions;

            for (int i = 0; i < positions.Count; ++i)
            {
                positions[i] = Vector3d.Multiply(Vector3d.Normalize(positions[i]), quadCubeMesh.Ellipsoid.Radii);

                if ((quadCubeMesh.Normals != null) || (quadCubeMesh.TextureCoordinate != null))
                {
                    Vector3d deticSurfaceNormal = quadCubeMesh.Ellipsoid.DeticSurfaceNormal(positions[i]);

                    if (quadCubeMesh.Normals != null)
                    {
                        quadCubeMesh.Normals.Add(new Vector3h(deticSurfaceNormal));
                    }

                    if (quadCubeMesh.TextureCoordinate != null)
                    {
                        quadCubeMesh.TextureCoordinate.Add(SubdivisionUtility.ComputeTextureCoordinate(deticSurfaceNormal));
                    }
                }
            }
        }

        private static int NumberOfTriangles(int numberOfPartitions)
        {
            return 6 * 2 * numberOfPartitions * numberOfPartitions;
        }

        private static int NumberOfVertices(int numberOfPartitions)
        {
            int numberOfPartitionsMinusOne = numberOfPartitions -1 ;
            int numberOfVertices = 8;                                                           // Corners
            numberOfVertices += 12 * numberOfPartitionsMinusOne;                                // Edges
            numberOfVertices += 6 * numberOfPartitionsMinusOne * numberOfPartitionsMinusOne;    // Faces
            return numberOfVertices;
        }

        private static T[] ReversedArray<T>(T[] array)
        {
            T[] reversed = new T[array.Length];

            int j = 0;
            int i = array.Length - 1;
            while (i >= 0)
            {
                reversed[j++] = array[i--];
            }

            return reversed;
        }
    }
}