#region License
//
// (C) Copyright 2010 Patrick Cozzi, Deron Ohlarik, and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using NUnit.Framework;

namespace OpenGlobe.Core
{
    [TestFixture]
    public class Geodetic2DTests
    {
        [Test]
        public void Construct()
        {
            Geodetic2D a = new Geodetic2D(1.0, 2.0);
            Assert.AreEqual(1.0, a.Longitude);
            Assert.AreEqual(2.0, a.Latitude);
        }

        [Test]
        public void TestEquals()
        {
            Geodetic2D a = new Geodetic2D(1.0, 2.0);
            Geodetic2D b = new Geodetic2D(2.0, 3.0);
            Geodetic2D c = new Geodetic2D(2.0, 3.0);

            object objA = a;
            object objB = b;
            object objC = c;

            Assert.IsTrue(a.Equals(a));
            Assert.IsTrue(b.Equals(c));
            Assert.IsTrue(a.Equals(objA));
            Assert.IsTrue(b.Equals(objC));
            Assert.IsTrue(objA.Equals(objA));
            Assert.IsTrue(objB.Equals(objC));

            Assert.IsTrue(b == c);
            Assert.IsTrue(c == b);
            Assert.IsTrue(a != b);

            Assert.IsFalse(a.Equals(b));
            Assert.IsFalse(a.Equals(c));

            Assert.IsFalse(a.Equals(objB));
            Assert.IsFalse(a.Equals(objC));

            Assert.IsFalse(objA.Equals(objB));
            Assert.IsFalse(objA.Equals(objC));

            Assert.IsFalse(a == b);
            Assert.IsFalse(a == c);
            Assert.IsFalse(b != c);

            Assert.IsFalse(a.Equals(null));
            Assert.IsFalse(a.Equals(5));
        }

        [Test]
        public void TestGetHashCode()
        {
            Geodetic2D a = new Geodetic2D(1.0, 2.0);
            Geodetic2D b = new Geodetic2D(2.0, 3.0);
            Geodetic2D c = new Geodetic2D(2.0, 3.0);

            Assert.AreEqual(b.GetHashCode(), c.GetHashCode());
            Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());
            Assert.AreNotEqual(a.GetHashCode(), c.GetHashCode());
        }
    }
}
