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
    public class Vector2BTests
    {
        [Test]
        public void Construct()
        {
            Vector2B v = new Vector2B(false, true);
            Assert.IsFalse(v.X);
            Assert.IsTrue(v.Y);
        }

        [Test]
        public void False()
        {
            Assert.IsFalse(Vector2B.False.X);
            Assert.IsFalse(Vector2B.False.Y);
        }

        [Test]
        public void True()
        {
            Assert.IsTrue(Vector2B.True.X);
            Assert.IsTrue(Vector2B.True.Y);
        }

        [Test]
        public void TestEquals()
        {
            Vector2B a = new Vector2B(false, true);
            Vector2B b = new Vector2B(true, false);
            Vector2B c = new Vector2B(false, true);

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
            Vector2B a = new Vector2B(false, true);
            Vector2B b = new Vector2B(true, false);
            Vector2B c = new Vector2B(false, true);

            Assert.AreEqual(a.GetHashCode(), c.GetHashCode());
            Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());
        }

        [Test]
        public void ToVector2D()
        {
            Vector2B a = new Vector2B(false, true);
            Vector2D sA = a.ToVector2D();
            Assert.AreEqual(0.0, sA.X, 1e-7);
            Assert.AreEqual(1.0, sA.Y, 1e-7);
        }

        [Test]
        public void ToVector2S()
        {
            Vector2B a = new Vector2B(false, true);
            Vector2S sA = a.ToVector2S();
            Assert.AreEqual(0.0f, sA.X, 1e-7);
            Assert.AreEqual(1.0f, sA.Y, 1e-7);
        }

        [Test]
        public void ToVector2I()
        {
            Vector2B a = new Vector2B(false, true);
            Vector2I sA = a.ToVector2I();
            Assert.AreEqual(0, sA.X);
            Assert.AreEqual(1, sA.Y);
        }

        [Test]
        public void ToVector2H()
        {
            Vector2B a = new Vector2B(false, true);
            Vector2H sA = a.ToVector2H();
            Assert.AreEqual((Half)0.0, sA.X, 1e-7);
            Assert.AreEqual((Half)1.0, sA.Y, 1e-7);
        }

#if !CSToJava
        [Test]
        public void TestToString()
        {
            CultureInfo originalCulture = Thread.CurrentThread.CurrentCulture;
            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("tr-TR");
                Vector2B a = new Vector2B(false, true);
                Assert.AreEqual("(False, True)", a.ToString());
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = originalCulture;
            }
        }
#endif
    }
}
