#region License
//
// (C) Copyright 2010 Patrick Cozzi, Deron Ohlarik, and Kevin Ring
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
    public class Vector2DTests
    {
        [Test]
        public void Construct()
        {
            Vector2D v = new Vector2D(1.0, 2.0);
            Assert.AreEqual(1.0, v.X);
            Assert.AreEqual(2.0, v.Y);
        }

        [Test]
        public void Magnitude()
        {
            Vector2D v = new Vector2D(3.0, 4.0);
            Assert.AreEqual(25.0, v.MagnitudeSquared, 1e-14);
            Assert.AreEqual(5.0, v.Magnitude, 1e-14);
        }

        [Test]
        public void Normalize()
        {
            Vector2D v, n1, n2;
            double magnitude;

            v = new Vector2D(3.0, 4.0);
            n1 = v.Normalize();
            n2 = v.Normalize(out magnitude);
            Assert.AreEqual(1.0, n1.Magnitude, 1e-14);
            Assert.AreEqual(1.0, n2.Magnitude, 1e-14);
            Assert.AreEqual(5.0, magnitude, 1e-14);
        }

        [Test]
        public void NormalizeZeroVector()
        {
            Vector2D v = new Vector2D(0.0, 0.0);

            Vector2D n1 = v.Normalize();
            Assert.IsNaN(n1.X);
            Assert.IsNaN(n1.Y);
            Assert.IsTrue(n1.IsUndefined);

            double magnitude;
            Vector2D n2 = v.Normalize(out magnitude);
            Assert.IsNaN(n2.X);
            Assert.IsNaN(n2.Y);
            Assert.IsTrue(n2.IsUndefined);
            Assert.AreEqual(0.0, magnitude);
        }

        [Test]
        public void Add()
        {
            Vector2D v1 = new Vector2D(1.0, 2.0);
            Vector2D v2 = new Vector2D(4.0, 5.0);

            Vector2D v3 = v1 + v2;
            Assert.AreEqual(5.0, v3.X, 1e-14);
            Assert.AreEqual(7.0, v3.Y, 1e-14);

            Vector2D v4 = v1.Add(v2);
            Assert.AreEqual(5.0, v4.X, 1e-14);
            Assert.AreEqual(7.0, v4.Y, 1e-14);
        }

        [Test]
        public void Subtract()
        {
            Vector2D v1 = new Vector2D(1.0, 2.0);
            Vector2D v2 = new Vector2D(4.0, 5.0);

            Vector2D v3 = v1 - v2;
            Assert.AreEqual(-3.0, v3.X, 1e-14);
            Assert.AreEqual(-3.0, v3.Y, 1e-14);

            Vector2D v4 = v1.Subtract(v2);
            Assert.AreEqual(-3.0, v4.X, 1e-14);
            Assert.AreEqual(-3.0, v4.Y, 1e-14);
        }

        [Test]
        public void Multiply()
        {
            Vector2D v1 = new Vector2D(1.0, 2.0);

            Vector2D v2 = v1 * 5.0;
            Assert.AreEqual(5.0, v2.X, 1e-14);
            Assert.AreEqual(10.0, v2.Y, 1e-14);

            Vector2D v3 = 5.0 * v1;
            Assert.AreEqual(5.0, v3.X, 1e-14);
            Assert.AreEqual(10.0, v3.Y, 1e-14);

            Vector2D v4 = v1.Multiply(5.0);
            Assert.AreEqual(5.0, v4.X, 1e-14);
            Assert.AreEqual(10.0, v4.Y, 1e-14);
        }

        [Test]
        public void Divide()
        {
            Vector2D v1 = new Vector2D(10.0, 20.0);

            Vector2D v2 = v1 / 5.0;
            Assert.AreEqual(2.0, v2.X, 1e-14);
            Assert.AreEqual(4.0, v2.Y, 1e-14);

            Vector2D v3 = v1.Divide(5.0);
            Assert.AreEqual(2.0, v3.X, 1e-14);
            Assert.AreEqual(4.0, v3.Y, 1e-14);
        }

        [Test]
        public void Zero()
        {
            Assert.AreEqual(0.0, Vector2D.Zero.X);
            Assert.AreEqual(0.0, Vector2D.Zero.Y);
        }

        [Test]
        public void UnitX()
        {
            Assert.AreEqual(1.0, Vector2D.UnitX.X);
            Assert.AreEqual(0.0, Vector2D.UnitX.Y);
        }

        [Test]
        public void UnitY()
        {
            Assert.AreEqual(0.0, Vector2D.UnitY.X);
            Assert.AreEqual(1.0, Vector2D.UnitY.Y);
        }

        [Test]
        public void Undefined()
        {
            Assert.IsNaN(Vector2D.Undefined.X);
            Assert.IsNaN(Vector2D.Undefined.Y);
        }

        [Test]
        public void IsUndefined()
        {
            Assert.IsTrue(Vector2D.Undefined.IsUndefined);
            Assert.IsFalse(Vector2D.UnitX.IsUndefined);
            Assert.IsFalse(Vector2D.UnitY.IsUndefined);
        }

        [Test]
        public void TestEquals()
        {
            Vector2D a = new Vector2D(1.0, 2.0);
            Vector2D b = new Vector2D(4.0, 5.0);
            Vector2D c = new Vector2D(1.0, 2.0);

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
            Vector2D a = new Vector2D(1.0, 2.0);
            Vector2D b = new Vector2D(4.0, 5.0);
            Vector2D c = new Vector2D(1.0, 2.0);

            Assert.AreEqual(a.GetHashCode(), c.GetHashCode());
            Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());
        }

        [Test]
        public void Dot()
        {
            Vector2D a = new Vector2D(1.0, 2.0);
            Vector2D b = new Vector2D(4.0, 5.0);

            double dot = a.Dot(b);
            Assert.AreEqual(1.0 * 4.0 + 2.0 * 5.0, dot, 1e-14);
        }

        [Test]
        public void Negate()
        {
            Vector2D a = new Vector2D(1.0, 2.0);
            Vector2D negatedA1 = a.Negate();
            Assert.AreEqual(-1.0, negatedA1.X, 1e-14);
            Assert.AreEqual(-2.0, negatedA1.Y, 1e-14);
            Vector2D negatedA2 = -a;
            Assert.AreEqual(-1.0, negatedA2.X, 1e-14);
            Assert.AreEqual(-2.0, negatedA2.Y, 1e-14);

            Vector2D b = new Vector2D(-1.0, -2.0);
            Vector2D negatedB1 = b.Negate();
            Assert.AreEqual(1.0, negatedB1.X, 1e-14);
            Assert.AreEqual(2.0, negatedB1.Y, 1e-14);
            Vector2D negatedB2 = -b;
            Assert.AreEqual(1.0, negatedB2.X, 1e-14);
            Assert.AreEqual(2.0, negatedB2.Y, 1e-14);
        }

        [Test]
        public void ToVector2S()
        {
            Vector2D a = new Vector2D(1.0, 2.0);
            Vector2S sA = a.ToVector2S();
            Assert.AreEqual(1.0f, sA.X, 1e-7);
            Assert.AreEqual(2.0f, sA.Y, 1e-7);
        }

        [Test]
        public void ToVector2H()
        {
            Vector2D a = new Vector2D(1.0, 2.0);
            Vector2H sA = a.ToVector2H();
            Assert.AreEqual((Half)1.0, sA.X, 1e-7);
            Assert.AreEqual((Half)2.0, sA.Y, 1e-7);
        }

#if !CSToJava
        [Test]
        public void TestToString()
        {
            CultureInfo originalCulture = Thread.CurrentThread.CurrentCulture;
            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("tr-TR");
                Vector2D a = new Vector2D(1.23, 2.34);
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
            Vector2D a = new Vector2D(1.23, 2.34);
            Vector2D b = new Vector2D(1.24, 2.35);
            Assert.IsTrue(a.EqualsEpsilon(b, 0.011));
            Assert.IsFalse(a.EqualsEpsilon(b, 0.009));
        }
    }
}
