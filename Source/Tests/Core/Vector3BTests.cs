#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using NUnit.Framework;
using System.Globalization;
using System.Threading;

namespace OpenGlobe.Core
{
    [TestFixture]
    public class Vector3BTests
    {
        [Test]
        public void Construct0()
        {
            Vector3B v = new Vector3B(false, true, false);
            Assert.IsFalse(v.X);
            Assert.IsTrue(v.Y);
            Assert.IsFalse(v.Z);
        }

        [Test]
        public void Construct1()
        {
            Vector3B v = new Vector3B(new Vector2B(false, true), false);
            Assert.IsFalse(v.X);
            Assert.IsTrue(v.Y);
            Assert.IsFalse(v.Z);
        }

        [Test]
        public void False()
        {
            Assert.IsFalse(Vector3B.False.X);
            Assert.IsFalse(Vector3B.False.Y);
            Assert.IsFalse(Vector3B.False.Z);
        }

        [Test]
        public void True()
        {
            Assert.IsTrue(Vector3B.True.X);
            Assert.IsTrue(Vector3B.True.Y);
            Assert.IsTrue(Vector3B.True.Z);
        }

        [Test]
        public void TestEquals()
        {
            Vector3B a = new Vector3B(false, true, false);
            Vector3B b = new Vector3B(true, false, true);
            Vector3B c = new Vector3B(false, true, false);

            Assert.IsTrue(a.Equals(c));
            Assert.IsTrue(c.Equals(a));
            Assert.IsTrue(a == c);
            Assert.IsTrue(c == a);
            Assert.IsFalse(c != a);
            Assert.IsFalse(c != a);
            Assert.IsFalse(a.Equals(b));
            Assert.IsFalse(b.Equals(a));
            Assert.IsFalse(a == b);
            Assert.IsFalse(b == a);
            Assert.IsTrue(a != b);
            Assert.IsTrue(b != a);

            object objA = a;
            object objB = b;
            object objC = c;

            Assert.IsTrue(a.Equals(objA));
            Assert.IsTrue(a.Equals(objC));
            Assert.IsFalse(a.Equals(objB));

            Assert.IsTrue(objA.Equals(objC));
            Assert.IsFalse(objA.Equals(objB));

            Assert.IsFalse(a.Equals(null));
            Assert.IsFalse(a.Equals(5));
        }

        [Test]
        public void TestGetHashCode()
        {
            Vector3B a = new Vector3B(false, true, false);
            Vector3B b = new Vector3B(true, false, true);
            Vector3B c = new Vector3B(false, true, false);

            Assert.AreEqual(a.GetHashCode(), c.GetHashCode());
            Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());
        }


        [Test]
        public void ToVector3D()
        {
            Vector3B a = new Vector3B(false, true, false);
            Vector3D sA = a.ToVector3D();
            Assert.AreEqual(0.0, sA.X);
            Assert.AreEqual(1.0, sA.Y);
            Assert.AreEqual(0.0, sA.Z);
        }

        [Test]
        public void ToVector3S()
        {
            Vector3B a = new Vector3B(false, true, false);
            Vector3S sA = a.ToVector3S();
            Assert.AreEqual(0.0f, sA.X);
            Assert.AreEqual(1.0f, sA.Y);
            Assert.AreEqual(0.0f, sA.Z);
        }

        [Test]
        public void ToVector3I()
        {
            Vector3B a = new Vector3B(false, true, false);
            Vector3I sA = a.ToVector3I();
            Assert.AreEqual(0, sA.X);
            Assert.AreEqual(1, sA.Y);
            Assert.AreEqual(0, sA.Z);
        }

        [Test]
        public void ToVector3H()
        {
            Vector3B a = new Vector3B(false, true, false);
            Vector3H sA = a.ToVector3H();
            Assert.AreEqual((Half)0, sA.X);
            Assert.AreEqual((Half)1, sA.Y);
            Assert.AreEqual((Half)0, sA.Z);
        }

#if !CSToJava
        [Test]
        public void TestToString()
        {
            CultureInfo originalCulture = Thread.CurrentThread.CurrentCulture;
            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("tr-TR");
                Vector3B a = new Vector3B(false, true, false);
                Assert.AreEqual("(False, True, False)", a.ToString());
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = originalCulture;
            }
        }
#endif

        [Test]
        public void XY()
        {
            Vector3B a = new Vector3B(false, true, false);
            Vector2B xy = a.XY;
            Assert.IsFalse(xy.X);
            Assert.IsTrue(xy.Y);
        }
    }
}
