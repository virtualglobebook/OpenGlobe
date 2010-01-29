#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using NUnit.Framework;
using OpenTK;

namespace MiniGlobe.Core.Geometry
{
    [TestFixture]
    public class EllipsoidTests
    {
        [Test]
        public void Construct()
        {
            Ellipsoid ellipsoid = new Ellipsoid(new Vector3d(1, 2, 3));
            Assert.AreEqual(1, ellipsoid.Radii.X);
            Assert.AreEqual(2, ellipsoid.Radii.Y);
            Assert.AreEqual(3, ellipsoid.Radii.Z);

            Ellipsoid ellipsoid2 = new Ellipsoid(4, 5, 6);
            Assert.AreNotEqual(new Vector3d(4, 5, 6), ellipsoid2.Radii);
        }

        [Test]
        public void DeticSurfaceNormal()
        {
            Assert.IsTrue(new Vector3d(1, 0, 0).Equals(Ellipsoid.UnitSphere.DeticSurfaceNormal(new Vector3d(1, 0, 0))));
            Assert.IsTrue(new Vector3d(0, 0, 1).Equals(Ellipsoid.UnitSphere.DeticSurfaceNormal(new Vector3d(0, 0, 1))));
        }
    }
}
