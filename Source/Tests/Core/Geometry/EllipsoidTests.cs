#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using NUnit.Framework;

namespace OpenGlobe.Core
{
    [TestFixture]
    public class EllipsoidTests
    {
        [Test]
        public void Construct()
        {
            Ellipsoid ellipsoid = new Ellipsoid(new Vector3D(1, 2, 3));
            Assert.AreEqual(1, ellipsoid.Radii.X);
            Assert.AreEqual(2, ellipsoid.Radii.Y);
            Assert.AreEqual(3, ellipsoid.Radii.Z);

            Ellipsoid ellipsoid2 = new Ellipsoid(4, 5, 6);
            Assert.AreEqual(new Vector3D(4, 5, 6), ellipsoid2.Radii);

            Ellipsoid sphere = Ellipsoid.UnitSphere;
            Assert.IsTrue(sphere.OneOverRadii.Equals((new Vector3D(1, 1, 1))));
            Assert.IsTrue(sphere.OneOverRadiiSquared.Equals((new Vector3D(1, 1, 1))));
        }

        [Test]
        public void GeodeticSurfaceNormal()
        {
            Assert.IsTrue(new Vector3D(1, 0, 0).Equals(Ellipsoid.UnitSphere.GeodeticSurfaceNormal(new Vector3D(1, 0, 0))));
            Assert.IsTrue(new Vector3D(0, 0, 1).Equals(Ellipsoid.UnitSphere.GeodeticSurfaceNormal(new Vector3D(0, 0, 1))));
        }

        [Test]
        public void CentricSurfaceNormal()
        {
            Vector3D v = new Vector3D(1, 2, 3);
            Assert.AreEqual(v.Normalize(), Ellipsoid.CentricSurfaceNormal(v));
        }

        [Test]
        public void SphereIntersectionsTwoFromOutside()
        {
            Ellipsoid unitSphere = Ellipsoid.UnitSphere;
            
            double[] intersections = unitSphere.Intersections(new Vector3D(2.0, 0.0, 0.0), new Vector3D(-1.0, 0.0, 0.0));
            Assert.AreEqual(2, intersections.Length);
            Assert.AreEqual(1.0, intersections[0], 1e-14);
            Assert.AreEqual(3.0, intersections[1], 1e-14);

            intersections = unitSphere.Intersections(new Vector3D(0.0, 2.0, 0.0), new Vector3D(0.0, -1.0, 0.0));
            Assert.AreEqual(2, intersections.Length);
            Assert.AreEqual(1.0, intersections[0], 1e-14);
            Assert.AreEqual(3.0, intersections[1], 1e-14);

            intersections = unitSphere.Intersections(new Vector3D(0.0, 0.0, 2.0), new Vector3D(0.0, 0.0, -1.0));
            Assert.AreEqual(2, intersections.Length);
            Assert.AreEqual(1.0, intersections[0], 1e-14);
            Assert.AreEqual(3.0, intersections[1], 1e-14);

            intersections = unitSphere.Intersections(new Vector3D(1.0, 1.0, 0.0), new Vector3D(-1.0, 0.0, 0.0));
            Assert.AreEqual(1, intersections.Length);
            Assert.AreEqual(1.0, intersections[0], 1e-14);

            intersections = unitSphere.Intersections(new Vector3D(-2.0, 0.0, 0.0), new Vector3D(1.0, 0.0, 0.0));
            Assert.AreEqual(2, intersections.Length);
            Assert.AreEqual(1.0, intersections[0], 1e-14);
            Assert.AreEqual(3.0, intersections[1], 1e-14);

            intersections = unitSphere.Intersections(new Vector3D(0.0, -2.0, 0.0), new Vector3D(0.0, 1.0, 0.0));
            Assert.AreEqual(2, intersections.Length);
            Assert.AreEqual(1.0, intersections[0], 1e-14);
            Assert.AreEqual(3.0, intersections[1], 1e-14);

            intersections = unitSphere.Intersections(new Vector3D(0.0, 0.0, -2.0), new Vector3D(0.0, 0.0, 1.0));
            Assert.AreEqual(2, intersections.Length);
            Assert.AreEqual(1.0, intersections[0], 1e-14);
            Assert.AreEqual(3.0, intersections[1], 1e-14);

            intersections = unitSphere.Intersections(new Vector3D(-1.0, -1.0, 0.0), new Vector3D(1.0, 0.0, 0.0));
            Assert.AreEqual(1, intersections.Length);
            Assert.AreEqual(1.0, intersections[0], 1e-14);

            intersections = unitSphere.Intersections(new Vector3D(-2.0, 0.0, 0.0), new Vector3D(-1.0, 0.0, 0.0));
            Assert.AreEqual(2, intersections.Length);
            Assert.AreEqual(-3.0, intersections[0], 1e-14);
            Assert.AreEqual(-1.0, intersections[1], 1e-14);

            intersections = unitSphere.Intersections(new Vector3D(0.0, -2.0, 0.0), new Vector3D(0.0, -1.0, 0.0));
            Assert.AreEqual(2, intersections.Length);
            Assert.AreEqual(-3.0, intersections[0], 1e-14);
            Assert.AreEqual(-1.0, intersections[1], 1e-14);

            intersections = unitSphere.Intersections(new Vector3D(0.0, 0.0, -2.0), new Vector3D(0.0, 0.0, -1.0));
            Assert.AreEqual(2, intersections.Length);
            Assert.AreEqual(-3.0, intersections[0], 1e-14);
            Assert.AreEqual(-1.0, intersections[1], 1e-14);
        }

        [Test]
        public void SphereIntersectionsTwoFromInside()
        {
            Ellipsoid unitSphere = Ellipsoid.UnitSphere;

            double[] intersections;

            intersections = unitSphere.Intersections(new Vector3D(0.0, 0.0, 0.0), new Vector3D(0.0, 0.0, 1.0));
            Assert.AreEqual(2, intersections.Length);
            Assert.AreEqual(-1.0, intersections[0], 1e-14);
            Assert.AreEqual(1.0, intersections[1], 1e-14);

            intersections = unitSphere.Intersections(new Vector3D(0.0, 0.5, 0.0), new Vector3D(0.0, 1.0, 0.0));
            Assert.AreEqual(2, intersections.Length);
            Assert.AreEqual(-1.5, intersections[0], 1e-14);
            Assert.AreEqual(0.5, intersections[1], 1e-14);

            intersections = unitSphere.Intersections(new Vector3D(0.0, 0.5, 0.0), new Vector3D(0.0, -1.0, 0.0));
            Assert.AreEqual(2, intersections.Length);
            Assert.AreEqual(-0.5, intersections[0], 1e-14);
            Assert.AreEqual(1.5, intersections[1], 1e-14);
        }

        [Test]
        public void SphereIntersectionsFromEdge()
        {
            Ellipsoid unitSphere = Ellipsoid.UnitSphere;

            double[] intersections;

            intersections = unitSphere.Intersections(new Vector3D(1.0, 0.0, 0.0), new Vector3D(0.0, 0.0, 1.0));
            Assert.AreEqual(1, intersections.Length);
            Assert.AreEqual(0.0, intersections[0], 1e-14);

            intersections = unitSphere.Intersections(new Vector3D(1.0, 0.0, 0.0), new Vector3D(0.0, 1.0, 0.0));
            Assert.AreEqual(1, intersections.Length);
            Assert.AreEqual(0.0, intersections[0], 1e-14);

            intersections = unitSphere.Intersections(new Vector3D(1.0, 0.0, 0.0), new Vector3D(1.0, 0.0, 0.0));
            Assert.AreEqual(2, intersections.Length);
            Assert.AreEqual(-2.0, intersections[0], 1e-14);
            Assert.AreEqual(0.0, intersections[1], 1e-14);

            intersections = unitSphere.Intersections(new Vector3D(1.0, 0.0, 0.0), new Vector3D(0.0, 0.0, -1.0));
            Assert.AreEqual(1, intersections.Length);
            Assert.AreEqual(0.0, intersections[0], 1e-14);

            intersections = unitSphere.Intersections(new Vector3D(1.0, 0.0, 0.0), new Vector3D(0.0, -1.0, 0.0));
            Assert.AreEqual(1, intersections.Length);
            Assert.AreEqual(0.0, intersections[0], 1e-14);

            intersections = unitSphere.Intersections(new Vector3D(1.0, 0.0, 0.0), new Vector3D(-1.0, 0.0, 0.0));
            Assert.AreEqual(2, intersections.Length);
            Assert.AreEqual(0.0, intersections[0], 1e-14);
            Assert.AreEqual(2.0, intersections[1], 1e-14);
        }

        [Test]
        public void SphereIntersectionsNoIntersection()
        {
            Ellipsoid unitSphere = Ellipsoid.UnitSphere;

            double[] intersections;

            intersections = unitSphere.Intersections(new Vector3D(2.0, 0.0, 0.0), new Vector3D(0.0, 0.0, 1.0));
            Assert.AreEqual(0, intersections.Length);

            intersections = unitSphere.Intersections(new Vector3D(2.0, 0.0, 0.0), new Vector3D(0.0, 0.0, -1.0));
            Assert.AreEqual(0, intersections.Length);

            intersections = unitSphere.Intersections(new Vector3D(2.0, 0.0, 0.0), new Vector3D(0.0, 1.0, 0.0));
            Assert.AreEqual(0, intersections.Length);

            intersections = unitSphere.Intersections(new Vector3D(2.0, 0.0, 0.0), new Vector3D(0.0, -1.0, 0.0));
            Assert.AreEqual(0, intersections.Length);
        }

        [Test]
        public void ToVector3D()
        {
            Ellipsoid ellipsoid = new Ellipsoid(1, 1, 0.7);

            Assert.IsTrue(Vector3D.UnitX.EqualsEpsilon(ellipsoid.ToVector3D(new Geodetic3D(0, 0, 0)), 1e-10));
            Assert.IsTrue(Vector3D.UnitY.EqualsEpsilon(ellipsoid.ToVector3D(new Geodetic3D(Trig.ToRadians(90), 0, 0)), 1e-10));
            Assert.IsTrue(new Vector3D(0, 0, 0.7).EqualsEpsilon(ellipsoid.ToVector3D(new Geodetic3D(0, Trig.ToRadians(90), 0)), 1e-10));
        }

        [Test]
        public void ToGeodetic3D()
        {
            Ellipsoid ellipsoid = new Ellipsoid(6378137.0, 6378137.0, 6356752.314245);

            Vector3D v = ellipsoid.ToVector3D(new Geodetic3D(0, 0, 0));
            Geodetic3D g = ellipsoid.ToGeodetic3D(v);
            Assert.AreEqual(0.0, g.Longitude, 1e-10);
            Assert.AreEqual(0.0, g.Latitude, 1e-8);
            Assert.AreEqual(0.0, g.Height, 1e-10);

            v = ellipsoid.ToVector3D(new Geodetic3D(Trig.ToRadians(45.0), Trig.ToRadians(-60.0), -123.4));
            g = ellipsoid.ToGeodetic3D(v);
            Assert.AreEqual(Trig.ToRadians(45.0), g.Longitude, 1e-10);
            Assert.AreEqual(Trig.ToRadians(-60.0), g.Latitude, 1e-3);
            Assert.AreEqual(-123.4, g.Height, 1e-3);

            v = ellipsoid.ToVector3D(new Geodetic3D(Trig.ToRadians(-97.3), Trig.ToRadians(71.2), 1188.7));
            g = ellipsoid.ToGeodetic3D(v);
            Assert.AreEqual(Trig.ToRadians(-97.3), g.Longitude, 1e-10);
            Assert.AreEqual(Trig.ToRadians(71.2), g.Latitude, 1e-3);
            Assert.AreEqual(1188.7, g.Height, 1e-3);
        }
    }
}
