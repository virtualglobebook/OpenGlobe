#region License
//
// (C) Copyright 2010 Patrick Cozzi, Deron Ohlarik, and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0f.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using NUnit.Framework;
using System.Globalization;
using System.Threading;

namespace OpenGlobe.Core
{
    [TestFixture]
    public class Vector4FTests
    {
        [Test]
        public void Construct0()
        {
            Vector4F v = new Vector4F(1.0f, 2.0f, 3.0f, 4.0f);
            Assert.AreEqual(1.0f, v.X);
            Assert.AreEqual(2.0f, v.Y);
            Assert.AreEqual(3.0f, v.Z);
            Assert.AreEqual(4.0f, v.W);
        }

        [Test]
        public void Construct1()
        {
            Vector4F v = new Vector4F(new Vector3F(1.0f, 2.0f, 3.0f), 4.0f);
            Assert.AreEqual(1.0f, v.X);
            Assert.AreEqual(2.0f, v.Y);
            Assert.AreEqual(3.0f, v.Z);
            Assert.AreEqual(4.0f, v.W);
        }

        [Test]
        public void Construct2()
        {
            Vector4F v = new Vector4F(new Vector2F(1.0f, 2.0f), 3.0f, 4.0f);
            Assert.AreEqual(1.0f, v.X);
            Assert.AreEqual(2.0f, v.Y);
            Assert.AreEqual(3.0f, v.Z);
            Assert.AreEqual(4.0f, v.W);
        }

        [Test]
        public void Magnitude()
        {
            Vector4F v = new Vector4F(3.0f, 4.0f, 0.0f, 0.0f);
            Assert.AreEqual(25.0f, v.MagnitudeSquared, 1e-14);
            Assert.AreEqual(5.0f, v.Magnitude, 1e-14);

            v = new Vector4F(3.0f, 0.0f, 4.0f, 0.0f);
            Assert.AreEqual(25.0f, v.MagnitudeSquared, 1e-14);
            Assert.AreEqual(5.0f, v.Magnitude, 1e-14);

            v = new Vector4F(0.0f, 3.0f, 4.0f, 0.0f);
            Assert.AreEqual(25.0f, v.MagnitudeSquared, 1e-14);
            Assert.AreEqual(5.0f, v.Magnitude, 1e-14);

            v = new Vector4F(0.0f, 0.0f, 3.0f, 4.0f);
            Assert.AreEqual(25.0f, v.MagnitudeSquared, 1e-14);
            Assert.AreEqual(5.0f, v.Magnitude, 1e-14);
        }

        [Test]
        public void Normalize()
        {
            Vector4F v, n1, n2;
            float magnitude;

            v = new Vector4F(3.0f, 4.0f, 0.0f, 0.0f);
            n1 = v.Normalize();
            n2 = v.Normalize(out magnitude);
            Assert.AreEqual(1.0f, n1.Magnitude, 1e-14);
            Assert.AreEqual(1.0f, n2.Magnitude, 1e-14);
            Assert.AreEqual(5.0f, magnitude, 1e-14);

            v = new Vector4F(3.0f, 0.0f, 4.0f, 0.0f);
            n1 = v.Normalize();
            n2 = v.Normalize(out magnitude);
            Assert.AreEqual(1.0f, n1.Magnitude, 1e-14);
            Assert.AreEqual(1.0f, n2.Magnitude, 1e-14);
            Assert.AreEqual(5.0f, magnitude, 1e-14);

            v = new Vector4F(0.0f, 3.0f, 4.0f, 0.0f);
            n1 = v.Normalize();
            n2 = v.Normalize(out magnitude);
            Assert.AreEqual(1.0f, n1.Magnitude, 1e-14);
            Assert.AreEqual(1.0f, n2.Magnitude, 1e-14);
            Assert.AreEqual(5.0f, magnitude, 1e-14);

            v = new Vector4F(0.0f, 0.0f, 3.0f, 4.0f);
            n1 = v.Normalize();
            n2 = v.Normalize(out magnitude);
            Assert.AreEqual(1.0f, n1.Magnitude, 1e-14);
            Assert.AreEqual(1.0f, n2.Magnitude, 1e-14);
            Assert.AreEqual(5.0f, magnitude, 1e-14);
        }

        [Test]
        public void NormalizeZeroVector()
        {
            Vector4F v = new Vector4F(0.0f, 0.0f, 0.0f, 0.0f);

            Vector4F n1 = v.Normalize();
            Assert.IsNaN(n1.X);
            Assert.IsNaN(n1.Y);
            Assert.IsNaN(n1.Z);
            Assert.IsTrue(n1.IsUndefined);

            float magnitude;
            Vector4F n2 = v.Normalize(out magnitude);
            Assert.IsNaN(n2.X);
            Assert.IsNaN(n2.Y);
            Assert.IsNaN(n2.Z);
            Assert.IsTrue(n2.IsUndefined);
            Assert.AreEqual(0.0f, magnitude);
        }

        [Test]
        public void Add()
        {
            Vector4F v1 = new Vector4F(1.0f, 2.0f, 3.0f, 4.0f);
            Vector4F v2 = new Vector4F(4.0f, 5.0f, 6.0f, 7.0f);

            Vector4F v3 = v1 + v2;
            Assert.AreEqual(5.0f, v3.X, 1e-14);
            Assert.AreEqual(7.0f, v3.Y, 1e-14);
            Assert.AreEqual(9.0f, v3.Z, 1e-14);
            Assert.AreEqual(11.0f, v3.W, 1e-14);

            Vector4F v4 = v1.Add(v2);
            Assert.AreEqual(5.0f, v4.X, 1e-14);
            Assert.AreEqual(7.0f, v4.Y, 1e-14);
            Assert.AreEqual(9.0f, v4.Z, 1e-14);
            Assert.AreEqual(11.0f, v4.W, 1e-14);
        }

        [Test]
        public void Subtract()
        {
            Vector4F v1 = new Vector4F(1.0f, 2.0f, 3.0f, 4.0f);
            Vector4F v2 = new Vector4F(4.0f, 5.0f, 6.0f, 7.0f);

            Vector4F v3 = v1 - v2;
            Assert.AreEqual(-3.0f, v3.X, 1e-14);
            Assert.AreEqual(-3.0f, v3.Y, 1e-14);
            Assert.AreEqual(-3.0f, v3.Z, 1e-14);
            Assert.AreEqual(-3.0f, v3.W, 1e-14);

            Vector4F v4 = v1.Subtract(v2);
            Assert.AreEqual(-3.0f, v4.X, 1e-14);
            Assert.AreEqual(-3.0f, v4.Y, 1e-14);
            Assert.AreEqual(-3.0f, v4.Z, 1e-14);
            Assert.AreEqual(-3.0f, v4.W, 1e-14);
        }

        [Test]
        public void Multiply()
        {
            Vector4F v1 = new Vector4F(1.0f, 2.0f, 3.0f, 4.0f);

            Vector4F v2 = v1 * 5.0f;
            Assert.AreEqual(5.0f, v2.X, 1e-14);
            Assert.AreEqual(10.0f, v2.Y, 1e-14);
            Assert.AreEqual(15.0f, v2.Z, 1e-14);
            Assert.AreEqual(20.0f, v2.W, 1e-14);

            Vector4F v3 = 5.0f * v1;
            Assert.AreEqual(5.0f, v3.X, 1e-14);
            Assert.AreEqual(10.0f, v3.Y, 1e-14);
            Assert.AreEqual(15.0f, v3.Z, 1e-14);
            Assert.AreEqual(20.0f, v3.W, 1e-14);

            Vector4F v4 = v1.Multiply(5.0f);
            Assert.AreEqual(5.0f, v4.X, 1e-14);
            Assert.AreEqual(10.0f, v4.Y, 1e-14);
            Assert.AreEqual(15.0f, v4.Z, 1e-14);
            Assert.AreEqual(20.0f, v4.W, 1e-14);
        }

        [Test]
        public void MultiplyComponents()
        {
            Vector4F v1 = new Vector4F(1, 2, 3, 4);
            Vector4F v2 = new Vector4F(4, 8, 12, 16);

            Assert.AreEqual(new Vector4F(4, 16, 36, 64), v1.MultiplyComponents(v2));
        }

        [Test]
        public void Divide()
        {
            Vector4F v1 = new Vector4F(10.0f, 20.0f, 30.0f, 40.0f);

            Vector4F v2 = v1 / 5.0f;
            Assert.AreEqual(2.0f, v2.X, 1e-14);
            Assert.AreEqual(4.0f, v2.Y, 1e-14);
            Assert.AreEqual(6.0f, v2.Z, 1e-14);
            Assert.AreEqual(8.0f, v2.W, 1e-14);

            Vector4F v3 = v1.Divide(5.0f);
            Assert.AreEqual(2.0f, v3.X, 1e-14);
            Assert.AreEqual(4.0f, v3.Y, 1e-14);
            Assert.AreEqual(6.0f, v3.Z, 1e-14);
            Assert.AreEqual(8.0f, v3.W, 1e-14);
        }

        [Test]
        public void MostOrthogonalAxis()
        {
            Vector4F v1 = new Vector4F(1, 2, 3, 4);
            Assert.AreEqual(Vector4F.UnitX, v1.MostOrthogonalAxis);

            Vector4F v2 = new Vector4F(-3, -1, -2, -4);
            Assert.AreEqual(Vector4F.UnitY, v2.MostOrthogonalAxis);

            Vector4F v3 = new Vector4F(3, 2, 1, 4);
            Assert.AreEqual(Vector4F.UnitZ, v3.MostOrthogonalAxis);

            Vector4F v4 = new Vector4F(3, 2, 4, 1);
            Assert.AreEqual(Vector4F.UnitW, v4.MostOrthogonalAxis);
        }

        [Test]
        public void Zero()
        {
            Assert.AreEqual(0.0f, Vector4F.Zero.X);
            Assert.AreEqual(0.0f, Vector4F.Zero.Y);
            Assert.AreEqual(0.0f, Vector4F.Zero.Z);
            Assert.AreEqual(0.0f, Vector4F.Zero.W);
        }

        [Test]
        public void UnitX()
        {
            Assert.AreEqual(1.0f, Vector4F.UnitX.X);
            Assert.AreEqual(0.0f, Vector4F.UnitX.Y);
            Assert.AreEqual(0.0f, Vector4F.UnitX.Z);
            Assert.AreEqual(0.0f, Vector4F.UnitX.W);
        }

        [Test]
        public void UnitY()
        {
            Assert.AreEqual(0.0f, Vector4F.UnitY.X);
            Assert.AreEqual(1.0f, Vector4F.UnitY.Y);
            Assert.AreEqual(0.0f, Vector4F.UnitY.Z);
            Assert.AreEqual(0.0f, Vector4F.UnitY.W);
        }

        [Test]
        public void UnitZ()
        {
            Assert.AreEqual(0.0f, Vector4F.UnitZ.X);
            Assert.AreEqual(0.0f, Vector4F.UnitZ.Y);
            Assert.AreEqual(1.0f, Vector4F.UnitZ.Z);
            Assert.AreEqual(0.0f, Vector4F.UnitZ.W);
        }

        [Test]
        public void UnitW()
        {
            Assert.AreEqual(0.0f, Vector4F.UnitW.X);
            Assert.AreEqual(0.0f, Vector4F.UnitW.Y);
            Assert.AreEqual(0.0f, Vector4F.UnitW.Z);
            Assert.AreEqual(1.0f, Vector4F.UnitW.W);
        }

        [Test]
        public void Undefined()
        {
            Assert.IsNaN(Vector4F.Undefined.X);
            Assert.IsNaN(Vector4F.Undefined.Y);
            Assert.IsNaN(Vector4F.Undefined.Z);
            Assert.IsNaN(Vector4F.Undefined.W);
        }

        [Test]
        public void IsUndefined()
        {
            Assert.IsTrue(Vector4F.Undefined.IsUndefined);
            Assert.IsFalse(Vector4F.UnitX.IsUndefined);
            Assert.IsFalse(Vector4F.UnitY.IsUndefined);
            Assert.IsFalse(Vector4F.UnitZ.IsUndefined);
            Assert.IsFalse(Vector4F.UnitW.IsUndefined);
        }

        [Test]
        public void TestEquals()
        {
            Vector4F a = new Vector4F(1.0f, 2.0f, 3.0f, 4.0f);
            Vector4F b = new Vector4F(4.0f, 5.0f, 6.0f, 7.0f);
            Vector4F c = new Vector4F(1.0f, 2.0f, 3.0f, 4.0f);

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
            Vector4F a = new Vector4F(1.0f, 2.0f, 3.0f, 4.0f);
            Vector4F b = new Vector4F(4.0f, 5.0f, 6.0f, 7.0f);
            Vector4F c = new Vector4F(1.0f, 2.0f, 3.0f, 4.0f);

            Assert.AreEqual(a.GetHashCode(), c.GetHashCode());
            Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());
        }

        [Test]
        public void Dot()
        {
            Vector4F a = new Vector4F(1.0f, 2.0f, 3.0f, 4.0f);
            Vector4F b = new Vector4F(4.0f, 5.0f, 6.0f, 7.0f);

            double dot = a.Dot(b);
            Assert.AreEqual(1.0f * 4.0f + 2.0f * 5.0f + 3.0f * 6.0f + 4.0f * 7.0f, dot, 1e-14);
        }

        [Test]
        public void Negate()
        {
            Vector4F a = new Vector4F(1.0f, 2.0f, 3.0f, 4.0f);
            Vector4F negatedA1 = a.Negate();
            Assert.AreEqual(-1.0f, negatedA1.X, 1e-14);
            Assert.AreEqual(-2.0f, negatedA1.Y, 1e-14);
            Assert.AreEqual(-3.0f, negatedA1.Z, 1e-14);
            Assert.AreEqual(-4.0f, negatedA1.W, 1e-14);
            Vector4F negatedA2 = -a;
            Assert.AreEqual(-1.0f, negatedA2.X, 1e-14);
            Assert.AreEqual(-2.0f, negatedA2.Y, 1e-14);
            Assert.AreEqual(-3.0f, negatedA2.Z, 1e-14);
            Assert.AreEqual(-4.0f, negatedA2.W, 1e-14);

            Vector4F b = new Vector4F(-1.0f, -2.0f, -3.0f, -4.0f);
            Vector4F negatedB1 = b.Negate();
            Assert.AreEqual(1.0f, negatedB1.X, 1e-14);
            Assert.AreEqual(2.0f, negatedB1.Y, 1e-14);
            Assert.AreEqual(3.0f, negatedB1.Z, 1e-14);
            Assert.AreEqual(4.0f, negatedB1.W, 1e-14);
            Vector4F negatedB2 = -b;
            Assert.AreEqual(1.0f, negatedB2.X, 1e-14);
            Assert.AreEqual(2.0f, negatedB2.Y, 1e-14);
            Assert.AreEqual(3.0f, negatedB2.Z, 1e-14);
            Assert.AreEqual(4.0f, negatedB2.W, 1e-14);
        }

        [Test]
        public void ToVector4D()
        {
            Vector4F a = new Vector4F(1.0f, 2.0f, 3.0f, 4.0f);
            Vector4D dA = a.ToVector4D();
            Assert.AreEqual(1.0, dA.X, 1e-7);
            Assert.AreEqual(2.0, dA.Y, 1e-7);
            Assert.AreEqual(3.0, dA.Z, 1e-7);
            Assert.AreEqual(4.0, dA.W, 1e-7);
        }

        [Test]
        public void ToVector4H()
        {
            Vector4F a = new Vector4F(1.0f, 2.0f, 3.0f, 4.0f);
            Vector4H sA = a.ToVector4H();
            Assert.AreEqual((Half)1.0f, sA.X, 1e-7);
            Assert.AreEqual((Half)2.0f, sA.Y, 1e-7);
            Assert.AreEqual((Half)3.0f, sA.Z, 1e-7);
            Assert.AreEqual((Half)4.0f, sA.W, 1e-7);
        }

        [Test]
        public void ToVector4I()
        {
            Vector4F a = new Vector4F(1.0f, 2.0f, 3.0f, 4.0f);
            Vector4I dA = a.ToVector4I();
            Assert.AreEqual(1, dA.X);
            Assert.AreEqual(2, dA.Y);
            Assert.AreEqual(3, dA.Z);
            Assert.AreEqual(4, dA.W);
        }

        [Test]
        public void ToVector4B()
        {
            Vector4F a = new Vector4F(1.0f, 0.0f, 0.0f, 1.0f);
            Vector4B dA = a.ToVector4B();
            Assert.IsTrue(dA.X);
            Assert.IsFalse(dA.Y);
            Assert.IsFalse(dA.Z);
            Assert.IsTrue(dA.W);
        }

#if !CSToJava
        [Test]
        public void TestToString()
        {
            CultureInfo originalCulture = Thread.CurrentThread.CurrentCulture;
            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("tr-TR");
                Vector4F a = new Vector4F(1.23f, 2.34f, 3.45f, 4.56f);
                Assert.AreEqual("(1,23, 2,34, 3,45, 4,56)", a.ToString());
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
            Vector4F a = new Vector4F(1.23f, 2.34f, 3.45f, 4.56f);
            Vector4F b = new Vector4F(1.24f, 2.35f, 3.46f, 4.57f);
            Assert.IsTrue(a.EqualsEpsilon(b, 0.011f));
            Assert.IsFalse(a.EqualsEpsilon(b, 0.009f));
        }

        [Test]
        public void XY()
        {
            Vector4F a = new Vector4F(1.23f, 2.34f, 3.45f, 4.56f);
            Vector2F xy = a.XY;
            Assert.AreEqual(1.23f, xy.X, 1e-14);
            Assert.AreEqual(2.34f, xy.Y, 1e-14);
        }

        [Test]
        public void XYZ()
        {
            Vector4F a = new Vector4F(1.23f, 2.34f, 3.45f, 4.56f);
            Vector3F xyz = a.XYZ;
            Assert.AreEqual(1.23f, xyz.X, 1e-14);
            Assert.AreEqual(2.34f, xyz.Y, 1e-14);
            Assert.AreEqual(3.45f, xyz.Z, 1e-14);
        }
    }
}
