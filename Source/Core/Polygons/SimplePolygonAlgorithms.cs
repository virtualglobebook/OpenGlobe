#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System.Collections.Generic;
using System;

namespace OpenGlobe.Core
{
    public enum PolygonWindingOrder
    {
        Clockwise,
        Counterclockwise
    }

    public static class SimplePolygonAlgorithms
    {
        public static double ComputeArea(IEnumerable<Vector2D> positions)
        {
            if (positions == null)
            {
                throw new ArgumentNullException("positions");
            }

            int count = CollectionAlgorithms.EnumerableCount(positions);

            if (count < 3)
            {
                throw new ArgumentOutOfRangeException("positions", "At least three positions are required.");
            }

            //
            // Compute the area of the polygon.  The sign of the area determines the winding order.
            //
            double area = 0.0;
            bool first = true;
            Vector2D firstPosition = Vector2D.Zero;
            Vector2D previousPosition = Vector2D.Zero;

            foreach (Vector2D position in positions)
            {
                if (first)
                {
                    firstPosition = position;
                    first = false;
                }
                else
                {
                    area += (previousPosition.X * position.Y) - (position.X * previousPosition.Y);
                }

                previousPosition = position;
            }
            area += (previousPosition.X * firstPosition.Y) - (firstPosition.X * previousPosition.Y);

            return area * 0.5;
        }

        public static PolygonWindingOrder ComputeWindingOrder(IEnumerable<Vector2D> positions)
        {
            return (ComputeArea(positions) >= 0.0) ? PolygonWindingOrder.Counterclockwise : PolygonWindingOrder.Clockwise;
        }
    }
}