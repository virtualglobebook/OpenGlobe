#region License
//
// (C) Copyright 2010 Patrick Cozzi, Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using NUnit.Framework;

namespace OpenGlobe.Core
{
    [TestFixture]
    public class IntersectionTestsTests
    {
        [Test]
        public void RayPlaneParallel()
        {
            Vector3D intersectionPoint;
            Vector3D normal = Vector3D.UnitY;
            Assert.IsFalse(IntersectionTests.TryRayPlane(Vector3D.Zero, Vector3D.UnitX,
                normal, planeD(Vector3D.UnitY, normal), out intersectionPoint));
        }

        [Test]
        public void RayPlaneOppositeDirection()
        {
            Vector3D intersectionPoint;
            Vector3D normal = -Vector3D.UnitX;
            Assert.IsFalse(IntersectionTests.TryRayPlane(Vector3D.Zero, Vector3D.UnitX,
                normal, planeD(-Vector3D.UnitX, normal), out intersectionPoint));
        }

        [Test]
        public void RayPlaneOppositeDirection2()
        {
            Vector3D intersectionPoint;
            Vector3D normal = Vector3D.UnitX;
            Assert.IsFalse(IntersectionTests.TryRayPlane(Vector3D.Zero, Vector3D.UnitX,
                normal, planeD(-Vector3D.UnitX, normal), out intersectionPoint));
        }

        [Test]
        public void RayPlaneIntersects()
        {
            Vector3D intersectionPoint;
            Vector3D normal = Vector3D.UnitX;
            Assert.IsTrue(IntersectionTests.TryRayPlane(Vector3D.Zero, Vector3D.UnitX,
                normal, planeD(Vector3D.UnitX, normal), out intersectionPoint));
        }

        // TODO:  where to put this?
        private static double planeD(Vector3D origin, Vector3D normal)
        {
            return -origin.Dot(normal);
        }
    }
}
