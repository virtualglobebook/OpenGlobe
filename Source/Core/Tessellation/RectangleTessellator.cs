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

namespace OpenGlobe.Core.Tessellation
{
    public static class RectangleTessellator
    {
        public static Mesh Compute(RectangleD rectangle, int numberOfPartitionsX, int numberOfPartitionsY)
        {
            if (numberOfPartitionsX < 0)
            {
                throw new ArgumentOutOfRangeException("numberOfPartitionsX");
            }

            if (numberOfPartitionsY < 0)
            {
                throw new ArgumentOutOfRangeException("numberOfPartitionsY");
            }

            Mesh mesh = new Mesh();
            mesh.PrimitiveType = PrimitiveType.Triangles;
            mesh.FrontFaceWindingOrder = WindingOrder.Counterclockwise;

            int numberOfPositions = (numberOfPartitionsX + 1) * (numberOfPartitionsY + 1);
            VertexAttributeDoubleVector2 positionsAttribute = new VertexAttributeDoubleVector2("position", numberOfPositions);
            IList<Vector2D> positions = positionsAttribute.Values;
            mesh.Attributes.Add(positionsAttribute);

            int numberOfIndices = (numberOfPartitionsX * numberOfPartitionsY) * 6;
            IndicesInt32 indices = new IndicesInt32(numberOfIndices);
            mesh.Indices = indices;

            //
            // Positions
            //
            Vector2D lowerLeft = rectangle.LowerLeft;
            Vector2D toUpperRight = rectangle.UpperRight - lowerLeft;

            for (int y = 0; y <= numberOfPartitionsY; ++y)
            {
                double deltaY = y / (double)numberOfPartitionsY;
                double currentY = lowerLeft.Y + (deltaY * toUpperRight.Y);

                for (int x = 0; x <= numberOfPartitionsX; ++x)
                {
                    double deltaX = x / (double)numberOfPartitionsX;
                    double currentX = lowerLeft.X + (deltaX * toUpperRight.X);
                    positions.Add(new Vector2D(currentX, currentY));
                }
            }

            //
            // Indices
            //
            int rowDelta = numberOfPartitionsX + 1;
            int i = 0;
            for (int y = 0; y < numberOfPartitionsY; ++y)
            {
                for (int x = 0; x < numberOfPartitionsX; ++x)
                {
                    indices.AddTriangle(new TriangleIndicesInt32(i, i + 1, rowDelta + (i + 1)));
                    indices.AddTriangle(new TriangleIndicesInt32(i, rowDelta + (i + 1), rowDelta + i));
                    i += 1;
                }
                i += 1;
            }

            return mesh;
        }
    }
}