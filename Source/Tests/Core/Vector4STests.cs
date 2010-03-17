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
    public class Vector4STests
    {
        [Test]
        public void Construct()
        {
            Vector4S v = new Vector4S(1.0f, 2.0f, 3.0f, 4.0f);
            Assert.AreEqual(1.0f, v.X);
            Assert.AreEqual(2.0f, v.Y);
            Assert.AreEqual(3.0f, v.Z);
        }

        [Test]
        public void Magnitude()
        {
            Vector4S v = new Vector4S(3.0f, 4.0f, 0.0f, 0.0f);
            Assert.AreEqual(25.0f, v.MagnitudeSquared, 1e-14);
            Assert.AreEqual(5.0f, v.Magnitude, 1e-14);

            v = new Vector4S(3.0f, 0.0f, 4.0f, 0.0f);
            Assert.AreEqual(25.0f, v.MagnitudeSquared, 1e-14);
            Assert.AreEqual(5.0f, v.Magnitude, 1e-14);

            v = new Vector4S(0.0f, 3.0f, 4.0f, 0.0f);
            Assert.AreEqual(25.0f, v.MagnitudeSquared, 1e-14);
            Assert.AreEqual(5.0f, v.Magnitude, 1e-14);

            v = new Vector4S(0.0f, 0.0f, 3.0f, 4.0f);
            Assert.AreEqual(25.0f, v.MagnitudeSquared, 1e-14);
            Assert.AreEqual(5.0f, v.Magnitude, 1e-14);
        }

        [Test]
        public void Normalize()
        {
            Vector4S v, n1, n2;
            float magnitude;

            v = new Vector4S(3.0f, 4.0f, 0.0f, 0.0f);
            n1 = v.Normalize();
            n2 = v.Normalize(out magnitude);
            Assert.AreEqual(1.0f, n1.Magnitude, 1e-14);
            Assert.AreEqual(1.0f, n2.Magnitude, 1e-14);
            Assert.AreEqual(5.0f, magnitude, 1e-14);

            v = new Vector4S(3.0f, 0.0f, 4.0f, 0.0f);
            n1 = v.Normalize();
            n2 = v.Normalize(out magnitude);
            Assert.AreEqual(1.0f, n1.Magnitude, 1e-14);
            Assert.AreEqual(1.0f, n2.Magnitude, 1e-14);
            Assert.AreEqual(5.0f, magnitude, 1e-14);

            v = new Vector4S(0.0f, 3.0f, 4.0f, 0.0f);
            n1 = v.Normalize();
            n2 = v.Normalize(out magnitude);
            Assert.AreEqual(1.0f, n1.Magnitude, 1e-14);
            Assert.AreEqual(1.0f, n2.Magnitude, 1e-14);
            Assert.AreEqual(5.0f, magnitude, 1e-14);

            v = new Vector4S(0.0f, 0.0f, 3.0f, 4.0f);
            n1 = v.Normalize();
            n2 = v.Normalize(out magnitude);
            Assert.AreEqual(1.0f, n1.Magnitude, 1e-14);
            Assert.AreEqual(1.0f, n2.Magnitude, 1e-14);
            Assert.AreEqual(5.0f, magnitude, 1e-14);
        }

        [Test]
        public void NormalizeZeroVector()
        {
            Vector4S v = new Vector4S(0.0f, 0.0f, 0.0f, 0.0f);

            Vector4S n1 = v.Normalize();
            Assert.IsNaN(n1.X);
            Assert.IsNaN(n1.Y);
            Assert.IsNaN(n1.Z);
            Assert.IsTrue(n1.IsUndefined);

            float magnitude;
            Vector4S n2 = v.Normalize(out magnitude);
            Assert.IsNaN(n2.X);
            Assert.IsNaN(n2.Y);
            Assert.IsNaN(n2.Z);
            Assert.IsTrue(n2.IsUndefined);
            Assert.AreEqual(0.0f, magnitude);
        }

        [Test]
        public void Add()
        {
            Vector4S v1 = new Vector4S(1.0f, 2.0f, 3.0f, 4.0f);
            Vector4S v2 = new Vector4S(4.0f, 5.0f, 6.0f, 7.0f);

            Vector4S v3 = v1 + v2;
            Assert.AreEqual(5.0f, v3.X, 1e-14);
            Assert.AreEqual(7.0f, v3.Y, 1e-14);
            Assert.AreEqual(9.0f, v3.Z, 1e-14);
            Assert.AreEqual(11.0f, v3.W, 1e-14);

            Vector4S v4 = v1.Add(v2);
            Assert.AreEqual(5.0f, v4.X, 1e-14);
            Assert.AreEqual(7.0f, v4.Y, 1e-14);
            Assert.AreEqual(9.0f, v4.Z, 1e-14);
            Assert.AreEqual(11.0f, v4.W, 1e-14);
        }

        [Test]
        public void Subtract()
        {
            Vector4S v1 = new Vector4S(1.0f, 2.0f, 3.0f, 4.0f);
            Vector4S v2 = new Vector4S(4.0f, 5.0f, 6.0f, 7.0f);

            Vector4S v3 = v1 - v2;
            Assert.AreEqual(-3.0f, v3.X, 1e-14);
            Assert.AreEqual(-3.0f, v3.Y, 1e-14);
            Assert.AreEqual(-3.0f, v3.Z, 1e-14);
            Assert.AreEqual(-3.0f, v3.W, 1e-14);

            Vector4S v4 = v1.Subtract(v2);
            Assert.AreEqual(-3.0f, v4.X, 1e-14);
            Assert.AreEqual(-3.0f, v4.Y, 1e-14);
            Assert.AreEqual(-3.0f, v4.Z, 1e-14);
            Assert.AreEqual(-3.0f, v4.W, 1e-14);
        }

        [Test]
        public void Multiply()
        {
            Vector4S v1 = new Vector4S(1.0f, 2.0f, 3.0f, 4.0f);

            Vector4S v2 = v1 * 5.0f;
            Assert.AreEqual(5.0f, v2.X, 1e-14);
            Assert.AreEqual(10.0f, v2.Y, 1e-14);
            Assert.AreEqual(15.0f, v2.Z, 1e-14);
            Assert.AreEqual(20.0f, v2.W, 1e-14);

            Vector4S v3 = 5.0f * v1;
            Assert.AreEqual(5.0f, v3.X, 1e-14);
            Assert.AreEqual(10.0f, v3.Y, 1e-14);
            Assert.AreEqual(15.0f, v3.Z, 1e-14);
            Assert.AreEqual(20.0f, v3.W, 1e-14);

            Vector4S v4 = v1.Multiply(5.0f);
            Assert.AreEqual(5.0f, v4.X, 1e-14);
            Assert.AreEqual(10.0f, v4.Y, 1e-14);
            Assert.AreEqual(15.0f, v4.Z, 1e-14);
            Assert.AreEqual(20.0f, v4.W, 1e-14);
        }

        [Test]
        public void MultiplyComponents()
        {
            Vector4S v1 = new Vector4S(1, 2, 3, 4);
            Vector4S v2 = new Vector4S(4, 8, 12, 16);

            Assert.AreEqual(new Vector4S(4, 16, 36, 64), v1.MultiplyComponents(v2));
        }

        [Test]
        public void Divide()
        {
            Vector4S v1 = new Vector4S(10.0f, 20.0f, 30.0f, 40.0f);

            Vector4S v2 = v1 / 5.0f;
            Assert.AreEqual(2.0f, v2.X, 1e-14);
            Assert.AreEqual(4.0f, v2.Y, 1e-14);
            Assert.AreEqual(6.0f, v2.Z, 1e-14);
            Assert.AreEqual(8.0f, v2.W, 1e-14);

            Vector4S v3 = v1.Divide(5.0f);
            Assert.AreEqual(2.0f, v3.X, 1e-14);
            Assert.AreEqual(4.0f, v3.Y, 1e-14);
            Assert.AreEqual(6.0f, v3.Z, 1e-14);
            Assert.AreEqual(8.0f, v3.W, 1e-14);
        }

        [Test]
        public void MostOrthogonalAxis()
        {
            Vector4S v1 = new Vector4S(1, 2, 3, 4);
            Assert.AreEqual(Vector4S.UnitX, v1.MostOrthogonalAxis);

            Vector4S v2 = new Vector4S(-3, -1, -2, -4);
            Assert.AreEqual(Vector4S.UnitY, v2.MostOrthogonalAxis);

            Vector4S v3 = new Vector4S(3, 2, 1, 4);
            Assert.AreEqual(Vector4S.UnitZ, v3.MostOrthogonalAxis);

            Vector4S v4 = new Vector4S(3, 2, 4, 1);
            Assert.AreEqual(Vector4S.UnitW, v4.MostOrthogonalAxis);
        }

        [Test]
        public void Zero()
        {
            Assert.AreEqual(0.0f, Vector4S.Zero.X);
            Assert.AreEqual(0.0f, Vector4S.Zero.Y);
            Assert.AreEqual(0.0f, Vector4S.Zero.Z);
            Assert.AreEqual(0.0f, Vector4S.Zero.W);
        }

        [Test]
        public void UnitX()
        {
            Assert.AreEqual(1.0f, Vector4S.UnitX.X);
            Assert.AreEqual(0.0f, Vector4S.UnitX.Y);
            Assert.AreEqual(0.0f, Vector4S.UnitX.Z);
            Assert.AreEqual(0.0f, Vector4S.UnitX.W);
        }

        [Test]
        public void UnitY()
        {
            Assert.AreEqual(0.0f, Vector4S.UnitY.X);
            Assert.AreEqual(1.0f, Vector4S.UnitY.Y);
            Assert.AreEqual(0.0f, Vector4S.UnitY.Z);
            Assert.AreEqual(0.0f, Vector4S.UnitY.W);
        }

        [Test]
        public void UnitZ()
        {
            Assert.AreEqual(0.0f, Vector4S.UnitZ.X);
            Assert.AreEqual(0.0f, Vector4S.UnitZ.Y);
            Assert.AreEqual(1.0f, Vector4S.UnitZ.Z);
            Assert.AreEqual(0.0f, Vector4S.UnitZ.W);
        }

        [Test]
        public void UnitW()
        {
            Assert.AreEqual(0.0f, Vector4S.UnitW.X);
            Assert.AreEqual(0.0f, Vector4S.UnitW.Y);
            Assert.AreEqual(0.0f, Vector4S.UnitW.Z);
            Assert.AreEqual(1.0f, Vector4S.UnitW.W);
        }

        [Test]
        public void Undefined()
        {
            Assert.IsNaN(Vector4S.Undefined.X);
            Assert.IsNaN(Vector4S.Undefined.Y);
            Assert.IsNaN(Vector4S.Undefined.Z);
            Assert.IsNaN(Vector4S.Undefined.W);
        }

        [Test]
        public void IsUndefined()
        {
            Assert.IsTrue(Vector4S.Undefined.IsUndefined);
            Assert.IsFalse(Vector4S.UnitX.IsUndefined);
            Assert.IsFalse(Vector4S.UnitY.IsUndefined);
            Assert.IsFalse(Vector4S.UnitZ.IsUndefined);
            Assert.IsFalse(Vector4S.UnitW.IsUndefined);
        }

        [Test]
        public void TestEquals()
        {
            Vector4S a = new Vector4S(1.0f, 2.0f, 3.0f, 4.0f);
            Vector4S b = new Vector4S(4.0f, 5.0f, 6.0f, 7.0f);
            Vector4S c = new Vector4S(1.0f, 2.0f, 3.0f, 4.0f);

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
            Vector4S a = new Vector4S(1.0f, 2.0f, 3.0f, 4.0f);
            Vector4S b = new Vector4S(4.0f, 5.0f, 6.0f, 7.0f);
            Vector4S c = new Vector4S(1.0f, 2.0f, 3.0f, 4.0f);

            Assert.AreEqual(a.GetHashCode(), c.GetHashCode());
            Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());
        }

        [Test]
        public void Dot()
        {
            Vector4S a = new Vector4S(1.0f, 2.0f, 3.0f, 4.0f);
            Vector4S b = new Vector4S(4.0f, 5.0f, 6.0f, 7.0f);

            double dot = a.Dot(b);
            Assert.AreEqual(1.0f * 4.0f + 2.0f * 5.0f + 3.0f * 6.0f + 4.0f * 7.0f, dot, 1e-14);
        }

        [Test]
        public void Invert()
        {
            Vector4S a = new Vector4S(1.0f, 2.0f, 3.0f, 4.0f);
            Vector4S invertedA1 = a.Invert();
            Assert.AreEqual(-1.0f, invertedA1.X, 1e-14);
            Assert.AreEqual(-2.0f, invertedA1.Y, 1e-14);
            Assert.AreEqual(-3.0f, invertedA1.Z, 1e-14);
            Assert.AreEqual(-4.0f, invertedA1.W, 1e-14);
            Vector4S invertedA2 = -a;
            Assert.AreEqual(-1.0f, invertedA2.X, 1e-14);
            Assert.AreEqual(-2.0f, invertedA2.Y, 1e-14);
            Assert.AreEqual(-3.0f, invertedA2.Z, 1e-14);
            Assert.AreEqual(-4.0f, invertedA2.W, 1e-14);

            Vector4S b = new Vector4S(-1.0f, -2.0f, -3.0f, -4.0f);
            Vector4S invertedB1 = b.Invert();
            Assert.AreEqual(1.0f, invertedB1.X, 1e-14);
            Assert.AreEqual(2.0f, invertedB1.Y, 1e-14);
            Assert.AreEqual(3.0f, invertedB1.Z, 1e-14);
            Assert.AreEqual(4.0f, invertedB1.W, 1e-14);
            Vector4S invertedB2 = -b;
            Assert.AreEqual(1.0f, invertedB2.X, 1e-14);
            Assert.AreEqual(2.0f, invertedB2.Y, 1e-14);
            Assert.AreEqual(3.0f, invertedB2.Z, 1e-14);
            Assert.AreEqual(4.0f, invertedB2.W, 1e-14);
        }

        [Test]
        public void ToVector4D()
        {
            Vector4S a = new Vector4S(1.0f, 2.0f, 3.0f, 4.0f);
            Vector4D dA = a.ToVector4D();
            Assert.AreEqual(1.0, dA.X, 1e-7);
            Assert.AreEqual(2.0, dA.Y, 1e-7);
            Assert.AreEqual(3.0, dA.Z, 1e-7);
            Assert.AreEqual(4.0, dA.W, 1e-7);
        }

        [Test]
        public void ToVector4H()
        {
            Vector4S a = new Vector4S(1.0f, 2.0f, 3.0f, 4.0f);
            Vector4H sA = a.ToVector4H();
            Assert.AreEqual((Half)1.0f, sA.X, 1e-7);
            Assert.AreEqual((Half)2.0f, sA.Y, 1e-7);
            Assert.AreEqual((Half)3.0f, sA.Z, 1e-7);
            Assert.AreEqual((Half)4.0f, sA.W, 1e-7);
        }

        [Test]
        public void TestToString()
        {
            CultureInfo originalCulture = Thread.CurrentThread.CurrentCulture;
            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("tr-TR");
                Vector4S a = new Vector4S(1.23f, 2.34f, 3.45f, 4.56f);
                Assert.AreEqual("(1,23, 2,34, 3,45, 4,56)", a.ToString());
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = originalCulture;
            }
        }

        [Test]
        public void EqualsEpsilon()
        {
            Vector4S a = new Vector4S(1.23f, 2.34f, 3.45f, 4.56f);
            Vector4S b = new Vector4S(1.24f, 2.35f, 3.46f, 4.57f);
            Assert.IsTrue(a.EqualsEpsilon(b, 0.011f));
            Assert.IsFalse(a.EqualsEpsilon(b, 0.009f));
        }

        [Test]
        public void XY()
        {
            Vector4S a = new Vector4S(1.23f, 2.34f, 3.45f, 4.56f);
            Vector2S xy = a.XY;
            Assert.AreEqual(1.23f, xy.X, 1e-14);
            Assert.AreEqual(2.34f, xy.Y, 1e-14);
        }

        [Test]
        public void XYZ()
        {
            Vector4S a = new Vector4S(1.23f, 2.34f, 3.45f, 4.56f);
            Vector3S xyz = a.XYZ;
            Assert.AreEqual(1.23f, xyz.X, 1e-14);
            Assert.AreEqual(2.34f, xyz.Y, 1e-14);
            Assert.AreEqual(3.45f, xyz.Z, 1e-14);
        }
    }
}
