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
    public class EarClippingOnEllipsoidTests
    {
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Null()
        {
            EarClippingOnEllipsoid.Triangulate(null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Empty()
        {
            EarClippingOnEllipsoid.Triangulate(new List<Vector3D>());
        }

        [Test]
        public void Triangle()
        {
            IList<Vector3D> positions = new List<Vector3D>();
            positions.Add(Ellipsoid.UnitSphere.ToVector3D(Trig.ToRadians(new Geodetic3D(0, 0))));
            positions.Add(Ellipsoid.UnitSphere.ToVector3D(Trig.ToRadians(new Geodetic3D(1, 0))));
            positions.Add(Ellipsoid.UnitSphere.ToVector3D(Trig.ToRadians(new Geodetic3D(1, 1))));

            IndicesUnsignedInt indices = EarClippingOnEllipsoid.Triangulate(positions);

            Assert.AreEqual(3, indices.Values.Count);
            Assert.AreEqual(0, indices.Values[0]);
            Assert.AreEqual(1, indices.Values[1]);
            Assert.AreEqual(2, indices.Values[2]);
        }

        [Test]
        public void Square()
        {
            IList<Vector3D> positions = new List<Vector3D>();
            positions.Add(Ellipsoid.UnitSphere.ToVector3D(Trig.ToRadians(new Geodetic3D(0, 0))));
            positions.Add(Ellipsoid.UnitSphere.ToVector3D(Trig.ToRadians(new Geodetic3D(1, 0))));
            positions.Add(Ellipsoid.UnitSphere.ToVector3D(Trig.ToRadians(new Geodetic3D(1, 1))));
            positions.Add(Ellipsoid.UnitSphere.ToVector3D(Trig.ToRadians(new Geodetic3D(0, 1))));
            
            IndicesUnsignedInt indices = EarClippingOnEllipsoid.Triangulate(positions);

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
            IList<Vector3D> positions = new List<Vector3D>();

            positions.Add(Ellipsoid.UnitSphere.ToVector3D(Trig.ToRadians(new Geodetic3D(0, 0))));
            positions.Add(Ellipsoid.UnitSphere.ToVector3D(Trig.ToRadians(new Geodetic3D(2, 0))));
            positions.Add(Ellipsoid.UnitSphere.ToVector3D(Trig.ToRadians(new Geodetic3D(2, 2))));
            positions.Add(Ellipsoid.UnitSphere.ToVector3D(Trig.ToRadians(new Geodetic3D(1, 0.25))));
            positions.Add(Ellipsoid.UnitSphere.ToVector3D(Trig.ToRadians(new Geodetic3D(0, 2))));

            IndicesUnsignedInt indices = EarClippingOnEllipsoid.Triangulate(positions);

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
            IList<Vector3D> positions = new List<Vector3D>();

            positions.Add(Ellipsoid.UnitSphere.ToVector3D(Trig.ToRadians(new Geodetic3D(0, 0))));
            positions.Add(Ellipsoid.UnitSphere.ToVector3D(Trig.ToRadians(new Geodetic3D(2, 0))));
            positions.Add(Ellipsoid.UnitSphere.ToVector3D(Trig.ToRadians(new Geodetic3D(2, 1))));
            positions.Add(Ellipsoid.UnitSphere.ToVector3D(Trig.ToRadians(new Geodetic3D(0.1, 1.5))));
            positions.Add(Ellipsoid.UnitSphere.ToVector3D(Trig.ToRadians(new Geodetic3D(2, 2))));
            positions.Add(Ellipsoid.UnitSphere.ToVector3D(Trig.ToRadians(new Geodetic3D(0, 2))));
            positions.Add(Ellipsoid.UnitSphere.ToVector3D(Trig.ToRadians(new Geodetic3D(0, 1))));
            positions.Add(Ellipsoid.UnitSphere.ToVector3D(Trig.ToRadians(new Geodetic3D(1.9, 0.5))));

            IndicesUnsignedInt indices = EarClippingOnEllipsoid.Triangulate(positions);

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
