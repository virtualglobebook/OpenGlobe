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

namespace OpenGlobe.Core
{
    public enum PolygonWindingOrder
    {
        Clockwise,
        Counterclockwise
    }

    public static class SimplePolygonAlgorithms
    {
        /// <summary>
        /// Cleans up a simple polygon by removing duplicate adjacent positions and making
        /// the first position not equal the last position
        /// </summary>
        public static IList<T> Cleanup<T>(IEnumerable<T> positions)
        {
            int count = PolygonCount(positions);

            List<T> cleanedPositions = new List<T>(count);

            bool first = true;
            T firstPosition = default(T);
            T previousPosition = default(T);

            foreach (T position in positions)
            {
                if (first)
                {
                    firstPosition = position;
                    first = false;
                }
                else if (!previousPosition.Equals(position))
                {
                    cleanedPositions.Add(previousPosition);
                }

                previousPosition = position;
            }

            if (!previousPosition.Equals(firstPosition))
            {
                cleanedPositions.Add(previousPosition);
            }

            cleanedPositions.TrimExcess();
            return cleanedPositions;
        }

        public static double ComputeArea(IEnumerable<Vector2D> positions)
        {
            int count = PolygonCount(positions);

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

        private static int PolygonCount<T>(IEnumerable<T> positions)
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

            return count;
        }
    }
}