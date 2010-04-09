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
using MiniGlobe.Core.Geometry;

namespace MiniGlobe.Core.Tessellation
{
    public static class BoxTessellator
    {
        public static Mesh Compute(Vector3D length)
        {
            if (length.X < 0 || length.Y < 0 || length.Z < 0)
            {
                throw new ArgumentOutOfRangeException("length");
            }

            Mesh mesh = new Mesh();
            mesh.PrimitiveType = PrimitiveType.Triangles;
            mesh.FrontFaceWindingOrder = WindingOrder.Counterclockwise;

            VertexAttributeDoubleVector3 positionsAttribute = new VertexAttributeDoubleVector3("position", 8);
            mesh.Attributes.Add(positionsAttribute);

            IndicesByte indices = new IndicesByte(24);
            mesh.Indices = indices;

            //
            // 8 corner points
            //
            IList<Vector3D> positions = positionsAttribute.Values;

            Vector3D corner = 0.5 * length;
            positions.Add(new Vector3D(-corner.X, -corner.Y, -corner.Z));
            positions.Add(new Vector3D(corner.X, -corner.Y, -corner.Z));
            positions.Add(new Vector3D(corner.X, corner.Y, -corner.Z));
            positions.Add(new Vector3D(-corner.X, corner.Y, -corner.Z));
            positions.Add(new Vector3D(-corner.X, -corner.Y, corner.Z));
            positions.Add(new Vector3D(corner.X, -corner.Y, corner.Z));
            positions.Add(new Vector3D(corner.X, corner.Y, corner.Z));
            positions.Add(new Vector3D(-corner.X, corner.Y, corner.Z));

            //
            // 6 faces, 2 triangles each
            //
            indices.AddTriangle(new TriangleIndicesByte(4, 5, 6));    // Top: plane z = corner.Z
            indices.AddTriangle(new TriangleIndicesByte(4, 6, 7));
            indices.AddTriangle(new TriangleIndicesByte(1, 0, 3));    // Bottom: plane z = -corner.Z
            indices.AddTriangle(new TriangleIndicesByte(1, 3, 2));
            indices.AddTriangle(new TriangleIndicesByte(1, 6, 5));    // Side: plane x = corner.X
            indices.AddTriangle(new TriangleIndicesByte(1, 2, 6));
            indices.AddTriangle(new TriangleIndicesByte(2, 3, 7));    // Side: plane y = corner.Y
            indices.AddTriangle(new TriangleIndicesByte(2, 7, 6));
            indices.AddTriangle(new TriangleIndicesByte(3, 0, 4));    // Side: plane x = -corner.X
            indices.AddTriangle(new TriangleIndicesByte(3, 4, 7));
            indices.AddTriangle(new TriangleIndicesByte(0, 1, 5));    // Side: plane y = -corner.Y
            indices.AddTriangle(new TriangleIndicesByte(0, 5, 4));

            return mesh;
        }
    }
}