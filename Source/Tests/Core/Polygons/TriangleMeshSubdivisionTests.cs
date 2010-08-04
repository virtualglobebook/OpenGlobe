#region License
//
// (C) Copyright 2010 Patrick Cozzi, Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Collections.Generic;
using NUnit.Framework;
using OpenGlobe.Core.Geometry;

namespace OpenGlobe.Core
{
    [TestFixture]
    public class TriangleMeshSubdivisionTests
    {
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Null()
        {
            TriangleMeshSubdivision.Compute(null, new IndicesInt32());
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Null2()
        {
            TriangleMeshSubdivision.Compute( new Vector3D[] { }, null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void RangeException()
        {
            TriangleMeshSubdivision.Compute(new Vector3D[] { }, new IndicesInt32());
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void ArgumentException()
        {
            IndicesInt32 indices = new IndicesInt32();
            indices.Values.Add(0);
            indices.Values.Add(1);
            indices.Values.Add(2);
            indices.Values.Add(3);

            TriangleMeshSubdivision.Compute(new Vector3D[] { }, indices);
        }

        [Test]
        public void OneTriangle()
        {
            Vector3D[] positions = new Vector3D[]
            {
                Vector3D.Zero,
                new Vector3D(1, 0, 0),
                new Vector3D(1, 1, 0)
            };

            IndicesInt32 indices = new IndicesInt32();
            indices.Values.Add(0);
            indices.Values.Add(1);
            indices.Values.Add(2);

            TriangleMeshSubdivisionResult result = TriangleMeshSubdivision.Compute(positions, indices);

            Assert.AreEqual(0, result.Indices.Values[0]);
            Assert.AreEqual(1, result.Indices.Values[1]);
            Assert.AreEqual(2, result.Indices.Values[2]);
        }

        [Test]
        public void TwoTriangles()
        {
            Vector3D[] positions = new Vector3D[]
            {
                Vector3D.Zero,
                new Vector3D(1, 1, 0),
                new Vector3D(-1, 1, 0),
                new Vector3D(0, 2, 0)
            };

            IndicesInt32 indices = new IndicesInt32();
            indices.Values.Add(0);
            indices.Values.Add(1);
            indices.Values.Add(2);
            indices.Values.Add(3);
            indices.Values.Add(2);
            indices.Values.Add(1);

            TriangleMeshSubdivisionResult result = TriangleMeshSubdivision.Compute(positions, indices);

            Assert.AreEqual(0, result.Indices.Values[0]);
            Assert.AreEqual(1, result.Indices.Values[1]);
            Assert.AreEqual(2, result.Indices.Values[2]);

            Assert.AreEqual(3, result.Indices.Values[3]);
            Assert.AreEqual(2, result.Indices.Values[4]);
            Assert.AreEqual(1, result.Indices.Values[5]);
        }

        [Test]
        public void TwoTriangles2()
        {
            Vector3D[] positions = new Vector3D[]
            {
                Vector3D.Zero,
                new Vector3D(1, 1, 0),
                new Vector3D(-1, 1, 0),
                new Vector3D(0, 2, 0)
            };

            IndicesInt32 indices = new IndicesInt32();
            indices.Values.Add(1);
            indices.Values.Add(2);
            indices.Values.Add(0);
            indices.Values.Add(2);
            indices.Values.Add(1);
            indices.Values.Add(3);

            TriangleMeshSubdivisionResult result = TriangleMeshSubdivision.Compute(positions, indices);

            Assert.AreEqual(1, result.Indices.Values[0]);
            Assert.AreEqual(2, result.Indices.Values[1]);
            Assert.AreEqual(0, result.Indices.Values[2]);

            Assert.AreEqual(2, result.Indices.Values[3]);
            Assert.AreEqual(1, result.Indices.Values[4]);
            Assert.AreEqual(3, result.Indices.Values[5]);
        }
    }
}
