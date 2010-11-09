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
    public class Vector2ITests
    {
        [Test]
        public void Construct()
        {
            Vector2I v = new Vector2I(1, 2);
            Assert.AreEqual(1, v.X);
            Assert.AreEqual(2, v.Y);
        }

        [Test]
        public void Add()
        {
            Vector2I v1 = new Vector2I(1, 2);
            Vector2I v2 = new Vector2I(4, 5);

            Vector2I v3 = v1 + v2;
            Assert.AreEqual(5, v3.X);
            Assert.AreEqual(7, v3.Y);

            Vector2I v4 = v1.Add(v2);
            Assert.AreEqual(5, v4.X);
            Assert.AreEqual(7, v4.Y);
        }

        [Test]
        public void Subtract()
        {
            Vector2I v1 = new Vector2I(1, 2);
            Vector2I v2 = new Vector2I(4, 5);

            Vector2I v3 = v1 - v2;
            Assert.AreEqual(-3, v3.X);
            Assert.AreEqual(-3, v3.Y);

            Vector2I v4 = v1.Subtract(v2);
            Assert.AreEqual(-3, v4.X);
            Assert.AreEqual(-3, v4.Y);
        }

        [Test]
        public void Multiply()
        {
            Vector2I v1 = new Vector2I(1, 2);

            Vector2I v2 = v1 * 5;
            Assert.AreEqual(5, v2.X);
            Assert.AreEqual(10, v2.Y);

            Vector2I v3 = 5 * v1;
            Assert.AreEqual(5, v3.X);
            Assert.AreEqual(10, v3.Y);

            Vector2I v4 = v1.Multiply(5);
            Assert.AreEqual(5, v4.X);
            Assert.AreEqual(10, v4.Y);
        }

        [Test]
        public void Zero()
        {
            Assert.AreEqual(0, Vector2I.Zero.X);
            Assert.AreEqual(0, Vector2I.Zero.Y);
        }

        [Test]
        public void UnitX()
        {
            Assert.AreEqual(1, Vector2I.UnitX.X);
            Assert.AreEqual(0, Vector2I.UnitX.Y);
        }

        [Test]
        public void UnitY()
        {
            Assert.AreEqual(0, Vector2I.UnitY.X);
            Assert.AreEqual(1, Vector2I.UnitY.Y);
        }

        [Test]
        public void TestEquals()
        {
            Vector2I a = new Vector2I(1, 2);
            Vector2I b = new Vector2I(4, 5);
            Vector2I c = new Vector2I(1, 2);

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
            Vector2I a = new Vector2I(1, 2);
            Vector2I b = new Vector2I(4, 5);
            Vector2I c = new Vector2I(1, 2);

            Assert.AreEqual(a.GetHashCode(), c.GetHashCode());
            Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());
        }

        [Test]
        public void Negate()
        {
            Vector2I a = new Vector2I(1, 2);
            Vector2I negatedA1 = a.Negate();
            Assert.AreEqual(-1, negatedA1.X, 1e-14);
            Assert.AreEqual(-2, negatedA1.Y, 1e-14);
            Vector2I negatedA2 = -a;
            Assert.AreEqual(-1, negatedA2.X, 1e-14);
            Assert.AreEqual(-2, negatedA2.Y, 1e-14);

            Vector2I b = new Vector2I(-1, -2);
            Vector2I negatedB1 = b.Negate();
            Assert.AreEqual(1, negatedB1.X, 1e-14);
            Assert.AreEqual(2, negatedB1.Y, 1e-14);
            Vector2I negatedB2 = -b;
            Assert.AreEqual(1, negatedB2.X, 1e-14);
            Assert.AreEqual(2, negatedB2.Y, 1e-14);
        }

        [Test]
        public void ToVector2D()
        {
            Vector2I a = new Vector2I(1, 2);
            Vector2D sA = a.ToVector2D();
            Assert.AreEqual(1.0, sA.X, 1e-7);
            Assert.AreEqual(2.0, sA.Y, 1e-7);
        }

        [Test]
        public void ToVector2S()
        {
            Vector2I a = new Vector2I(1, 2);
            Vector2S sA = a.ToVector2S();
            Assert.AreEqual(1.0f, sA.X, 1e-7);
            Assert.AreEqual(2.0f, sA.Y, 1e-7);
        }

        [Test]
        public void ToVector2H()
        {
            Vector2I a = new Vector2I(1, 2);
            Vector2H sA = a.ToVector2H();
            Assert.AreEqual((Half)1, sA.X, 1e-7);
            Assert.AreEqual((Half)2, sA.Y, 1e-7);
        }

        [Test]
        public void ToVector2B()
        {
            Vector2I a = new Vector2I(0, 1);
            Vector2B sA = a.ToVector2B();
            Assert.IsFalse(sA.X);
            Assert.IsTrue(sA.Y);
        }

#if !CSToJava
        [Test]
        public void TestToString()
        {
            CultureInfo originalCulture = Thread.CurrentThread.CurrentCulture;
            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("tr-TR");
                Vector2I a = new Vector2I(1, 2);
                Assert.AreEqual("(1, 2)", a.ToString());
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = originalCulture;
            }
        }
#endif
    }
}
