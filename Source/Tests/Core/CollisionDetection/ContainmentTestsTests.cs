#region License
//
// (C) Copyright 2010 Patrick Cozzi, Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
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
        public void Inside()
        {
            Assert.IsTrue(ContainmentTests.PointInsideTriangle(
                new Vector2D(0.25, 0.25),
                new Vector2D(0, 0),
                new Vector2D(1, 0),
                new Vector2D(0, 1)));
        }

        [Test]
        public void Outside()
        {
            Assert.IsFalse(ContainmentTests.PointInsideTriangle(
                new Vector2D(1, 1),
                new Vector2D(0, 0),
                new Vector2D(1, 0),
                new Vector2D(0, 1)));
        }

        [Test]
        public void Outside2()
        {
            Assert.IsFalse(ContainmentTests.PointInsideTriangle(
                new Vector2D(0.5, -0.5),
                new Vector2D(0, 0),
                new Vector2D(1, 0),
                new Vector2D(0, 1)));
        }

        [Test]
        public void Outside3()
        {
            Assert.IsFalse(ContainmentTests.PointInsideTriangle(
                new Vector2D(-0.5, 0.5),
                new Vector2D(0, 0),
                new Vector2D(1, 0),
                new Vector2D(0, 1)));
        }

        [Test]
        public void OnCorner()
        {
            Assert.IsTrue(ContainmentTests.PointInsideTriangle(
                new Vector2D(0, 0),
                new Vector2D(0, 0),
                new Vector2D(1, 0),
                new Vector2D(0, 1)));
        }

        [Test]
        public void OnEdge()
        {
            Assert.IsTrue(ContainmentTests.PointInsideTriangle(
                new Vector2D(0.5, 0),
                new Vector2D(0, 0),
                new Vector2D(1, 0),
                new Vector2D(0, 1)));
        }
    }
}
