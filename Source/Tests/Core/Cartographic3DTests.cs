#region License
//
// (C) Copyright 2010 Patrick Cozzi, Deron Ohlarik, and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using NUnit.Framework;

namespace MiniGlobe.Core
{
    [TestFixture]
    public class Cartographic3DTests
    {
        [Test]
        public void Construct()
        {
            Geodetic3D a = new Geodetic3D(1.0, 2.0, 3.0);
            Assert.AreEqual(1.0, a.Longitude);
            Assert.AreEqual(2.0, a.Latitude);
            Assert.AreEqual(3.0, a.Height);
        }

        [Test]
        public void TestEquals()
        {
            Geodetic3D a = new Geodetic3D(1.0, 2.0, 3.0);
            Geodetic3D b = new Geodetic3D(1.0, 2.0, 4.0);
            Geodetic3D c = new Geodetic3D(1.0, 3.0, 3.0);
            Geodetic3D d = new Geodetic3D(2.0, 2.0, 3.0);
            Geodetic3D e = new Geodetic3D(1.0, 2.0, 3.0);

            object objA = a;
            object objB = b;
            object objC = c;
            object objD = d;
            object objE = e;

            Assert.IsTrue(a.Equals(a));
            Assert.IsTrue(a.Equals(e));
            Assert.IsTrue(e.Equals(a));
            Assert.IsTrue(a.Equals(objA));
            Assert.IsTrue(a.Equals(objE));
            Assert.IsTrue(objA.Equals(objA));
            Assert.IsTrue(objA.Equals(objE));

            Assert.IsTrue(a == e);
            Assert.IsTrue(e == a);
            Assert.IsTrue(a != b);
            Assert.IsTrue(a != c);
            Assert.IsTrue(a != d);

            Assert.IsFalse(a.Equals(b));
            Assert.IsFalse(a.Equals(c));
            Assert.IsFalse(a.Equals(d));

            Assert.IsFalse(b.Equals(a));
            Assert.IsFalse(c.Equals(a));
            Assert.IsFalse(d.Equals(a));

            Assert.IsFalse(a.Equals(objB));
            Assert.IsFalse(a.Equals(objC));
            Assert.IsFalse(a.Equals(objD));

            Assert.IsFalse(objA.Equals(objB));
            Assert.IsFalse(objA.Equals(objC));
            Assert.IsFalse(objA.Equals(objD));

            Assert.IsFalse(a == b);
            Assert.IsFalse(a == c);
            Assert.IsFalse(a == d);
            Assert.IsFalse(a != e);

            Assert.IsFalse(a.Equals(null));
            Assert.IsFalse(a.Equals(5));
        }

        [Test]
        public void TestGetHashCode()
        {
            Geodetic3D a = new Geodetic3D(1.0, 2.0, 3.0);
            Geodetic3D b = new Geodetic3D(1.0, 2.0, 4.0);
            Geodetic3D c = new Geodetic3D(1.0, 3.0, 3.0);
            Geodetic3D d = new Geodetic3D(2.0, 2.0, 3.0);
            Geodetic3D e = new Geodetic3D(1.0, 2.0, 3.0);

            Assert.AreEqual(a.GetHashCode(), e.GetHashCode());
            Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());
            Assert.AreNotEqual(a.GetHashCode(), c.GetHashCode());
            Assert.AreNotEqual(a.GetHashCode(), d.GetHashCode());
        }
    }
}
