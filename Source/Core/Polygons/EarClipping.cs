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
    [CLSCompliant(false)]
    public static class EarClipping
    {
        public static IndicesUnsignedInt Triangulate(IEnumerable<Vector2D> positions)
        {
            //
            // O(n^3)
            //
            // There are several optimization opportunities:
            //   * http://www.geometrictools.com/Documentation/TriangulationByEarClipping.pdf
            //   * http://cgm.cs.mcgill.ca/~godfried/publications/triangulation.held.ps.gz
            //   * http://blogs.agi.com/insight3d/index.php/2008/03/20/triangulation-rhymes-with-strangulation/
            //

            if (positions == null)
            {
                throw new ArgumentNullException("positions");
            }

            //
            // Doubly linked list.  This would be a tad cleaner if it were also circular.
            //
            LinkedList<IndexedVector<Vector2D>> remainingPositions = new LinkedList<IndexedVector<Vector2D>>(); ;

            int index = 0;
            foreach (Vector2D position in positions)
            {
                remainingPositions.AddLast(new IndexedVector<Vector2D>(position, index++));
            }

            if (remainingPositions.Count < 3)
            {
                throw new ArgumentOutOfRangeException("positions", "At least three positions are required.");
            }
            
            IndicesUnsignedInt indices = new IndicesUnsignedInt(3 * (remainingPositions.Count - 2));

            ///////////////////////////////////////////////////////////////////

            LinkedListNode<IndexedVector<Vector2D>> previousNode = remainingPositions.First;
            LinkedListNode<IndexedVector<Vector2D>> node = previousNode.Next;
            LinkedListNode<IndexedVector<Vector2D>> nextNode = node.Next;

            int bailCount = remainingPositions.Count * remainingPositions.Count;

            while (remainingPositions.Count > 3)
            {
                Vector2D p0 = previousNode.Value.Vector;
                Vector2D p1 = node.Value.Vector;
                Vector2D p2 = nextNode.Value.Vector;

                if (IsTipConvex(p0, p1, p2))
                {
                    bool isEar = true;
                    for (LinkedListNode<IndexedVector<Vector2D>> n = ((nextNode.Next != null) ? nextNode.Next : remainingPositions.First);
                        n != previousNode;
                        n = ((n.Next != null) ? n.Next : remainingPositions.First))
                    {
                        if (ContainmentTests.PointInsideTriangle(n.Value.Vector, p0, p1, p2))
                        {
                            isEar = false;
                            break;
                        }
                    }

                    if (isEar)
                    {
                        indices.AddTriangle(new TriangleIndicesUnsignedInt(previousNode.Value.Index, node.Value.Index, nextNode.Value.Index));
                        remainingPositions.Remove(node);

                        node = nextNode;
                        nextNode = (nextNode.Next != null) ? nextNode.Next : remainingPositions.First;
                        continue;
                    }
                }

                previousNode = (previousNode.Next != null) ? previousNode.Next : remainingPositions.First;
                node = (node.Next != null) ? node.Next : remainingPositions.First;
                nextNode = (nextNode.Next != null) ? nextNode.Next : remainingPositions.First;

                if (--bailCount == 0)
                {
                    break;
                }
            }

            LinkedListNode<IndexedVector<Vector2D>> n0 = remainingPositions.First;
            LinkedListNode<IndexedVector<Vector2D>> n1 = n0.Next;
            LinkedListNode<IndexedVector<Vector2D>> n2 = n1.Next;
            indices.AddTriangle(new TriangleIndicesUnsignedInt(n0.Value.Index, n1.Value.Index, n2.Value.Index));

            return indices;
        }

        private static bool IsTipConvex(Vector2D p0, Vector2D p1, Vector2D p2)
        {
            Vector2D u = p1 - p0;
            Vector2D v = p2 - p1;

            //
            // Use the sign of the z component of the cross product
            //
            return ((u.X * v.Y) - (u.Y * v.X)) >= 0.0;
        }
   }
}