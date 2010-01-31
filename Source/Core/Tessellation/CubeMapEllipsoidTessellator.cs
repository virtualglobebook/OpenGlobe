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
    public enum CubeMapEllipsoidVertexAttributes
    {
        Position = 1,
        Normal = 2,
        TextureCoordinate = 4,
        All = Position | Normal | TextureCoordinate
    }
    
    public static class CubeMapEllipsoidTessellator
    {
        internal class CubeMapMesh
        {
            public Ellipsoid Ellipsoid { get; set; }
            public int NumberOfPartitions { get; set; }
            public IList<Vector3d> Positions { get; set; }
            public IList<Vector3h> Normals { get; set; }
            public IList<Vector2h> TextureCoordinate { get; set; }
            public IndicesInt Indices { get; set; }
        }

        public static Mesh Compute(Ellipsoid ellipsoid, int numberOfPartitions, CubeMapEllipsoidVertexAttributes vertexAttributes)
        {
            if (numberOfPartitions < 0)
            {
                throw new ArgumentOutOfRangeException("numberOfPartitions");
            }

            if ((vertexAttributes & CubeMapEllipsoidVertexAttributes.Position) != CubeMapEllipsoidVertexAttributes.Position)
            {
                throw new ArgumentException("Positions must be provided.", "vertexAttributes");
            }

            Mesh mesh = new Mesh();
            mesh.PrimitiveType = PrimitiveType.Triangles;
            mesh.FrontFaceWindingOrder = WindingOrder.Counterclockwise;

            int numberOfVertices = NumberOfVertices(numberOfPartitions);
            VertexAttributeDoubleVector3 positionsAttribute = new VertexAttributeDoubleVector3("position", numberOfVertices);
            mesh.Attributes.Add(positionsAttribute);

            IndicesInt indices = new IndicesInt(3 * NumberOfTriangles(numberOfPartitions));
            mesh.Indices = indices;

            CubeMapMesh CubeMapMesh = new CubeMapMesh();
            CubeMapMesh.Ellipsoid = ellipsoid;
            CubeMapMesh.NumberOfPartitions = numberOfPartitions;
            CubeMapMesh.Positions = positionsAttribute.Values;
            CubeMapMesh.Indices = indices;

            if ((vertexAttributes & CubeMapEllipsoidVertexAttributes.Normal) == CubeMapEllipsoidVertexAttributes.Normal)
            {
                VertexAttributeHalfFloatVector3 normalsAttribute = new VertexAttributeHalfFloatVector3("normal", numberOfVertices);
                mesh.Attributes.Add(normalsAttribute);
                CubeMapMesh.Normals = normalsAttribute.Values;
            }

            if ((vertexAttributes & CubeMapEllipsoidVertexAttributes.TextureCoordinate) == CubeMapEllipsoidVertexAttributes.TextureCoordinate)
            {
                VertexAttributeHalfFloatVector2 textureCoordinateAttribute = new VertexAttributeHalfFloatVector2("textureCoordinate", numberOfVertices);
                mesh.Attributes.Add(textureCoordinateAttribute);
                CubeMapMesh.TextureCoordinate = textureCoordinateAttribute.Values;
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
            CubeMapMesh.Positions.Add(new Vector3d(-1, 0, -1));
            CubeMapMesh.Positions.Add(new Vector3d(0, -1, -1));
            CubeMapMesh.Positions.Add(new Vector3d(1, 0, -1));
            CubeMapMesh.Positions.Add(new Vector3d(0, 1, -1));
            CubeMapMesh.Positions.Add(new Vector3d(-1, 0, 1));
            CubeMapMesh.Positions.Add(new Vector3d(0, -1, 1));
            CubeMapMesh.Positions.Add(new Vector3d(1, 0, 1));
            CubeMapMesh.Positions.Add(new Vector3d(0, 1, 1));

            //
            // Edges
            //
            // 0 -> 1, 1 -> 2, 2 -> 3, 3 -> 0.  Plane z = -1
            // 4 -> 5, 5 -> 6, 6 -> 7, 7 -> 4.  Plane z = 1
            // 0 -> 4, 1 -> 5, 2 -> 6, 3 -> 7.  From plane z = -1 to plane z - 1
            //
            int[] edge0to1 = AddEdgePositions(0, 1, CubeMapMesh);
            int[] edge1to2 = AddEdgePositions(1, 2, CubeMapMesh);
            int[] edge2to3 = AddEdgePositions(2, 3, CubeMapMesh);
            int[] edge3to0 = AddEdgePositions(3, 0, CubeMapMesh);

            int[] edge4to5 = AddEdgePositions(4, 5, CubeMapMesh);
            int[] edge5to6 = AddEdgePositions(5, 6, CubeMapMesh);
            int[] edge6to7 = AddEdgePositions(6, 7, CubeMapMesh);
            int[] edge7to4 = AddEdgePositions(7, 4, CubeMapMesh);

            int[] edge0to4 = AddEdgePositions(0, 4, CubeMapMesh);
            int[] edge1to5 = AddEdgePositions(1, 5, CubeMapMesh);
            int[] edge2to6 = AddEdgePositions(2, 6, CubeMapMesh);
            int[] edge3to7 = AddEdgePositions(3, 7, CubeMapMesh);

            AddFaceTriangles(edge0to4, edge0to1, edge1to5, edge4to5, CubeMapMesh); // Q3 Face
            AddFaceTriangles(edge1to5, edge1to2, edge2to6, edge5to6, CubeMapMesh); // Q4 Face
            AddFaceTriangles(edge2to6, edge2to3, edge3to7, edge6to7, CubeMapMesh); // Q1 Face
            AddFaceTriangles(edge3to7, edge3to0, edge0to4, edge7to4, CubeMapMesh); // Q2 Face
            AddFaceTriangles(ReversedArray(edge7to4), edge4to5, edge5to6, ReversedArray(edge6to7), CubeMapMesh); // Plane z = 1
            AddFaceTriangles(edge1to2, ReversedArray(edge0to1), ReversedArray(edge3to0), edge2to3, CubeMapMesh); // Plane z = -1

            CubeToEllipsoid(CubeMapMesh);
            return mesh;
        }

        private static int[] AddEdgePositions(int i0, int i1, CubeMapMesh CubeMapMesh)
        {
            IList<Vector3d> positions = CubeMapMesh.Positions;
            int numberOfPartitions = CubeMapMesh.NumberOfPartitions;

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
            CubeMapMesh CubeMapMesh)
        {
            IList<Vector3d> positions = CubeMapMesh.Positions;
            IndicesInt indices = CubeMapMesh.Indices;
            int numberOfPartitions = CubeMapMesh.NumberOfPartitions;

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

                        topIndicesBuffer[i] = CubeMapMesh.Positions.Count;
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

        private static void CubeToEllipsoid(CubeMapMesh CubeMapMesh)
        {
            IList<Vector3d> positions = CubeMapMesh.Positions;

            for (int i = 0; i < positions.Count; ++i)
            {
                positions[i] = Vector3d.Multiply(Vector3d.Normalize(positions[i]), CubeMapMesh.Ellipsoid.Radii);

                if ((CubeMapMesh.Normals != null) || (CubeMapMesh.TextureCoordinate != null))
                {
                    Vector3d deticSurfaceNormal = CubeMapMesh.Ellipsoid.DeticSurfaceNormal(positions[i]);

                    if (CubeMapMesh.Normals != null)
                    {
                        CubeMapMesh.Normals.Add(new Vector3h(deticSurfaceNormal));
                    }

                    if (CubeMapMesh.TextureCoordinate != null)
                    {
                        CubeMapMesh.TextureCoordinate.Add(SubdivisionUtility.ComputeTextureCoordinate(deticSurfaceNormal));
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