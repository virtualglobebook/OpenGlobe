#region License
//
// (C) Copyright 2010 Patrick Cozzi, Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;
using NUnit.Framework;

namespace OpenGlobe.Core
{
    [TestFixture]
    public class ContainmentTestsTests
    {
        [Test]
        public void PointInsideTriangleInside()
        {
            Assert.IsTrue(ContainmentTests.PointInsideTriangle(
                new Vector2D(0.25, 0.25),
                Vector2D.Zero,
                new Vector2D(1, 0),
                new Vector2D(0, 1)));
        }

        [Test]
        public void PointInsideTriangleOutside()
        {
            Assert.IsFalse(ContainmentTests.PointInsideTriangle(
                new Vector2D(1, 1),
                Vector2D.Zero,
                new Vector2D(1, 0),
                new Vector2D(0, 1)));
        }

        [Test]
        public void PointInsideTriangleOutside2()
        {
            Assert.IsFalse(ContainmentTests.PointInsideTriangle(
                new Vector2D(0.5, -0.5),
                Vector2D.Zero,
                new Vector2D(1, 0),
                new Vector2D(0, 1)));
        }

        [Test]
        public void PointInsideTriangleOutside3()
        {
            Assert.IsFalse(ContainmentTests.PointInsideTriangle(
                new Vector2D(-0.5, 0.5),
                Vector2D.Zero,
                new Vector2D(1, 0),
                new Vector2D(0, 1)));
        }

        [Test]
        public void PointInsideTriangleOnCorner()
        {
            Assert.IsFalse(ContainmentTests.PointInsideTriangle(
                Vector2D.Zero,
                Vector2D.Zero,
                new Vector2D(1, 0),
                new Vector2D(0, 1)));
        }

        [Test]
        public void PointInsideTriangleOnEdge()
        {
            Assert.IsFalse(ContainmentTests.PointInsideTriangle(
                new Vector2D(0.5, 0),
                Vector2D.Zero,
                new Vector2D(1, 0),
                new Vector2D(0, 1)));
        }

        [Test]
        public void PointInsideThreeSidedInfinitePyramidInside()
        {
            Assert.IsTrue(ContainmentTests.PointInsideThreeSidedInfinitePyramid(
                new Vector3D(0, 0, 0.5),
                Vector3D.Zero,
                new Vector3D(1, -1, 1),
                new Vector3D(1, 1, 1),
                new Vector3D(-1, 0, 1)));
        }

        [Test]
        public void PointInsideThreeSidedInfinitePyramidOutside()
        {
            Assert.IsFalse(ContainmentTests.PointInsideThreeSidedInfinitePyramid(
                new Vector3D(2, 0, 0.5),
                Vector3D.Zero,
                new Vector3D(1, -1, 1),
                new Vector3D(1, 1, 1),
                new Vector3D(-1, 0, 1)));
        }

        [Test]
        public void PointInsideThreeSidedInfinitePyramidOutside2()
        {
            Assert.IsFalse(ContainmentTests.PointInsideThreeSidedInfinitePyramid(
                new Vector3D(0, 0, -0.5),
                Vector3D.Zero,
                new Vector3D(1, -1, 1),
                new Vector3D(1, 1, 1),
                new Vector3D(-1, 0, 1)));
        }

        [Test]
        public void PointInsideThreeSidedInfinitePyramidOnEdge()
        {
            Assert.IsFalse(ContainmentTests.PointInsideThreeSidedInfinitePyramid(
                new Vector3D(1, 1, 1),
                Vector3D.Zero,
                new Vector3D(1, -1, 1),
                new Vector3D(1, 1, 1),
                new Vector3D(-1, 0, 1)));
        }
    }
}
