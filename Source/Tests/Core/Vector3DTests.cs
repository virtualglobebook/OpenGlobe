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

namespace MiniGlobe.Core
{
    [TestFixture]
    public class Vector3DTests
    {
        [Test]
        public void Construct()
        {
            Vector3D v = new Vector3D(1.0, 2.0, 3.0);
            Assert.AreEqual(1.0, v.X);
            Assert.AreEqual(2.0, v.Y);
            Assert.AreEqual(3.0, v.Z);
        }

        [Test]
        public void Magnitude()
        {
            Vector3D v = new Vector3D(3.0, 4.0, 0.0);
            Assert.AreEqual(25.0, v.MagnitudeSquared, 1e-14);
            Assert.AreEqual(5.0, v.Magnitude, 1e-14);

            v = new Vector3D(3.0, 0.0, 4.0);
            Assert.AreEqual(25.0, v.MagnitudeSquared, 1e-14);
            Assert.AreEqual(5.0, v.Magnitude, 1e-14);

            v = new Vector3D(0.0, 3.0, 4.0);
            Assert.AreEqual(25.0, v.MagnitudeSquared, 1e-14);
            Assert.AreEqual(5.0, v.Magnitude, 1e-14);
        }

        [Test]
        public void Normalize()
        {
            Vector3D v, n1, n2;
            double magnitude;

            v = new Vector3D(3.0, 4.0, 0.0);
            n1 = v.Normalize();
            n2 = v.Normalize(out magnitude);
            Assert.AreEqual(1.0, n1.Magnitude, 1e-14);
            Assert.AreEqual(1.0, n2.Magnitude, 1e-14);
            Assert.AreEqual(5.0, magnitude, 1e-14);

            v = new Vector3D(3.0, 0.0, 4.0);
            n1 = v.Normalize();
            n2 = v.Normalize(out magnitude);
            Assert.AreEqual(1.0, n1.Magnitude, 1e-14);
            Assert.AreEqual(1.0, n2.Magnitude, 1e-14);
            Assert.AreEqual(5.0, magnitude, 1e-14);

            v = new Vector3D(0.0, 3.0, 4.0);
            n1 = v.Normalize();
            n2 = v.Normalize(out magnitude);
            Assert.AreEqual(1.0, n1.Magnitude, 1e-14);
            Assert.AreEqual(1.0, n2.Magnitude, 1e-14);
            Assert.AreEqual(5.0, magnitude, 1e-14);
        }

        [Test]
        public void NormalizeZeroVector()
        {
            Vector3D v = new Vector3D(0.0, 0.0, 0.0);
            
            Vector3D n1 = v.Normalize();
            Assert.IsNaN(n1.X);
            Assert.IsNaN(n1.Y);
            Assert.IsNaN(n1.Z);
            Assert.IsTrue(n1.IsUndefined);

            double magnitude;
            Vector3D n2 = v.Normalize(out magnitude);
            Assert.IsNaN(n2.X);
            Assert.IsNaN(n2.Y);
            Assert.IsNaN(n2.Z);
            Assert.IsTrue(n2.IsUndefined);
            Assert.AreEqual(0.0, magnitude);
        }

        [Test]
        public void Add()
        {
            Vector3D v1 = new Vector3D(1.0, 2.0, 3.0);
            Vector3D v2 = new Vector3D(4.0, 5.0, 6.0);
            
            Vector3D v3 = v1 + v2;
            Assert.AreEqual(5.0, v3.X, 1e-14);
            Assert.AreEqual(7.0, v3.Y, 1e-14);
            Assert.AreEqual(9.0, v3.Z, 1e-14);

            Vector3D v4 = v1.Add(v2);
            Assert.AreEqual(5.0, v4.X, 1e-14);
            Assert.AreEqual(7.0, v4.Y, 1e-14);
            Assert.AreEqual(9.0, v4.Z, 1e-14);
        }

        [Test]
        public void Subtract()
        {
            Vector3D v1 = new Vector3D(1.0, 2.0, 3.0);
            Vector3D v2 = new Vector3D(4.0, 5.0, 6.0);

            Vector3D v3 = v1 - v2;
            Assert.AreEqual(-3.0, v3.X, 1e-14);
            Assert.AreEqual(-3.0, v3.Y, 1e-14);
            Assert.AreEqual(-3.0, v3.Z, 1e-14);

            Vector3D v4 = v1.Subtract(v2);
            Assert.AreEqual(-3.0, v4.X, 1e-14);
            Assert.AreEqual(-3.0, v4.Y, 1e-14);
            Assert.AreEqual(-3.0, v4.Z, 1e-14);
        }

        [Test]
        public void Multiply()
        {
            Vector3D v1 = new Vector3D(1.0, 2.0, 3.0);

            Vector3D v2 = v1 * 5.0;
            Assert.AreEqual(5.0, v2.X, 1e-14);
            Assert.AreEqual(10.0, v2.Y, 1e-14);
            Assert.AreEqual(15.0, v2.Z, 1e-14);

            Vector3D v3 = 5.0 * v1;
            Assert.AreEqual(5.0, v3.X, 1e-14);
            Assert.AreEqual(10.0, v3.Y, 1e-14);
            Assert.AreEqual(15.0, v3.Z, 1e-14);

            Vector3D v4 = v1.Multiply(5.0);
            Assert.AreEqual(5.0, v4.X, 1e-14);
            Assert.AreEqual(10.0, v4.Y, 1e-14);
            Assert.AreEqual(15.0, v4.Z, 1e-14);
        }

        [Test]
        public void MultiplyComponents()
        {
            Vector3D v1 = new Vector3D(1, 2, 3);
            Vector3D v2 = new Vector3D(4, 8, 12);

            Assert.AreEqual(new Vector3D(4, 16, 36), v1.MultiplyComponents(v2));
        }

        [Test]
        public void Divide()
        {
            Vector3D v1 = new Vector3D(10.0, 20.0, 30.0);

            Vector3D v2 = v1 / 5.0;
            Assert.AreEqual(2.0, v2.X, 1e-14);
            Assert.AreEqual(4.0, v2.Y, 1e-14);
            Assert.AreEqual(6.0, v2.Z, 1e-14);

            Vector3D v3 = v1.Divide(5.0);
            Assert.AreEqual(2.0, v3.X, 1e-14);
            Assert.AreEqual(4.0, v3.Y, 1e-14);
            Assert.AreEqual(6.0, v3.Z, 1e-14);
        }

        [Test]
        public void MostOrthogonalAxis()
        {
            Vector3D v1 = new Vector3D(1, 2, 3);
            Assert.AreEqual(Vector3D.UnitX, v1.MostOrthogonalAxis);

            Vector3D v2 = new Vector3D(-3, -1, -2);
            Assert.AreEqual(Vector3D.UnitY, v2.MostOrthogonalAxis);

            Vector3D v3 = new Vector3D(3, 2, 1);
            Assert.AreEqual(Vector3D.UnitZ, v3.MostOrthogonalAxis);
        }

        [Test]
        public void Zero()
        {
            Assert.AreEqual(0.0, Vector3D.Zero.X);
            Assert.AreEqual(0.0, Vector3D.Zero.Y);
            Assert.AreEqual(0.0, Vector3D.Zero.Z);
        }

        [Test]
        public void UnitX()
        {
            Assert.AreEqual(1.0, Vector3D.UnitX.X);
            Assert.AreEqual(0.0, Vector3D.UnitX.Y);
            Assert.AreEqual(0.0, Vector3D.UnitX.Z);
        }

        [Test]
        public void UnitY()
        {
            Assert.AreEqual(0.0, Vector3D.UnitY.X);
            Assert.AreEqual(1.0, Vector3D.UnitY.Y);
            Assert.AreEqual(0.0, Vector3D.UnitY.Z);
        }

        [Test]
        public void UnitZ()
        {
            Assert.AreEqual(0.0, Vector3D.UnitZ.X);
            Assert.AreEqual(0.0, Vector3D.UnitZ.Y);
            Assert.AreEqual(1.0, Vector3D.UnitZ.Z);
        }

        [Test]
        public void Undefined()
        {
            Assert.IsNaN(Vector3D.Undefined.X);
            Assert.IsNaN(Vector3D.Undefined.Y);
            Assert.IsNaN(Vector3D.Undefined.Z);
        }

        [Test]
        public void IsUndefined()
        {
            Assert.IsTrue(Vector3D.Undefined.IsUndefined);
            Assert.IsFalse(Vector3D.UnitX.IsUndefined);
            Assert.IsFalse(Vector3D.UnitY.IsUndefined);
            Assert.IsFalse(Vector3D.UnitZ.IsUndefined);
        }

        [Test]
        public void TestEquals()
        {
            Vector3D a = new Vector3D(1.0, 2.0, 3.0);
            Vector3D b = new Vector3D(4.0, 5.0, 6.0);
            Vector3D c = new Vector3D(1.0, 2.0, 3.0);

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
            Vector3D a = new Vector3D(1.0, 2.0, 3.0);
            Vector3D b = new Vector3D(4.0, 5.0, 6.0);
            Vector3D c = new Vector3D(1.0, 2.0, 3.0);

            Assert.AreEqual(a.GetHashCode(), c.GetHashCode());
            Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());
        }

        [Test]
        public void Dot()
        {
            Vector3D a = new Vector3D(1.0, 2.0, 3.0);
            Vector3D b = new Vector3D(4.0, 5.0, 6.0);

            double dot = a.Dot(b);
            Assert.AreEqual(1.0 * 4.0 + 2.0 * 5.0 + 3.0 * 6.0, dot, 1e-14);
        }

        [Test]
        public void Invert()
        {
            Vector3D a = new Vector3D(1.0, 2.0, 3.0);
            Vector3D invertedA1 = a.Invert();
            Assert.AreEqual(-1.0, invertedA1.X, 1e-14);
            Assert.AreEqual(-2.0, invertedA1.Y, 1e-14);
            Assert.AreEqual(-3.0, invertedA1.Z, 1e-14);
            Vector3D invertedA2 = -a;
            Assert.AreEqual(-1.0, invertedA2.X, 1e-14);
            Assert.AreEqual(-2.0, invertedA2.Y, 1e-14);
            Assert.AreEqual(-3.0, invertedA2.Z, 1e-14);

            Vector3D b = new Vector3D(-1.0, -2.0, -3.0);
            Vector3D invertedB1 = b.Invert();
            Assert.AreEqual(1.0, invertedB1.X, 1e-14);
            Assert.AreEqual(2.0, invertedB1.Y, 1e-14);
            Assert.AreEqual(3.0, invertedB1.Z, 1e-14);
            Vector3D invertedB2 = -b;
            Assert.AreEqual(1.0, invertedB2.X, 1e-14);
            Assert.AreEqual(2.0, invertedB2.Y, 1e-14);
            Assert.AreEqual(3.0, invertedB2.Z, 1e-14);
        }

        [Test]
        public void ToVector3S()
        {
            Vector3D a = new Vector3D(1.0, 2.0, 3.0);
            Vector3S sA = a.ToVector3S();
            Assert.AreEqual(1.0f, sA.X, 1e-7);
            Assert.AreEqual(2.0f, sA.Y, 1e-7);
            Assert.AreEqual(3.0f, sA.Z, 1e-7);
        }

        [Test]
        public void ToVector3H()
        {
            Vector3D a = new Vector3D(1.0, 2.0, 3.0);
            Vector3H sA = a.ToVector3H();
            Assert.AreEqual((Half)1.0, sA.X, 1e-7);
            Assert.AreEqual((Half)2.0, sA.Y, 1e-7);
            Assert.AreEqual((Half)3.0, sA.Z, 1e-7);
        }

        [Test]
        public void Cross()
        {
            Vector3D a = new Vector3D(1.0, 0.0, 0.0);
            Vector3D b = new Vector3D(0.0, 1.0, 0.0);
            
            Vector3D cross1 = a.Cross(b);
            Assert.AreEqual(0.0, cross1.X, 1e-14);
            Assert.AreEqual(0.0, cross1.Y, 1e-14);
            Assert.AreEqual(1.0, cross1.Z, 1e-14);

            Vector3D cross2 = b.Cross(a);
            Assert.AreEqual(0.0, cross2.X, 1e-14);
            Assert.AreEqual(0.0, cross2.Y, 1e-14);
            Assert.AreEqual(-1.0, cross2.Z, 1e-14);
        }

        [Test]
        public void TestToString()
        {
            CultureInfo originalCulture = Thread.CurrentThread.CurrentCulture;
            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("tr-TR");
                Vector3D a = new Vector3D(1.23, 2.34, 3.45);
                Assert.AreEqual("(1,23, 2,34, 3,45)", a.ToString());
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = originalCulture;
            }
        }

        [Test]
        public void EqualsEpsilon()
        {
            Vector3D a = new Vector3D(1.23, 2.34, 3.45);
            Vector3D b = new Vector3D(1.24, 2.35, 3.46);
            Assert.IsTrue(a.EqualsEpsilon(b, 0.011));
            Assert.IsFalse(a.EqualsEpsilon(b, 0.009));
        }

        [Test]
        public void XY()
        {
            Vector3D a = new Vector3D(1.23, 2.34, 3.45);
            Vector2D xy = a.XY;
            Assert.AreEqual(1.23, xy.X, 1e-14);
            Assert.AreEqual(2.34, xy.Y, 1e-14);
        }
    }
}
