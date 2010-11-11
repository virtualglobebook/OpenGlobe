#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using NUnit.Framework;
using System.Globalization;
using System.Threading;

namespace OpenGlobe.Core
{
    [TestFixture]
    public class Vector4BTests
    {
        [Test]
        public void Construct0()
        {
            Vector4B v = new Vector4B(false, false, true, true);
            Assert.IsFalse(v.X);
            Assert.IsFalse(v.Y);
            Assert.IsTrue(v.Z);
            Assert.IsTrue(v.W);
        }

        [Test]
        public void Construct1()
        {
            Vector4B v = new Vector4B(new Vector3B(false, false, true), true);
            Assert.IsFalse(v.X);
            Assert.IsFalse(v.Y);
            Assert.IsTrue(v.Z);
            Assert.IsTrue(v.W);
        }

        [Test]
        public void Construct2()
        {
            Vector4B v = new Vector4B(new Vector2B(false, false), true, true);
            Assert.IsFalse(v.X);
            Assert.IsFalse(v.Y);
            Assert.IsTrue(v.Z);
            Assert.IsTrue(v.W);
        }

        [Test]
        public void False()
        {
            Assert.IsFalse(Vector4B.False.X);
            Assert.IsFalse(Vector4B.False.Y);
            Assert.IsFalse(Vector4B.False.Z);
            Assert.IsFalse(Vector4B.False.W);
        }

        [Test]
        public void True()
        {
            Assert.IsTrue(Vector4B.True.X);
            Assert.IsTrue(Vector4B.True.Y);
            Assert.IsTrue(Vector4B.True.Z);
            Assert.IsTrue(Vector4B.True.W);
        }

        [Test]
        public void TestEquals()
        {
            Vector4B a = new Vector4B(false, true, false, true);
            Vector4B b = new Vector4B(true, false, true, false);
            Vector4B c = new Vector4B(false, true, false, true);

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
            Vector4B a = new Vector4B(false, true, false, true);
            Vector4B b = new Vector4B(true, false, true, false);
            Vector4B c = new Vector4B(false, true, false, true);

            Assert.AreEqual(a.GetHashCode(), c.GetHashCode());
            Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());
        }

        [Test]
        public void ToVector4D()
        {
            Vector4B a = new Vector4B(false, false, true, true);
            Vector4D sA = a.ToVector4D();
            Assert.AreEqual(0.0, sA.X, 1e-7);
            Assert.AreEqual(0.0, sA.Y, 1e-7);
            Assert.AreEqual(1.0, sA.Z, 1e-7);
            Assert.AreEqual(1.0, sA.W, 1e-7);
        }

        [Test]
        public void ToVector4S()
        {
            Vector4B a = new Vector4B(false, false, true, true);
            Vector4S sA = a.ToVector4S();
            Assert.AreEqual(0.0f, sA.X, 1e-7);
            Assert.AreEqual(0.0f, sA.Y, 1e-7);
            Assert.AreEqual(1.0f, sA.Z, 1e-7);
            Assert.AreEqual(1.0f, sA.W, 1e-7);
        }

        [Test]
        public void ToVector4I()
        {
            Vector4B a = new Vector4B(false, false, true, true);
            Vector4I sA = a.ToVector4I();
            Assert.AreEqual(0, sA.X);
            Assert.AreEqual(0, sA.Y);
            Assert.AreEqual(1, sA.Z);
            Assert.AreEqual(1, sA.W);
        }

        [Test]
        public void ToVector4H()
        {
            Vector4B a = new Vector4B(false, false, true, true);
            Vector4H sA = a.ToVector4H();
            Assert.AreEqual((Half)0, sA.X);
            Assert.AreEqual((Half)0, sA.Y);
            Assert.AreEqual((Half)1, sA.Z);
            Assert.AreEqual((Half)1, sA.W);
        }

#if !CSToJava
        [Test]
        public void TestToString()
        {
            CultureInfo originalCulture = Thread.CurrentThread.CurrentCulture;
            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("tr-TR");
                Vector4B a = new Vector4B(false, true, false, true);
                Assert.AreEqual("(False, True, False, True)", a.ToString());
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
            Vector4B a = new Vector4B(false, true, false, true);
            Vector2B xy = a.XY;
            Assert.IsFalse(xy.X);
            Assert.IsTrue(xy.Y);
        }

        [Test]
        public void XYZ()
        {
            Vector4B a = new Vector4B(false, true, false, true);
            Vector3B xyz = a.XYZ;
            Assert.IsFalse(xyz.X);
            Assert.IsTrue(xyz.Y);
            Assert.IsFalse(xyz.Z);
        }
    }
}
