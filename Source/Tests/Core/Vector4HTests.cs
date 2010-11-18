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
    public class Vector4HTests
    {
        [Test]
        public void Construct0()
        {
            Vector4H v1 = new Vector4H(1.0f, 2.0f, 3.0f, 4.0f);
            Assert.AreEqual((Half)1.0f, v1.X);
            Assert.AreEqual((Half)2.0f, v1.Y);
            Assert.AreEqual((Half)3.0f, v1.Z);
            Assert.AreEqual((Half)4.0f, v1.W);

            Vector4H v2 = new Vector4H(1.0, 2.0, 3.0, 4.0);
            Assert.AreEqual((Half)1.0, v2.X);
            Assert.AreEqual((Half)2.0, v2.Y);
            Assert.AreEqual((Half)3.0, v2.Z);
            Assert.AreEqual((Half)4.0, v2.W);
        }

        [Test]
        public void Construct1()
        {
            Vector4H v = new Vector4H(new Vector3H(1.0, 2.0, 3.0), (Half)4.0);
            Assert.AreEqual((Half)1.0, v.X);
            Assert.AreEqual((Half)2.0, v.Y);
            Assert.AreEqual((Half)3.0, v.Z);
            Assert.AreEqual((Half)4.0, v.W);
        }

        [Test]
        public void Construct2()
        {
            Vector4H v = new Vector4H(new Vector2H(1.0, 2.0), (Half)3.0, (Half)4.0);
            Assert.AreEqual((Half)1.0, v.X);
            Assert.AreEqual((Half)2.0, v.Y);
            Assert.AreEqual((Half)3.0, v.Z);
            Assert.AreEqual((Half)4.0, v.W);
        }

        [Test]
        public void Zero()
        {
            Assert.AreEqual((Half)0.0, Vector4H.Zero.X);
            Assert.AreEqual((Half)0.0, Vector4H.Zero.Y);
            Assert.AreEqual((Half)0.0, Vector4H.Zero.Z);
            Assert.AreEqual((Half)0.0, Vector4H.Zero.W);
        }

        [Test]
        public void UnitX()
        {
            Assert.AreEqual((Half)1.0, Vector4H.UnitX.X);
            Assert.AreEqual((Half)0.0, Vector4H.UnitX.Y);
            Assert.AreEqual((Half)0.0, Vector4H.UnitX.Z);
            Assert.AreEqual((Half)0.0, Vector4H.UnitX.W);
        }

        [Test]
        public void UnitY()
        {
            Assert.AreEqual((Half)0.0, Vector4H.UnitY.X);
            Assert.AreEqual((Half)1.0, Vector4H.UnitY.Y);
            Assert.AreEqual((Half)0.0, Vector4H.UnitY.Z);
            Assert.AreEqual((Half)0.0, Vector4H.UnitY.W);
        }

        [Test]
        public void UnitZ()
        {
            Assert.AreEqual((Half)0.0, Vector4H.UnitZ.X);
            Assert.AreEqual((Half)0.0, Vector4H.UnitZ.Y);
            Assert.AreEqual((Half)1.0, Vector4H.UnitZ.Z);
            Assert.AreEqual((Half)0.0, Vector4H.UnitZ.W);
        }

        [Test]
        public void UnitW()
        {
            Assert.AreEqual((Half)0.0, Vector4H.UnitW.X);
            Assert.AreEqual((Half)0.0, Vector4H.UnitW.Y);
            Assert.AreEqual((Half)0.0, Vector4H.UnitW.Z);
            Assert.AreEqual((Half)1.0, Vector4H.UnitW.W);
        }

        [Test]
        public void Undefined()
        {
            Assert.IsNaN(Vector4H.Undefined.X);
            Assert.IsNaN(Vector4H.Undefined.Y);
            Assert.IsNaN(Vector4H.Undefined.Z);
            Assert.IsNaN(Vector4H.Undefined.W);
        }

        [Test]
        public void IsUndefined()
        {
            Assert.IsTrue(Vector4H.Undefined.IsUndefined);
            Assert.IsFalse(Vector4H.UnitX.IsUndefined);
            Assert.IsFalse(Vector4H.UnitY.IsUndefined);
            Assert.IsFalse(Vector4H.UnitZ.IsUndefined);
            Assert.IsFalse(Vector4H.UnitW.IsUndefined);
        }

        [Test]
        public void TestEquals()
        {
            Vector4H a = new Vector4H(1.0f, 2.0f, 3.0f, 4.0f);
            Vector4H b = new Vector4H(4.0f, 5.0f, 6.0f, 7.0f);
            Vector4H c = new Vector4H(1.0f, 2.0f, 3.0f, 4.0f);

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
            Vector4H a = new Vector4H(1.0f, 2.0f, 3.0f, 4.0f);
            Vector4H b = new Vector4H(4.0f, 5.0f, 6.0f, 7.0f);
            Vector4H c = new Vector4H(1.0f, 2.0f, 3.0f, 4.0f);

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
                Vector4H a = new Vector4H(1.23f, 2.34f, 3.45f, 4.56f);
                Assert.AreEqual("(1,230469, 2,339844, 3,449219, 4,558594)", a.ToString());
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = originalCulture;
            }
        }
#endif

        [Test]
        public void ToVector4F()
        {
            Vector4H a = new Vector4H(1.0, 2.0, 3.0, 4.0);
            Vector4F sA = a.ToVector4F();
            Assert.AreEqual(1.0f, sA.X, 1e-7);
            Assert.AreEqual(2.0f, sA.Y, 1e-7);
            Assert.AreEqual(3.0f, sA.Z, 1e-7);
            Assert.AreEqual(4.0f, sA.W, 1e-7);
        }

        [Test]
        public void ToVector4D()
        {
            Vector4H a = new Vector4H(1.0, 2.0, 3.0, 4.0);
            Vector4D sA = a.ToVector4D();
            Assert.AreEqual(1.0, sA.X, 1e-7);
            Assert.AreEqual(2.0, sA.Y, 1e-7);
            Assert.AreEqual(3.0, sA.Z, 1e-7);
            Assert.AreEqual(4.0, sA.W, 1e-7);
        }

        [Test]
        public void ToVector4I()
        {
            Vector4H a = new Vector4H(1.0, 2.0, 3.0, 4.0);
            Vector4I sA = a.ToVector4I();
            Assert.AreEqual(1, sA.X);
            Assert.AreEqual(2, sA.Y);
            Assert.AreEqual(3, sA.Z);
            Assert.AreEqual(4, sA.W);
        }

        [Test]
        public void ToVector4B()
        {
            Vector4H a = new Vector4H(0.0, 1.0, 1.0, 0.0);
            Vector4B sA = a.ToVector4B();
            Assert.IsFalse(sA.X);
            Assert.IsTrue(sA.Y);
            Assert.IsTrue(sA.Z);
            Assert.IsFalse(sA.W);
        }

        [Test]
        public void XY()
        {
            Vector4H a = new Vector4H(1.0, 2.0, 3.0, 4.0);
            Vector2H xy = a.XY;
            Assert.AreEqual((Half)1.0, xy.X);
            Assert.AreEqual((Half)2.0, xy.Y);
        }

        [Test]
        public void XYZ()
        {
            Vector4H a = new Vector4H(1.0, 2.0, 3.0, 4.0);
            Vector3H xyz = a.XYZ;
            Assert.AreEqual((Half)1.0, xyz.X);
            Assert.AreEqual((Half)2.0, xyz.Y);
            Assert.AreEqual((Half)3.0, xyz.Z);
        }
    }
}
