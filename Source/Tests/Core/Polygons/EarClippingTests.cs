#region License
//
// (C) Copyright 2010 Patrick Cozzi, Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace OpenGlobe.Core
{
    [TestFixture]
    public class EarClippingTests
    {
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Null()
        {
            EarClipping.Triangulate(null);
        }
        
        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Empty()
        {
            EarClipping.Triangulate(new List<Vector2D>());
        }

        [Test]
        public void Triangle()
        {
            IList<Vector2D> positions = new List<Vector2D>();
            positions.Add(new Vector2D(0, 0));
            positions.Add(new Vector2D(1, 0));
            positions.Add(new Vector2D(1, 1));

            IndicesUnsignedInt indices = EarClipping.Triangulate(positions);

            Assert.AreEqual(3, indices.Values.Count);
            Assert.AreEqual(0, indices.Values[0]);
            Assert.AreEqual(1, indices.Values[1]);
            Assert.AreEqual(2, indices.Values[2]);
        }

        [Test]
        public void Square()
        {
            IList<Vector2D> positions = new List<Vector2D>();
            positions.Add(new Vector2D(0, 0));
            positions.Add(new Vector2D(1, 0));
            positions.Add(new Vector2D(1, 1));
            positions.Add(new Vector2D(0, 1));

            IndicesUnsignedInt indices = EarClipping.Triangulate(positions);

            Assert.AreEqual(6, indices.Values.Count);
            Assert.AreEqual(0, indices.Values[0]);
            Assert.AreEqual(1, indices.Values[1]);
            Assert.AreEqual(2, indices.Values[2]);
            Assert.AreEqual(0, indices.Values[3]);
            Assert.AreEqual(2, indices.Values[4]);
            Assert.AreEqual(3, indices.Values[5]);
        }

        [Test]
        public void SimpleConcave()
        {
            IList<Vector2D> positions = new List<Vector2D>();

            positions.Add(new Vector2D(0, 0));
            positions.Add(new Vector2D(2, 0));
            positions.Add(new Vector2D(2, 2));
            positions.Add(new Vector2D(1, 0.25));
            positions.Add(new Vector2D(0, 2));

            IndicesUnsignedInt indices = EarClipping.Triangulate(positions);

            Assert.AreEqual(9, indices.Values.Count);
            Assert.AreEqual(1, indices.Values[0]);
            Assert.AreEqual(2, indices.Values[1]);
            Assert.AreEqual(3, indices.Values[2]);

            Assert.AreEqual(3, indices.Values[3]);
            Assert.AreEqual(4, indices.Values[4]);
            Assert.AreEqual(0, indices.Values[5]);

            Assert.AreEqual(0, indices.Values[6]);
            Assert.AreEqual(1, indices.Values[7]);
            Assert.AreEqual(3, indices.Values[8]);
        }

        [Test]
        public void ComplexConcave()
        {
            IList<Vector2D> positions = new List<Vector2D>();

            positions.Add(new Vector2D(0, 0));
            positions.Add(new Vector2D(2, 0));
            positions.Add(new Vector2D(2, 1));
            positions.Add(new Vector2D(0.1, 1.5));
            positions.Add(new Vector2D(2, 2));
            positions.Add(new Vector2D(0, 2));
            positions.Add(new Vector2D(0, 1));
            positions.Add(new Vector2D(1.9, 0.5));

            IndicesUnsignedInt indices = EarClipping.Triangulate(positions);

            Assert.AreEqual(18, indices.Values.Count);
            Assert.AreEqual(3, indices.Values[0]);
            Assert.AreEqual(4, indices.Values[1]);
            Assert.AreEqual(5, indices.Values[2]);

            Assert.AreEqual(3, indices.Values[3]);
            Assert.AreEqual(5, indices.Values[4]);
            Assert.AreEqual(6, indices.Values[5]);

            Assert.AreEqual(3, indices.Values[6]);
            Assert.AreEqual(6, indices.Values[7]);
            Assert.AreEqual(7, indices.Values[8]);

            Assert.AreEqual(7, indices.Values[9]);
            Assert.AreEqual(0, indices.Values[10]);
            Assert.AreEqual(1, indices.Values[11]);

            Assert.AreEqual(7, indices.Values[12]);
            Assert.AreEqual(1, indices.Values[13]);
            Assert.AreEqual(2, indices.Values[14]);

            Assert.AreEqual(2, indices.Values[15]);
            Assert.AreEqual(3, indices.Values[16]);
            Assert.AreEqual(7, indices.Values[17]);
        }
    }
}
