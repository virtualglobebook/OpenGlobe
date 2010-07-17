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
    public static class EarClipping
    {
        public static IndicesInt32 Triangulate(IEnumerable<Vector2D> positions)
        {
            //
            // Implementation based on http://www.geometrictools.com/Documentation/TriangulationByEarClipping.pdf.
            // O(n^2)
            //

            if (positions == null)
            {
                throw new ArgumentNullException("positions");
            }

            //
            // Doubly linked list.  I wish it were also circular.
            //
            LinkedList<IndexedVector2D> remainingPositions = new LinkedList<IndexedVector2D>(); ;

            int index = 0;
            foreach (Vector2D position in positions)
            {
                remainingPositions.AddLast(new IndexedVector2D(position, index++));
            }

            if (remainingPositions.Count < 3)
            {
                throw new ArgumentOutOfRangeException("positions", "At least three positions are required.");
            }
            
            IndicesInt32 indices = new IndicesInt32(3 * (remainingPositions.Count - 2));

            ///////////////////////////////////////////////////////////////////

            LinkedListNode<IndexedVector2D> previousNode = remainingPositions.First;
            LinkedListNode<IndexedVector2D> node = previousNode.Next;
            LinkedListNode<IndexedVector2D> nextNode = node.Next;

            while (remainingPositions.Count > 3)
            {
                Vector2D p0 = previousNode.Value.Vector;
                Vector2D p1 = node.Value.Vector;
                Vector2D p2 = nextNode.Value.Vector;

                bool isEar = true;
                for (LinkedListNode<IndexedVector2D> n = nextNode.Next; n != previousNode; n = ((n.Next != null) ? n.Next : remainingPositions.First))
                {
                    if (ContainmentTests.PointInsideTriangle(n.Value.Vector, p0, p1, p2))
                    {
                        isEar = false;
                        break;
                    }
                }

                if (isEar)
                {
                    indices.AddTriangle(new TriangleIndicesInt32(previousNode.Value.Index, node.Value.Index, nextNode.Value.Index));
                    remainingPositions.Remove(node);

                    previousNode = nextNode;
                    node = (previousNode.Next != null) ? previousNode.Next : remainingPositions.First;
                    nextNode = (node.Next != null) ? node.Next : remainingPositions.First;
                }
                else
                {
                    previousNode = (previousNode.Next != null) ? previousNode.Next : remainingPositions.First;
                    node = (node.Next != null) ? node.Next : remainingPositions.First;
                    nextNode = (nextNode.Next != null) ? nextNode.Next : remainingPositions.First;
                }
            }

            LinkedListNode<IndexedVector2D> n0 = remainingPositions.First;
            LinkedListNode<IndexedVector2D> n1 = n0.Next;
            LinkedListNode<IndexedVector2D> n2 = n1.Next;
            indices.AddTriangle(new TriangleIndicesInt32(n0.Value.Index, n1.Value.Index, n2.Value.Index));

            return indices;
        }
   }
}