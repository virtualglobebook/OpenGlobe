#region License
//
// (C) Copyright 2010 Patrick Cozzi, Deron Ohlarik, and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0f.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using NUnit.Framework;
using System.Globalization;
using System.Threading;

namespace MiniGlobe.Core
{
    [TestFixture]
    public class Vector3STests
    {
        [Test]
        public void Construct()
        {
            Vector3S v = new Vector3S(1.0f, 2.0f, 3.0f);
            Assert.AreEqual(1.0f, v.X);
            Assert.AreEqual(2.0f, v.Y);
            Assert.AreEqual(3.0f, v.Z);
        }

        [Test]
        public void Magnitude()
        {
            Vector3S v = new Vector3S(3.0f, 4.0f, 0.0f);
            Assert.AreEqual(25.0f, v.MagnitudeSquared, 1e-7);
            Assert.AreEqual(5.0f, v.Magnitude, 1e-7);

            v = new Vector3S(3.0f, 0.0f, 4.0f);
            Assert.AreEqual(25.0f, v.MagnitudeSquared, 1e-7);
            Assert.AreEqual(5.0f, v.Magnitude, 1e-7);

            v = new Vector3S(0.0f, 3.0f, 4.0f);
            Assert.AreEqual(25.0f, v.MagnitudeSquared, 1e-7);
            Assert.AreEqual(5.0f, v.Magnitude, 1e-7);
        }

        [Test]
        public void Normalize()
        {
            Vector3S v, n1, n2;
            float magnitude;

            v = new Vector3S(3.0f, 4.0f, 0.0f);
            n1 = v.Normalize();
            n2 = v.Normalize(out magnitude);
            Assert.AreEqual(1.0f, n1.Magnitude, 1e-7);
            Assert.AreEqual(1.0f, n2.Magnitude, 1e-7);
            Assert.AreEqual(5.0f, magnitude, 1e-7);

            v = new Vector3S(3.0f, 0.0f, 4.0f);
            n1 = v.Normalize();
            n2 = v.Normalize(out magnitude);
            Assert.AreEqual(1.0f, n1.Magnitude, 1e-7);
            Assert.AreEqual(1.0f, n2.Magnitude, 1e-7);
            Assert.AreEqual(5.0f, magnitude, 1e-7);

            v = new Vector3S(0.0f, 3.0f, 4.0f);
            n1 = v.Normalize();
            n2 = v.Normalize(out magnitude);
            Assert.AreEqual(1.0f, n1.Magnitude, 1e-7);
            Assert.AreEqual(1.0f, n2.Magnitude, 1e-7);
            Assert.AreEqual(5.0f, magnitude, 1e-7);
        }

        [Test]
        public void NormalizeZeroVector()
        {
            Vector3S v = new Vector3S(0.0f, 0.0f, 0.0f);

            Vector3S n1 = v.Normalize();
            Assert.IsNaN(n1.X);
            Assert.IsNaN(n1.Y);
            Assert.IsNaN(n1.Z);
            Assert.IsTrue(n1.IsUndefined);

            float magnitude;
            Vector3S n2 = v.Normalize(out magnitude);
            Assert.IsNaN(n2.X);
            Assert.IsNaN(n2.Y);
            Assert.IsNaN(n2.Z);
            Assert.IsTrue(n2.IsUndefined);
            Assert.AreEqual(0.0f, magnitude);
        }

        [Test]
        public void Add()
        {
            Vector3S v1 = new Vector3S(1.0f, 2.0f, 3.0f);
            Vector3S v2 = new Vector3S(4.0f, 5.0f, 6.0f);

            Vector3S v3 = v1 + v2;
            Assert.AreEqual(5.0f, v3.X, 1e-7);
            Assert.AreEqual(7.0f, v3.Y, 1e-7);
            Assert.AreEqual(9.0f, v3.Z, 1e-7);

            Vector3S v4 = v1.Add(v2);
            Assert.AreEqual(5.0f, v4.X, 1e-7);
            Assert.AreEqual(7.0f, v4.Y, 1e-7);
            Assert.AreEqual(9.0f, v4.Z, 1e-7);
        }

        [Test]
        public void Subtract()
        {
            Vector3S v1 = new Vector3S(1.0f, 2.0f, 3.0f);
            Vector3S v2 = new Vector3S(4.0f, 5.0f, 6.0f);

            Vector3S v3 = v1 - v2;
            Assert.AreEqual(-3.0f, v3.X, 1e-7);
            Assert.AreEqual(-3.0f, v3.Y, 1e-7);
            Assert.AreEqual(-3.0f, v3.Z, 1e-7);

            Vector3S v4 = v1.Subtract(v2);
            Assert.AreEqual(-3.0f, v4.X, 1e-7);
            Assert.AreEqual(-3.0f, v4.Y, 1e-7);
            Assert.AreEqual(-3.0f, v4.Z, 1e-7);
        }

        [Test]
        public void Multiply()
        {
            Vector3S v1 = new Vector3S(1.0f, 2.0f, 3.0f);

            Vector3S v2 = v1 * 5.0f;
            Assert.AreEqual(5.0f, v2.X, 1e-7);
            Assert.AreEqual(10.0f, v2.Y, 1e-7);
            Assert.AreEqual(15.0f, v2.Z, 1e-7);

            Vector3S v3 = 5.0f * v1;
            Assert.AreEqual(5.0f, v3.X, 1e-7);
            Assert.AreEqual(10.0f, v3.Y, 1e-7);
            Assert.AreEqual(15.0f, v3.Z, 1e-7);

            Vector3S v4 = v1.Multiply(5.0f);
            Assert.AreEqual(5.0f, v4.X, 1e-7);
            Assert.AreEqual(10.0f, v4.Y, 1e-7);
            Assert.AreEqual(15.0f, v4.Z, 1e-7);
        }

        [Test]
        public void MultiplyComponents()
        {
            Vector3S v1 = new Vector3S(1, 2, 3);
            Vector3S v2 = new Vector3S(4, 8, 12);

            Assert.AreEqual(new Vector3S(4, 16, 36), v1.MultiplyComponents(v2));
        }

        [Test]
        public void Divide()
        {
            Vector3S v1 = new Vector3S(10.0f, 20.0f, 30.0f);

            Vector3S v2 = v1 / 5.0f;
            Assert.AreEqual(2.0f, v2.X, 1e-7);
            Assert.AreEqual(4.0f, v2.Y, 1e-7);
            Assert.AreEqual(6.0f, v2.Z, 1e-7);

            Vector3S v3 = v1.Divide(5.0f);
            Assert.AreEqual(2.0f, v3.X, 1e-7);
            Assert.AreEqual(4.0f, v3.Y, 1e-7);
            Assert.AreEqual(6.0f, v3.Z, 1e-7);
        }

        [Test]
        public void MostOrthogonalAxis()
        {
            Vector3S v1 = new Vector3S(1, 2, 3);
            Assert.AreEqual(Vector3S.UnitX, v1.MostOrthogonalAxis);

            Vector3S v2 = new Vector3S(-3, -1, -2);
            Assert.AreEqual(Vector3S.UnitY, v2.MostOrthogonalAxis);

            Vector3S v3 = new Vector3S(3, 2, 1);
            Assert.AreEqual(Vector3S.UnitZ, v3.MostOrthogonalAxis);
        }

        [Test]
        public void Zero()
        {
            Assert.AreEqual(0.0f, Vector3S.Zero.X);
            Assert.AreEqual(0.0f, Vector3S.Zero.Y);
            Assert.AreEqual(0.0f, Vector3S.Zero.Z);
        }

        [Test]
        public void UnitX()
        {
            Assert.AreEqual(1.0f, Vector3S.UnitX.X);
            Assert.AreEqual(0.0f, Vector3S.UnitX.Y);
            Assert.AreEqual(0.0f, Vector3S.UnitX.Z);
        }

        [Test]
        public void UnitY()
        {
            Assert.AreEqual(0.0f, Vector3S.UnitY.X);
            Assert.AreEqual(1.0f, Vector3S.UnitY.Y);
            Assert.AreEqual(0.0f, Vector3S.UnitY.Z);
        }

        [Test]
        public void UnitZ()
        {
            Assert.AreEqual(0.0f, Vector3S.UnitZ.X);
            Assert.AreEqual(0.0f, Vector3S.UnitZ.Y);
            Assert.AreEqual(1.0f, Vector3S.UnitZ.Z);
        }

        [Test]
        public void Undefined()
        {
            Assert.IsNaN(Vector3S.Undefined.X);
            Assert.IsNaN(Vector3S.Undefined.Y);
            Assert.IsNaN(Vector3S.Undefined.Z);
        }

        [Test]
        public void IsUndefined()
        {
            Assert.IsTrue(Vector3S.Undefined.IsUndefined);
            Assert.IsFalse(Vector3S.UnitX.IsUndefined);
            Assert.IsFalse(Vector3S.UnitY.IsUndefined);
            Assert.IsFalse(Vector3S.UnitZ.IsUndefined);
        }

        [Test]
        public void TestEquals()
        {
            Vector3S a = new Vector3S(1.0f, 2.0f, 3.0f);
            Vector3S b = new Vector3S(4.0f, 5.0f, 6.0f);
            Vector3S c = new Vector3S(1.0f, 2.0f, 3.0f);

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
            Vector3S a = new Vector3S(1.0f, 2.0f, 3.0f);
            Vector3S b = new Vector3S(4.0f, 5.0f, 6.0f);
            Vector3S c = new Vector3S(1.0f, 2.0f, 3.0f);

            Assert.AreEqual(a.GetHashCode(), c.GetHashCode());
            Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());
        }

        [Test]
        public void Dot()
        {
            Vector3S a = new Vector3S(1.0f, 2.0f, 3.0f);
            Vector3S b = new Vector3S(4.0f, 5.0f, 6.0f);

            double dot = a.Dot(b);
            Assert.AreEqual(1.0f * 4.0f + 2.0f * 5.0f + 3.0f * 6.0f, dot, 1e-7);
        }

        [Test]
        public void Negate()
        {
            Vector3S a = new Vector3S(1.0f, 2.0f, 3.0f);
            Vector3S negatedA1 = a.Negate();
            Assert.AreEqual(-1.0f, negatedA1.X, 1e-7);
            Assert.AreEqual(-2.0f, negatedA1.Y, 1e-7);
            Assert.AreEqual(-3.0f, negatedA1.Z, 1e-7);
            Vector3S negatedA2 = -a;
            Assert.AreEqual(-1.0f, negatedA2.X, 1e-7);
            Assert.AreEqual(-2.0f, negatedA2.Y, 1e-7);
            Assert.AreEqual(-3.0f, negatedA2.Z, 1e-7);

            Vector3S b = new Vector3S(-1.0f, -2.0f, -3.0f);
            Vector3S negatedB1 = b.Negate();
            Assert.AreEqual(1.0f, negatedB1.X, 1e-7);
            Assert.AreEqual(2.0f, negatedB1.Y, 1e-7);
            Assert.AreEqual(3.0f, negatedB1.Z, 1e-7);
            Vector3S negatedB2 = -b;
            Assert.AreEqual(1.0f, negatedB2.X, 1e-7);
            Assert.AreEqual(2.0f, negatedB2.Y, 1e-7);
            Assert.AreEqual(3.0f, negatedB2.Z, 1e-7);
        }

        [Test]
        public void ToVector3D()
        {
            Vector3S a = new Vector3S(1.0f, 2.0f, 3.0f);
            Vector3D dA = a.ToVector3D();
            Assert.AreEqual(1.0, dA.X, 1e-7);
            Assert.AreEqual(2.0, dA.Y, 1e-7);
            Assert.AreEqual(3.0, dA.Z, 1e-7);
        }

        [Test]
        public void ToVector3H()
        {
            Vector3S a = new Vector3S(1.0f, 2.0f, 3.0f);
            Vector3H sA = a.ToVector3H();
            Assert.AreEqual((Half)1.0f, sA.X, 1e-7);
            Assert.AreEqual((Half)2.0f, sA.Y, 1e-7);
            Assert.AreEqual((Half)3.0f, sA.Z, 1e-7);
        }

        [Test]
        public void Cross()
        {
            Vector3S a = new Vector3S(1.0f, 0.0f, 0.0f);
            Vector3S b = new Vector3S(0.0f, 1.0f, 0.0f);

            Vector3S cross1 = a.Cross(b);
            Assert.AreEqual(0.0f, cross1.X, 1e-7);
            Assert.AreEqual(0.0f, cross1.Y, 1e-7);
            Assert.AreEqual(1.0f, cross1.Z, 1e-7);

            Vector3S cross2 = b.Cross(a);
            Assert.AreEqual(0.0f, cross2.X, 1e-7);
            Assert.AreEqual(0.0f, cross2.Y, 1e-7);
            Assert.AreEqual(-1.0f, cross2.Z, 1e-7);
        }

#if !CSToJava
        [Test]
        public void TestToString()
        {
            CultureInfo originalCulture = Thread.CurrentThread.CurrentCulture;
            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("tr-TR");
                Vector3S a = new Vector3S(1.23f, 2.34f, 3.45f);
                Assert.AreEqual("(1,23, 2,34, 3,45)", a.ToString());
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
            Vector3S a = new Vector3S(1.23f, 2.34f, 3.45f);
            Vector3S b = new Vector3S(1.24f, 2.35f, 3.46f);
            Assert.IsTrue(a.EqualsEpsilon(b, 0.011f));
            Assert.IsFalse(a.EqualsEpsilon(b, 0.009f));
        }

        [Test]
        public void XY()
        {
            Vector3S a = new Vector3S(1.23f, 2.34f, 3.45f);
            Vector2D xy = a.XY;
            Assert.AreEqual(1.23, xy.X, 1e-7);
            Assert.AreEqual(2.34, xy.Y, 1e-7);
        }
    }
}
