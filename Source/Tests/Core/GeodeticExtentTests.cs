#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using NUnit.Framework;

namespace MiniGlobe.Core
{
    [TestFixture]
    public class GeodeticExtentTests
    {
        [Test]
        public void Construct()
        {
            GeodeticExtent extent = new GeodeticExtent(0, 1, 2, 3);
            Assert.AreEqual(extent.West, 0);
            Assert.AreEqual(extent.South, 1);
            Assert.AreEqual(extent.East, 2);
            Assert.AreEqual(extent.North, 3);

            GeodeticExtent extent2 = new GeodeticExtent(
                new Geodetic2D(0, 1), new Geodetic2D(2, 3));
            Assert.AreEqual(extent2.West, 0);
            Assert.AreEqual(extent2.South, 1);
            Assert.AreEqual(extent2.East, 2);
            Assert.AreEqual(extent2.North, 3);
        }

        [Test]
        public void Equals()
        {
            GeodeticExtent a = new GeodeticExtent(0, 1, 2, 3);
            GeodeticExtent b = new GeodeticExtent(4, 5, 6, 7);
            GeodeticExtent c = new GeodeticExtent(new Geodetic2D(4, 5), new Geodetic2D(6, 7));

            Assert.IsTrue(a != b);
            Assert.IsTrue(b == c);
        }
    }
}
