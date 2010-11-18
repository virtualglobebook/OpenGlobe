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
    public class Vector2FTests
    {
        [Test]
        public void Construct()
        {
            Vector2F v = new Vector2F(1.0f, 2.0f);
            Assert.AreEqual(1.0f, v.X);
            Assert.AreEqual(2.0f, v.Y);
        }

        [Test]
        public void Magnitude()
        {
            Vector2F v = new Vector2F(3.0f, 4.0f);
            Assert.AreEqual(25.0f, v.MagnitudeSquared, 1e-14);
            Assert.AreEqual(5.0f, v.Magnitude, 1e-14);
        }

        [Test]
        public void Normalize()
        {
            Vector2F v, n1, n2;
            float magnitude;

            v = new Vector2F(3.0f, 4.0f);
            n1 = v.Normalize();
            n2 = v.Normalize(out magnitude);
            Assert.AreEqual(1.0f, n1.Magnitude, 1e-14);
            Assert.AreEqual(1.0f, n2.Magnitude, 1e-14);
            Assert.AreEqual(5.0f, magnitude, 1e-14);
        }

        [Test]
        public void NormalizeZeroVector()
        {
            Vector2F v = new Vector2F(0.0f, 0.0f);

            Vector2F n1 = v.Normalize();
            Assert.IsNaN(n1.X);
            Assert.IsNaN(n1.Y);
            Assert.IsTrue(n1.IsUndefined);

            float magnitude;
            Vector2F n2 = v.Normalize(out magnitude);
            Assert.IsNaN(n2.X);
            Assert.IsNaN(n2.Y);
            Assert.IsTrue(n2.IsUndefined);
            Assert.AreEqual(0.0f, magnitude);
        }

        [Test]
        public void Add()
        {
            Vector2F v1 = new Vector2F(1.0f, 2.0f);
            Vector2F v2 = new Vector2F(4.0f, 5.0f);

            Vector2F v3 = v1 + v2;
            Assert.AreEqual(5.0f, v3.X, 1e-14);
            Assert.AreEqual(7.0f, v3.Y, 1e-14);

            Vector2F v4 = v1.Add(v2);
            Assert.AreEqual(5.0f, v4.X, 1e-14);
            Assert.AreEqual(7.0f, v4.Y, 1e-14);
        }

        [Test]
        public void Subtract()
        {
            Vector2F v1 = new Vector2F(1.0f, 2.0f);
            Vector2F v2 = new Vector2F(4.0f, 5.0f);

            Vector2F v3 = v1 - v2;
            Assert.AreEqual(-3.0f, v3.X, 1e-14);
            Assert.AreEqual(-3.0f, v3.Y, 1e-14);

            Vector2F v4 = v1.Subtract(v2);
            Assert.AreEqual(-3.0f, v4.X, 1e-14);
            Assert.AreEqual(-3.0f, v4.Y, 1e-14);
        }

        [Test]
        public void Multiply()
        {
            Vector2F v1 = new Vector2F(1.0f, 2.0f);

            Vector2F v2 = v1 * 5.0f;
            Assert.AreEqual(5.0f, v2.X, 1e-14);
            Assert.AreEqual(10.0f, v2.Y, 1e-14);

            Vector2F v3 = 5.0f * v1;
            Assert.AreEqual(5.0f, v3.X, 1e-14);
            Assert.AreEqual(10.0f, v3.Y, 1e-14);

            Vector2F v4 = v1.Multiply(5.0f);
            Assert.AreEqual(5.0f, v4.X, 1e-14);
            Assert.AreEqual(10.0f, v4.Y, 1e-14);
        }

        [Test]
        public void Divide()
        {
            Vector2F v1 = new Vector2F(10.0f, 20.0f);

            Vector2F v2 = v1 / 5.0f;
            Assert.AreEqual(2.0f, v2.X, 1e-14);
            Assert.AreEqual(4.0f, v2.Y, 1e-14);

            Vector2F v3 = v1.Divide(5.0f);
            Assert.AreEqual(2.0f, v3.X, 1e-14);
            Assert.AreEqual(4.0f, v3.Y, 1e-14);
        }

        [Test]
        public void Zero()
        {
            Assert.AreEqual(0.0f, Vector2F.Zero.X);
            Assert.AreEqual(0.0f, Vector2F.Zero.Y);
        }

        [Test]
        public void UnitX()
        {
            Assert.AreEqual(1.0f, Vector2F.UnitX.X);
            Assert.AreEqual(0.0f, Vector2F.UnitX.Y);
        }

        [Test]
        public void UnitY()
        {
            Assert.AreEqual(0.0f, Vector2F.UnitY.X);
            Assert.AreEqual(1.0f, Vector2F.UnitY.Y);
        }

        [Test]
        public void Undefined()
        {
            Assert.IsNaN(Vector2F.Undefined.X);
            Assert.IsNaN(Vector2F.Undefined.Y);
        }

        [Test]
        public void IsUndefined()
        {
            Assert.IsTrue(Vector2F.Undefined.IsUndefined);
            Assert.IsFalse(Vector2F.UnitX.IsUndefined);
            Assert.IsFalse(Vector2F.UnitY.IsUndefined);
        }

        [Test]
        public void TestEquals()
        {
            Vector2F a = new Vector2F(1.0f, 2.0f);
            Vector2F b = new Vector2F(4.0f, 5.0f);
            Vector2F c = new Vector2F(1.0f, 2.0f);

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
            Vector2F a = new Vector2F(1.0f, 2.0f);
            Vector2F b = new Vector2F(4.0f, 5.0f);
            Vector2F c = new Vector2F(1.0f, 2.0f);

            Assert.AreEqual(a.GetHashCode(), c.GetHashCode());
            Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());
        }

        [Test]
        public void Dot()
        {
            Vector2F a = new Vector2F(1.0f, 2.0f);
            Vector2F b = new Vector2F(4.0f, 5.0f);

            float dot = a.Dot(b);
            Assert.AreEqual(1.0f * 4.0f + 2.0f * 5.0f, dot, 1e-7);
        }

        [Test]
        public void Negate()
        {
            Vector2F a = new Vector2F(1.0f, 2.0f);
            Vector2F negatedA1 = a.Negate();
            Assert.AreEqual(-1.0f, negatedA1.X, 1e-7);
            Assert.AreEqual(-2.0f, negatedA1.Y, 1e-7);
            Vector2F negatedA2 = -a;
            Assert.AreEqual(-1.0f, negatedA2.X, 1e-7);
            Assert.AreEqual(-2.0f, negatedA2.Y, 1e-7);

            Vector2F b = new Vector2F(-1.0f, -2.0f);
            Vector2F negatedB1 = b.Negate();
            Assert.AreEqual(1.0f, negatedB1.X, 1e-7);
            Assert.AreEqual(2.0f, negatedB1.Y, 1e-7);
            Vector2F negatedB2 = -b;
            Assert.AreEqual(1.0f, negatedB2.X, 1e-7);
            Assert.AreEqual(2.0f, negatedB2.Y, 1e-7);
        }

#if !CSToJava
        [Test]
        public void TestToString()
        {
            CultureInfo originalCulture = Thread.CurrentThread.CurrentCulture;
            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("tr-TR");
                Vector2F a = new Vector2F(1.23f, 2.34f);
                Assert.AreEqual("(1,23, 2,34)", a.ToString());
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = originalCulture;
            }
        }
#endif

        [Test]
        public void EqualsEpsilon()
        {
            Vector2F a = new Vector2F(1.23f, 2.34f);
            Vector2F b = new Vector2F(1.24f, 2.35f);
            Assert.IsTrue(a.EqualsEpsilon(b, 0.011f));
            Assert.IsFalse(a.EqualsEpsilon(b, 0.009f));
        }

        [Test]
        public void ToVector2D()
        {
            Vector2F a = new Vector2F(1.0f, 2.0f);
            Vector2D sA = a.ToVector2D();
            Assert.AreEqual(1.0, sA.X, 1e-14);
            Assert.AreEqual(2.0, sA.Y, 1e-14);
        }

        [Test]
        public void ToVector2H()
        {
            Vector2F a = new Vector2F(1.0f, 2.0f);
            Vector2H sA = a.ToVector2H();
            Assert.AreEqual((Half)1.0, sA.X, 1e-7);
            Assert.AreEqual((Half)2.0, sA.Y, 1e-7);
        }
    }
}
