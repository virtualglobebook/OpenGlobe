#region License
//
// (C) Copyright 2010 Patrick Cozzi, Deron Ohlarik, and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0f.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using NUnit.Framework;
using System.Threading;
using System.Globalization;

namespace OpenGlobe.Core
{
    [TestFixture]
    public class Vector3HTests
    {
        [Test]
        public void Construct()
        {
            Vector3H v1 = new Vector3H(1.0f, 2.0f, 3.0f);
            Assert.AreEqual((Half)1.0f, v1.X);
            Assert.AreEqual((Half)2.0f, v1.Y);
            Assert.AreEqual((Half)3.0f, v1.Z);

            Vector3H v2 = new Vector3H(1.0, 2.0, 3.0);
            Assert.AreEqual((Half)1.0, v2.X);
            Assert.AreEqual((Half)2.0, v2.Y);
            Assert.AreEqual((Half)3.0, v2.Z);
        }

        [Test]
        public void Zero()
        {
            Assert.AreEqual((Half)0.0, Vector3H.Zero.X);
            Assert.AreEqual((Half)0.0, Vector3H.Zero.Y);
            Assert.AreEqual((Half)0.0, Vector3H.Zero.Z);
        }

        [Test]
        public void UnitX()
        {
            Assert.AreEqual((Half)1.0, Vector3H.UnitX.X);
            Assert.AreEqual((Half)0.0, Vector3H.UnitX.Y);
            Assert.AreEqual((Half)0.0, Vector3H.UnitX.Z);
        }

        [Test]
        public void UnitY()
        {
            Assert.AreEqual((Half)0.0, Vector3H.UnitY.X);
            Assert.AreEqual((Half)1.0, Vector3H.UnitY.Y);
            Assert.AreEqual((Half)0.0, Vector3H.UnitY.Z);
        }

        [Test]
        public void UnitZ()
        {
            Assert.AreEqual((Half)0.0, Vector3H.UnitZ.X);
            Assert.AreEqual((Half)0.0, Vector3H.UnitZ.Y);
            Assert.AreEqual((Half)1.0, Vector3H.UnitZ.Z);
        }

        [Test]
        public void Undefined()
        {
            Assert.IsNaN(Vector3H.Undefined.X);
            Assert.IsNaN(Vector3H.Undefined.Y);
            Assert.IsNaN(Vector3H.Undefined.Z);
        }

        [Test]
        public void IsUndefined()
        {
            Assert.IsTrue(Vector3H.Undefined.IsUndefined);
            Assert.IsFalse(Vector3H.UnitX.IsUndefined);
            Assert.IsFalse(Vector3H.UnitY.IsUndefined);
            Assert.IsFalse(Vector3H.UnitZ.IsUndefined);
        }

        [Test]
        public void TestEquals()
        {
            Vector3H a = new Vector3H(1.0f, 2.0f, 3.0f);
            Vector3H b = new Vector3H(4.0f, 5.0f, 6.0f);
            Vector3H c = new Vector3H(1.0f, 2.0f, 3.0f);

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
            Vector3H a = new Vector3H(1.0f, 2.0f, 3.0f);
            Vector3H b = new Vector3H(4.0f, 5.0f, 6.0f);
            Vector3H c = new Vector3H(1.0f, 2.0f, 3.0f);

            Assert.AreEqual(a.GetHashCode(), c.GetHashCode());
            Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());
        }

#if !CSToJava
        [Test]
        public void TestToString()
        {
            CultureInfo originalCulture = Thread.CurrentThread.CurrentCulture;
            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("tr-TR");
                Vector3H a = new Vector3H(1.23f, 2.34f, 3.45f);
                Assert.AreEqual("(1,230469, 2,339844, 3,449219)", a.ToString());
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = originalCulture;
            }
        }
#endif

        [Test]
        public void ToVector3S()
        {
            Vector3H a = new Vector3H(1.0, 2.0, 3.0);
            Vector3S sA = a.ToVector3S();
            Assert.AreEqual(1.0f, sA.X, 1e-7);
            Assert.AreEqual(2.0f, sA.Y, 1e-7);
            Assert.AreEqual(3.0f, sA.Z, 1e-7);
        }

        [Test]
        public void ToVector3D()
        {
            Vector3H a = new Vector3H(1.0, 2.0, 3.0);
            Vector3D sA = a.ToVector3D();
            Assert.AreEqual(1.0, sA.X, 1e-7);
            Assert.AreEqual(2.0, sA.Y, 1e-7);
            Assert.AreEqual(3.0, sA.Z, 1e-7);
        }

        [Test]
        public void ToVector3I()
        {
            Vector3H a = new Vector3H(1.0, 2.0, 3.0);
            Vector3I sA = a.ToVector3I();
            Assert.AreEqual(1, sA.X);
            Assert.AreEqual(2, sA.Y);
            Assert.AreEqual(3, sA.Z);
        }

        [Test]
        public void ToVector3B()
        {
            Vector3H a = new Vector3H(0, 1, 0);
            Vector3B sA = a.ToVector3B();
            Assert.IsFalse(sA.X);
            Assert.IsTrue(sA.Y);
            Assert.IsFalse(sA.Z);
        }

        [Test]
        public void XY()
        {
            Vector3H a = new Vector3H(1.0, 2.0, 3.0);
            Vector2H xy = a.XY;
            Assert.AreEqual((Half)1.0, xy.X);
            Assert.AreEqual((Half)2.0, xy.Y);
        }
    }
}
