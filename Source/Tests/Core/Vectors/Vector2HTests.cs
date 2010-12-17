#region License
//
// (C) Copyright 2010 Patrick Cozzi, Deron Ohlarik, and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0f.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using NUnit.Framework;
using System.Threading;
using System.Globalization;

namespace OpenGlobe.Core
{
    [TestFixture]
    public class Vector2HTests
    {
        [Test]
        public void Construct()
        {
            Vector2H v1 = new Vector2H(1.0f, 2.0f);
            Assert.AreEqual((Half)1.0f, v1.X);
            Assert.AreEqual((Half)2.0f, v1.Y);

            Vector2H v2 = new Vector2H(1.0, 2.0);
            Assert.AreEqual((Half)1.0, v2.X);
            Assert.AreEqual((Half)2.0, v2.Y);
        }

        [Test]
        public void Zero()
        {
            Assert.AreEqual((Half)0.0, Vector2H.Zero.X);
            Assert.AreEqual((Half)0.0, Vector2H.Zero.Y);
        }

        [Test]
        public void UnitX()
        {
            Assert.AreEqual((Half)1.0, Vector2H.UnitX.X);
            Assert.AreEqual((Half)0.0, Vector2H.UnitX.Y);
        }

        [Test]
        public void UnitY()
        {
            Assert.AreEqual((Half)0.0, Vector2H.UnitY.X);
            Assert.AreEqual((Half)1.0, Vector2H.UnitY.Y);
        }

        [Test]
        public void Undefined()
        {
            Assert.IsNaN(Vector2H.Undefined.X);
            Assert.IsNaN(Vector2H.Undefined.Y);
        }

        [Test]
        public void IsUndefined()
        {
            Assert.IsTrue(Vector2H.Undefined.IsUndefined);
            Assert.IsFalse(Vector2H.UnitX.IsUndefined);
            Assert.IsFalse(Vector2H.UnitY.IsUndefined);
        }

        [Test]
        public void TestEquals()
        {
            Vector2H a = new Vector2H(1.0f, 2.0f);
            Vector2H b = new Vector2H(4.0f, 5.0f);
            Vector2H c = new Vector2H(1.0f, 2.0f);

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
            Vector2H a = new Vector2H(1.0f, 2.0f);
            Vector2H b = new Vector2H(4.0f, 5.0f);
            Vector2H c = new Vector2H(1.0f, 2.0f);

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
                Vector2H a = new Vector2H(1.23f, 2.34f);
                Assert.AreEqual("(1,230469, 2,339844)", a.ToString());
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = originalCulture;
            }
        }
#endif

        [Test]
        public void ToVector2F()
        {
            Vector2H a = new Vector2H(1.0, 2.0);
            Vector2F sA = a.ToVector2F();
            Assert.AreEqual(1.0f, sA.X, 1e-7);
            Assert.AreEqual(2.0f, sA.Y, 1e-7);
        }

        [Test]
        public void ToVector2D()
        {
            Vector2H a = new Vector2H(1.0, 2.0);
            Vector2D sA = a.ToVector2D();
            Assert.AreEqual(1.0, sA.X, 1e-7);
            Assert.AreEqual(2.0, sA.Y, 1e-7);
        }

        [Test]
        public void ToVector2I()
        {
            Vector2H a = new Vector2H(1.0, 2.0);
            Vector2I sA = a.ToVector2I();
            Assert.AreEqual(1, sA.X);
            Assert.AreEqual(2, sA.Y);
        }

        [Test]
        public void ToVector2B()
        {
            Vector2H a = new Vector2H(1.0, 0.0);
            Vector2B sA = a.ToVector2B();
            Assert.IsTrue(sA.X);
            Assert.IsFalse(sA.Y);
        }
    }
}
