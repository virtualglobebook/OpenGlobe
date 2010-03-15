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
using MiniGlobe.Core.Geometry;

namespace MiniGlobe.Core.Tessellation
{
    [Flags]
    public enum SubdivisionEllipsoidVertexAttributes
    {
        Position = 1,
        Normal = 2,
        TextureCoordinate = 4,
        All = Position | Normal | TextureCoordinate
    }

    public static class SubdivisionEllipsoidTessellator
    {
        internal class SubdivisionMesh
        {
            public Ellipsoid Ellipsoid { get; set; }
            public IList<Vector3D> Positions { get; set; }
            public IList<Vector3H> Normals { get; set; }
            public IList<Vector2H> TextureCoordinate { get; set; }
            public IndicesInt Indices { get; set; }
        }

        public static Mesh Compute(Ellipsoid ellipsoid, int numberOfSubdivisions, SubdivisionEllipsoidVertexAttributes vertexAttributes)
        {
            if (numberOfSubdivisions < 0)
            {
                throw new ArgumentOutOfRangeException("numberOfSubdivisions");
            }

            if ((vertexAttributes & SubdivisionEllipsoidVertexAttributes.Position) != SubdivisionEllipsoidVertexAttributes.Position)
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
            subdivisionMesh.Ellipsoid = ellipsoid;
            subdivisionMesh.Positions = positionsAttribute.Values;
            subdivisionMesh.Indices = indices;

            if ((vertexAttributes & SubdivisionEllipsoidVertexAttributes.Normal) == SubdivisionEllipsoidVertexAttributes.Normal)
            {
                VertexAttributeHalfFloatVector3 normalsAttribute = new VertexAttributeHalfFloatVector3("normal", numberOfVertices);
                mesh.Attributes.Add(normalsAttribute);
                subdivisionMesh.Normals = normalsAttribute.Values;
            }

            if ((vertexAttributes & SubdivisionEllipsoidVertexAttributes.TextureCoordinate) == SubdivisionEllipsoidVertexAttributes.TextureCoordinate)
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

            Vector3D n0 = new Vector3D(0, 0, 1);
            Vector3D n1 = new Vector3D(0, (2.0 * Math.Sqrt(2.0)) / 3.0, negativeOneThird);
            Vector3D n2 = new Vector3D(-rootSixOverThree, negativeRootTwoOverThree, negativeOneThird);
            Vector3D n3 = new Vector3D(rootSixOverThree, negativeRootTwoOverThree, negativeOneThird);

            Vector3D p0 = n0.MultiplyComponents(ellipsoid.Radii);
            Vector3D p1 = n1.MultiplyComponents(ellipsoid.Radii);
            Vector3D p2 = n2.MultiplyComponents(ellipsoid.Radii);
            Vector3D p3 = n3.MultiplyComponents(ellipsoid.Radii);
            
            subdivisionMesh.Positions.Add(p0);
            subdivisionMesh.Positions.Add(p1);
            subdivisionMesh.Positions.Add(p2);
            subdivisionMesh.Positions.Add(p3);

            if ((subdivisionMesh.Normals != null) || (subdivisionMesh.TextureCoordinate != null))
            {
                Vector3D d0 = ellipsoid.DeticSurfaceNormal(p0);
                Vector3D d1 = ellipsoid.DeticSurfaceNormal(p1);
                Vector3D d2 = ellipsoid.DeticSurfaceNormal(p2);
                Vector3D d3 = ellipsoid.DeticSurfaceNormal(p3);

                if (subdivisionMesh.Normals != null)
                {
                    subdivisionMesh.Normals.Add(d0.ToVector3H());
                    subdivisionMesh.Normals.Add(d1.ToVector3H());
                    subdivisionMesh.Normals.Add(d2.ToVector3H());
                    subdivisionMesh.Normals.Add(d3.ToVector3H());
                }

                if (subdivisionMesh.TextureCoordinate != null)
                {
                    subdivisionMesh.TextureCoordinate.Add(SubdivisionUtility.ComputeTextureCoordinate(d0));
                    subdivisionMesh.TextureCoordinate.Add(SubdivisionUtility.ComputeTextureCoordinate(d1));
                    subdivisionMesh.TextureCoordinate.Add(SubdivisionUtility.ComputeTextureCoordinate(d2));
                    subdivisionMesh.TextureCoordinate.Add(SubdivisionUtility.ComputeTextureCoordinate(d3));
                }
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
                IList<Vector3D> positions = subdivisionMesh.Positions;
                Vector3D n01 = ((positions[triangle.I0] + positions[triangle.I1]) * 0.5).Normalize();
                Vector3D n12 = ((positions[triangle.I1] + positions[triangle.I2]) * 0.5).Normalize();
                Vector3D n20 = ((positions[triangle.I2] + positions[triangle.I0]) * 0.5).Normalize();

                Vector3D p01 = n01.MultiplyComponents(subdivisionMesh.Ellipsoid.Radii);
                Vector3D p12 = n12.MultiplyComponents(subdivisionMesh.Ellipsoid.Radii);
                Vector3D p20 = n20.MultiplyComponents(subdivisionMesh.Ellipsoid.Radii);

                positions.Add(p01);
                positions.Add(p12);
                positions.Add(p20);

                int i01 = positions.Count - 3;
                int i12 = positions.Count - 2;
                int i20 = positions.Count - 1;

                if ((subdivisionMesh.Normals != null) || (subdivisionMesh.TextureCoordinate != null))
                {
                    Vector3D d01 = subdivisionMesh.Ellipsoid.DeticSurfaceNormal(p01);
                    Vector3D d12 = subdivisionMesh.Ellipsoid.DeticSurfaceNormal(p12);
                    Vector3D d20 = subdivisionMesh.Ellipsoid.DeticSurfaceNormal(p20);

                    if (subdivisionMesh.Normals != null)
                    {
                        subdivisionMesh.Normals.Add(d01.ToVector3H());
                        subdivisionMesh.Normals.Add(d12.ToVector3H());
                        subdivisionMesh.Normals.Add(d20.ToVector3H());
                    }

                    if (subdivisionMesh.TextureCoordinate != null)
                    {
                        subdivisionMesh.TextureCoordinate.Add(SubdivisionUtility.ComputeTextureCoordinate(d01));
                        subdivisionMesh.TextureCoordinate.Add(SubdivisionUtility.ComputeTextureCoordinate(d12));
                        subdivisionMesh.TextureCoordinate.Add(SubdivisionUtility.ComputeTextureCoordinate(d20));
                    }
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