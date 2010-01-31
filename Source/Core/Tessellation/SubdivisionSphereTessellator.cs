#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
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
    public enum SubdivisionSphereVertexAttributes
    {
        Position = 1,
        Normal = 2,
        TextureCoordinate = 4,
        All = Position | Normal | TextureCoordinate
    }

    public static class SubdivisionSphereTessellator
    {
        internal class SubdivisionMesh
        {
            public IList<Vector3d> Positions { get; set; }
            public IList<Vector3h> Normals { get; set; }
            public IList<Vector2h> TextureCoordinate { get; set; }
            public IndicesInt Indices { get; set; }
        }

        public static Mesh Compute(int numberOfSubdivisions, SubdivisionSphereVertexAttributes vertexAttributes)
        {
            if (numberOfSubdivisions < 0)
            {
                throw new ArgumentOutOfRangeException("numberOfSubdivisions");
            }

            if ((vertexAttributes & SubdivisionSphereVertexAttributes.Position) != SubdivisionSphereVertexAttributes.Position)
            {
                throw new ArgumentException("Positions must be provided.", "vertexAttributes");
            }

            Mesh mesh = new Mesh();
            mesh.PrimitiveType = PrimitiveType.Triangles;
            mesh.FrontFaceWindingOrder = WindingOrder.Counterclockwise;

            int numberOfVertices = SubdivisionUtility.NumberOfVertices(numberOfSubdivisions);
            VertexAttributeDoubleVector3 positionsAttribute = new VertexAttributeDoubleVector3("position", numberOfVertices);
            mesh.Attributes.Add(positionsAttribute);

            IndicesInt indices = new IndicesInt(3 * SubdivisionUtility.NumberOfTriangles(numberOfSubdivisions));
            mesh.Indices = indices;

            SubdivisionMesh subdivisionMesh = new SubdivisionMesh();
            subdivisionMesh.Positions = positionsAttribute.Values;
            subdivisionMesh.Indices = indices;

            if ((vertexAttributes & SubdivisionSphereVertexAttributes.Normal) == SubdivisionSphereVertexAttributes.Normal)
            {
                VertexAttributeHalfFloatVector3 normalsAttribute = new VertexAttributeHalfFloatVector3("normal", numberOfVertices);
                mesh.Attributes.Add(normalsAttribute);
                subdivisionMesh.Normals = normalsAttribute.Values;
            }

            if ((vertexAttributes & SubdivisionSphereVertexAttributes.TextureCoordinate) == SubdivisionSphereVertexAttributes.TextureCoordinate)
            {
                VertexAttributeHalfFloatVector2 textureCoordinateAttribute = new VertexAttributeHalfFloatVector2("textureCoordinate", numberOfVertices);
                mesh.Attributes.Add(textureCoordinateAttribute);
                subdivisionMesh.TextureCoordinate = textureCoordinateAttribute.Values;
            }

            //
            // Initial tetrahedron
            //
            double negativeRootTwoOverThree = -Math.Sqrt(2.0) / 3.0;
            const double negativeOneThird = -1.0 / 3.0;
            double rootSixOverThree = Math.Sqrt(6.0) / 3.0;

            Vector3d p0 = new Vector3d(0, 0, 1);
            Vector3d p1 = new Vector3d(0, (2.0 * Math.Sqrt(2.0)) / 3.0, negativeOneThird);
            Vector3d p2 = new Vector3d(-rootSixOverThree, negativeRootTwoOverThree, negativeOneThird);
            Vector3d p3 = new Vector3d(rootSixOverThree, negativeRootTwoOverThree, negativeOneThird);
            
            subdivisionMesh.Positions.Add(p0);
            subdivisionMesh.Positions.Add(p1);
            subdivisionMesh.Positions.Add(p2);
            subdivisionMesh.Positions.Add(p3);

            if (subdivisionMesh.Normals != null)
            {
                subdivisionMesh.Normals.Add(new Vector3h(p0));
                subdivisionMesh.Normals.Add(new Vector3h(p1));
                subdivisionMesh.Normals.Add(new Vector3h(p2));
                subdivisionMesh.Normals.Add(new Vector3h(p3));
            }

            if (subdivisionMesh.TextureCoordinate != null)
            {
                subdivisionMesh.TextureCoordinate.Add(SubdivisionUtility.ComputeTextureCoordinate(p0));
                subdivisionMesh.TextureCoordinate.Add(SubdivisionUtility.ComputeTextureCoordinate(p1));
                subdivisionMesh.TextureCoordinate.Add(SubdivisionUtility.ComputeTextureCoordinate(p2));
                subdivisionMesh.TextureCoordinate.Add(SubdivisionUtility.ComputeTextureCoordinate(p3));
            }

            Subdivide(subdivisionMesh, new TriangleIndices<int>(0, 1, 2), numberOfSubdivisions);
            Subdivide(subdivisionMesh, new TriangleIndices<int>(0, 2, 3), numberOfSubdivisions);
            Subdivide(subdivisionMesh, new TriangleIndices<int>(0, 3, 1), numberOfSubdivisions);
            Subdivide(subdivisionMesh, new TriangleIndices<int>(1, 3, 2), numberOfSubdivisions);

            return mesh;
        }

        private static void Subdivide(SubdivisionMesh subdivisionMesh, TriangleIndices<int> triangle, int level)
        {
            if (level > 0)
            {
                IList<Vector3d> positions = subdivisionMesh.Positions;
                Vector3d p01 = Vector3d.Normalize((positions[triangle.I0] + positions[triangle.I1]) * 0.5);
                Vector3d p12 = Vector3d.Normalize((positions[triangle.I1] + positions[triangle.I2]) * 0.5);
                Vector3d p20 = Vector3d.Normalize((positions[triangle.I2] + positions[triangle.I0]) * 0.5);

                positions.Add(p01);
                positions.Add(p12);
                positions.Add(p20);

                int i01 = positions.Count - 3;
                int i12 = positions.Count - 2;
                int i20 = positions.Count - 1;

                if (subdivisionMesh.Normals != null)
                {
                    subdivisionMesh.Normals.Add(new Vector3h(p01));
                    subdivisionMesh.Normals.Add(new Vector3h(p12));
                    subdivisionMesh.Normals.Add(new Vector3h(p20));
                }

                if (subdivisionMesh.TextureCoordinate != null)
                {
                    subdivisionMesh.TextureCoordinate.Add(SubdivisionUtility.ComputeTextureCoordinate(p01));
                    subdivisionMesh.TextureCoordinate.Add(SubdivisionUtility.ComputeTextureCoordinate(p12));
                    subdivisionMesh.TextureCoordinate.Add(SubdivisionUtility.ComputeTextureCoordinate(p20));
                }

                //
                // Subdivide input triangle into four triangles
                //
                --level;
                Subdivide(subdivisionMesh, new TriangleIndices<int>(triangle.I0, i01, i20), level);
                Subdivide(subdivisionMesh, new TriangleIndices<int>(i01, triangle.I1, i12), level);
                Subdivide(subdivisionMesh, new TriangleIndices<int>(i01, i12, i20), level);
                Subdivide(subdivisionMesh, new TriangleIndices<int>(i20, i12, triangle.I2), level);
            }
            else
            {
                subdivisionMesh.Indices.AddTriangle(triangle);
            }
        }
    }
}
