#region License
//
// (C) Copyright 2010 Patrick Cozzi, Deron Ohlarik, and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using NUnit.Framework;

namespace MiniGlobe.Core
{
    [TestFixture]
    public class Vector4DTests
    {
        [Test]
        public void Construct()
        {
            Vector4D v = new Vector4D(1.0, 2.0, 3.0, 4.0);
            Assert.AreEqual(1.0, v.X);
            Assert.AreEqual(2.0, v.Y);
            Assert.AreEqual(3.0, v.Z);
        }

        [Test]
        public void Magnitude()
        {
            Vector4D v = new Vector4D(3.0, 4.0, 0.0, 0.0);
            Assert.AreEqual(25.0, v.MagnitudeSquared, 1e-14);
            Assert.AreEqual(5.0, v.Magnitude, 1e-14);

            v = new Vector4D(3.0, 0.0, 4.0, 0.0);
            Assert.AreEqual(25.0, v.MagnitudeSquared, 1e-14);
            Assert.AreEqual(5.0, v.Magnitude, 1e-14);

            v = new Vector4D(0.0, 3.0, 4.0, 0.0);
            Assert.AreEqual(25.0, v.MagnitudeSquared, 1e-14);
            Assert.AreEqual(5.0, v.Magnitude, 1e-14);

            v = new Vector4D(0.0, 0.0, 3.0, 4.0);
            Assert.AreEqual(25.0, v.MagnitudeSquared, 1e-14);
            Assert.AreEqual(5.0, v.Magnitude, 1e-14);
        }

        [Test]
        public void Normalize()
        {
            Vector4D v, n1, n2;
            double magnitude;

            v = new Vector4D(3.0, 4.0, 0.0, 0.0);
            n1 = v.Normalize();
            n2 = v.Normalize(out magnitude);
            Assert.AreEqual(1.0, n1.Magnitude, 1e-14);
            Assert.AreEqual(1.0, n2.Magnitude, 1e-14);
            Assert.AreEqual(5.0, magnitude, 1e-14);

            v = new Vector4D(3.0, 0.0, 4.0, 0.0);
            n1 = v.Normalize();
            n2 = v.Normalize(out magnitude);
            Assert.AreEqual(1.0, n1.Magnitude, 1e-14);
            Assert.AreEqual(1.0, n2.Magnitude, 1e-14);
            Assert.AreEqual(5.0, magnitude, 1e-14);

            v = new Vector4D(0.0, 3.0, 4.0, 0.0);
            n1 = v.Normalize();
            n2 = v.Normalize(out magnitude);
            Assert.AreEqual(1.0, n1.Magnitude, 1e-14);
            Assert.AreEqual(1.0, n2.Magnitude, 1e-14);
            Assert.AreEqual(5.0, magnitude, 1e-14);

            v = new Vector4D(0.0, 0.0, 3.0, 4.0);
            n1 = v.Normalize();
            n2 = v.Normalize(out magnitude);
            Assert.AreEqual(1.0, n1.Magnitude, 1e-14);
            Assert.AreEqual(1.0, n2.Magnitude, 1e-14);
            Assert.AreEqual(5.0, magnitude, 1e-14);
        }

        [Test]
        public void NormalizeZeroVector()
        {
            Vector4D v = new Vector4D(0.0, 0.0, 0.0, 0.0);
            
            Vector4D n1 = v.Normalize();
            Assert.IsNaN(n1.X);
            Assert.IsNaN(n1.Y);
            Assert.IsNaN(n1.Z);
            Assert.IsTrue(n1.IsUndefined);

            double magnitude;
            Vector4D n2 = v.Normalize(out magnitude);
            Assert.IsNaN(n2.X);
            Assert.IsNaN(n2.Y);
            Assert.IsNaN(n2.Z);
            Assert.IsTrue(n2.IsUndefined);
            Assert.AreEqual(0.0, magnitude);
        }

        [Test]
        public void Add()
        {
            Vector4D v1 = new Vector4D(1.0, 2.0, 3.0, 4.0);
            Vector4D v2 = new Vector4D(4.0, 5.0, 6.0, 7.0);
            
            Vector4D v3 = v1 + v2;
            Assert.AreEqual(5.0, v3.X, 1e-14);
            Assert.AreEqual(7.0, v3.Y, 1e-14);
            Assert.AreEqual(9.0, v3.Z, 1e-14);
            Assert.AreEqual(11.0, v3.W, 1e-14);

            Vector4D v4 = v1.Add(v2);
            Assert.AreEqual(5.0, v4.X, 1e-14);
            Assert.AreEqual(7.0, v4.Y, 1e-14);
            Assert.AreEqual(9.0, v4.Z, 1e-14);
            Assert.AreEqual(11.0, v4.W, 1e-14);
        }

        [Test]
        public void Subtract()
        {
            Vector4D v1 = new Vector4D(1.0, 2.0, 3.0, 4.0);
            Vector4D v2 = new Vector4D(4.0, 5.0, 6.0, 7.0);

            Vector4D v3 = v1 - v2;
            Assert.AreEqual(-3.0, v3.X, 1e-14);
            Assert.AreEqual(-3.0, v3.Y, 1e-14);
            Assert.AreEqual(-3.0, v3.Z, 1e-14);
            Assert.AreEqual(-3.0, v3.W, 1e-14);

            Vector4D v4 = v1.Subtract(v2);
            Assert.AreEqual(-3.0, v4.X, 1e-14);
            Assert.AreEqual(-3.0, v4.Y, 1e-14);
            Assert.AreEqual(-3.0, v4.Z, 1e-14);
            Assert.AreEqual(-3.0, v4.W, 1e-14);
        }

        [Test]
        public void Multiply()
        {
            Vector4D v1 = new Vector4D(1.0, 2.0, 3.0, 4.0);

            Vector4D v2 = v1 * 5.0;
            Assert.AreEqual(5.0, v2.X, 1e-14);
            Assert.AreEqual(10.0, v2.Y, 1e-14);
            Assert.AreEqual(15.0, v2.Z, 1e-14);
            Assert.AreEqual(20.0, v2.W, 1e-14);

            Vector4D v3 = 5.0 * v1;
            Assert.AreEqual(5.0, v3.X, 1e-14);
            Assert.AreEqual(10.0, v3.Y, 1e-14);
            Assert.AreEqual(15.0, v3.Z, 1e-14);
            Assert.AreEqual(20.0, v3.W, 1e-14);

            Vector4D v4 = v1.Multiply(5.0);
            Assert.AreEqual(5.0, v4.X, 1e-14);
            Assert.AreEqual(10.0, v4.Y, 1e-14);
            Assert.AreEqual(15.0, v4.Z, 1e-14);
            Assert.AreEqual(20.0, v4.W, 1e-14);
        }

        [Test]
        public void MultiplyComponents()
        {
            Vector4D v1 = new Vector4D(1, 2, 3, 4);
            Vector4D v2 = new Vector4D(4, 8, 12, 16);

            Assert.AreEqual(new Vector4D(4, 16, 36, 64), v1.MultiplyComponents(v2));
        }

        [Test]
        public void Divide()
        {
            Vector4D v1 = new Vector4D(10.0, 20.0, 30.0, 40.0);

            Vector4D v2 = v1 / 5.0;
            Assert.AreEqual(2.0, v2.X, 1e-14);
            Assert.AreEqual(4.0, v2.Y, 1e-14);
            Assert.AreEqual(6.0, v2.Z, 1e-14);
            Assert.AreEqual(8.0, v2.W, 1e-14);

            Vector4D v3 = v1.Divide(5.0);
            Assert.AreEqual(2.0, v3.X, 1e-14);
            Assert.AreEqual(4.0, v3.Y, 1e-14);
            Assert.AreEqual(6.0, v3.Z, 1e-14);
            Assert.AreEqual(8.0, v3.W, 1e-14);
        }

        [Test]
        public void MostOrthogonalAxis()
        {
            Vector4D v1 = new Vector4D(1, 2, 3, 4);
            Assert.AreEqual(Vector4D.UnitX, v1.MostOrthogonalAxis);

            Vector4D v2 = new Vector4D(-3, -1, -2, -4);
            Assert.AreEqual(Vector4D.UnitY, v2.MostOrthogonalAxis);

            Vector4D v3 = new Vector4D(3, 2, 1, 4);
            Assert.AreEqual(Vector4D.UnitZ, v3.MostOrthogonalAxis);

            Vector4D v4 = new Vector4D(3, 2, 4, 1);
            Assert.AreEqual(Vector4D.UnitW, v4.MostOrthogonalAxis);
        }

        [Test]
        public void Zero()
        {
            Assert.AreEqual(0.0, Vector4D.Zero.X);
            Assert.AreEqual(0.0, Vector4D.Zero.Y);
            Assert.AreEqual(0.0, Vector4D.Zero.Z);
            Assert.AreEqual(0.0, Vector4D.Zero.W);
        }

        [Test]
        public void UnitX()
        {
            Assert.AreEqual(1.0, Vector4D.UnitX.X);
            Assert.AreEqual(0.0, Vector4D.UnitX.Y);
            Assert.AreEqual(0.0, Vector4D.UnitX.Z);
            Assert.AreEqual(0.0, Vector4D.UnitX.W);
        }

        [Test]
        public void UnitY()
        {
            Assert.AreEqual(0.0, Vector4D.UnitY.X);
            Assert.AreEqual(1.0, Vector4D.UnitY.Y);
            Assert.AreEqual(0.0, Vector4D.UnitY.Z);
            Assert.AreEqual(0.0, Vector4D.UnitY.W);
        }

        [Test]
        public void UnitZ()
        {
            Assert.AreEqual(0.0, Vector4D.UnitZ.X);
            Assert.AreEqual(0.0, Vector4D.UnitZ.Y);
            Assert.AreEqual(1.0, Vector4D.UnitZ.Z);
            Assert.AreEqual(0.0, Vector4D.UnitZ.W);
        }

        [Test]
        public void UnitW()
        {
            Assert.AreEqual(0.0, Vector4D.UnitW.X);
            Assert.AreEqual(0.0, Vector4D.UnitW.Y);
            Assert.AreEqual(0.0, Vector4D.UnitW.Z);
            Assert.AreEqual(1.0, Vector4D.UnitW.W);
        }

        [Test]
        public void Undefined()
        {
            Assert.IsNaN(Vector4D.Undefined.X);
            Assert.IsNaN(Vector4D.Undefined.Y);
            Assert.IsNaN(Vector4D.Undefined.Z);
            Assert.IsNaN(Vector4D.Undefined.W);
        }

        [Test]
        public void IsUndefined()
        {
            Assert.IsTrue(Vector4D.Undefined.IsUndefined);
            Assert.IsFalse(Vector4D.UnitX.IsUndefined);
            Assert.IsFalse(Vector4D.UnitY.IsUndefined);
            Assert.IsFalse(Vector4D.UnitZ.IsUndefined);
            Assert.IsFalse(Vector4D.UnitW.IsUndefined);
        }

        [Test]
        public void TestEquals()
        {
            Vector4D a = new Vector4D(1.0, 2.0, 3.0, 4.0);
            Vector4D b = new Vector4D(4.0, 5.0, 6.0, 7.0);
            Vector4D c = new Vector4D(1.0, 2.0, 3.0, 4.0);

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
            Vector4D a = new Vector4D(1.0, 2.0, 3.0, 4.0);
            Vector4D b = new Vector4D(4.0, 5.0, 6.0, 7.0);
            Vector4D c = new Vector4D(1.0, 2.0, 3.0, 4.0);

            Assert.AreEqual(a.GetHashCode(), c.GetHashCode());
            Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());
        }

        [Test]
        public void Dot()
        {
            Vector4D a = new Vector4D(1.0, 2.0, 3.0, 4.0);
            Vector4D b = new Vector4D(4.0, 5.0, 6.0, 7.0);

            double dot = a.Dot(b);
            Assert.AreEqual(1.0 * 4.0 + 2.0 * 5.0 + 3.0 * 6.0 + 4.0 * 7.0, dot, 1e-14);
        }
    }
}
