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
    public class Vector4ITests
    {
        [Test]
        public void Construct()
        {
            Vector4I v = new Vector4I(1, 2, 3, 4);
            Assert.AreEqual(1, v.X);
            Assert.AreEqual(2, v.Y);
            Assert.AreEqual(3, v.Z);
        }

        [Test]
        public void Magnitude()
        {
            Vector4I v = new Vector4I(3, 4, 0, 0);
            Assert.AreEqual(25, v.MagnitudeSquared);
            Assert.AreEqual(5, v.Magnitude, 1e-14);

            v = new Vector4I(3, 0, 4, 0);
            Assert.AreEqual(25, v.MagnitudeSquared);
            Assert.AreEqual(5, v.Magnitude, 1e-14);

            v = new Vector4I(0, 3, 4, 0);
            Assert.AreEqual(25, v.MagnitudeSquared);
            Assert.AreEqual(5, v.Magnitude, 1e-14);

            v = new Vector4I(0, 0, 3, 4);
            Assert.AreEqual(25, v.MagnitudeSquared);
            Assert.AreEqual(5, v.Magnitude, 1e-14);
        }

        [Test]
        public void Add()
        {
            Vector4I v1 = new Vector4I(1, 2, 3, 4);
            Vector4I v2 = new Vector4I(4, 5, 6, 7);

            Vector4I v3 = v1 + v2;
            Assert.AreEqual(5, v3.X);
            Assert.AreEqual(7, v3.Y);
            Assert.AreEqual(9, v3.Z);
            Assert.AreEqual(11, v3.W);

            Vector4I v4 = v1.Add(v2);
            Assert.AreEqual(5, v4.X);
            Assert.AreEqual(7, v4.Y);
            Assert.AreEqual(9, v4.Z);
            Assert.AreEqual(11, v4.W);
        }

        [Test]
        public void Subtract()
        {
            Vector4I v1 = new Vector4I(1, 2, 3, 4);
            Vector4I v2 = new Vector4I(4, 5, 6, 7);

            Vector4I v3 = v1 - v2;
            Assert.AreEqual(-3, v3.X);
            Assert.AreEqual(-3, v3.Y);
            Assert.AreEqual(-3, v3.Z);
            Assert.AreEqual(-3, v3.W);

            Vector4I v4 = v1.Subtract(v2);
            Assert.AreEqual(-3, v4.X);
            Assert.AreEqual(-3, v4.Y);
            Assert.AreEqual(-3, v4.Z);
            Assert.AreEqual(-3, v4.W);
        }

        [Test]
        public void Multiply()
        {
            Vector4I v1 = new Vector4I(1, 2, 3, 4);

            Vector4I v2 = v1 * 5;
            Assert.AreEqual(5, v2.X);
            Assert.AreEqual(10, v2.Y);
            Assert.AreEqual(15, v2.Z);
            Assert.AreEqual(20, v2.W);

            Vector4I v3 = 5 * v1;
            Assert.AreEqual(5, v3.X);
            Assert.AreEqual(10, v3.Y);
            Assert.AreEqual(15, v3.Z);
            Assert.AreEqual(20, v3.W);

            Vector4I v4 = v1.Multiply(5);
            Assert.AreEqual(5, v4.X);
            Assert.AreEqual(10, v4.Y);
            Assert.AreEqual(15, v4.Z);
            Assert.AreEqual(20, v4.W);
        }

        [Test]
        public void MultiplyComponents()
        {
            Vector4I v1 = new Vector4I(1, 2, 3, 4);
            Vector4I v2 = new Vector4I(4, 8, 12, 16);

            Assert.AreEqual(new Vector4I(4, 16, 36, 64), v1.MultiplyComponents(v2));
        }

        [Test]
        public void Divide()
        {
            Vector4I v1 = new Vector4I(10, 20, 30, 40);

            Vector4I v2 = v1 / 5;
            Assert.AreEqual(2, v2.X);
            Assert.AreEqual(4, v2.Y);
            Assert.AreEqual(6, v2.Z);
            Assert.AreEqual(8, v2.W);

            Vector4I v3 = v1.Divide(5);
            Assert.AreEqual(2, v3.X);
            Assert.AreEqual(4, v3.Y);
            Assert.AreEqual(6, v3.Z);
            Assert.AreEqual(8, v3.W);
        }

        [Test]
        public void MostOrthogonalAxis()
        {
            Vector4I v1 = new Vector4I(1, 2, 3, 4);
            Assert.AreEqual(Vector4I.UnitX, v1.MostOrthogonalAxis);

            Vector4I v2 = new Vector4I(-3, -1, -2, -4);
            Assert.AreEqual(Vector4I.UnitY, v2.MostOrthogonalAxis);

            Vector4I v3 = new Vector4I(3, 2, 1, 4);
            Assert.AreEqual(Vector4I.UnitZ, v3.MostOrthogonalAxis);

            Vector4I v4 = new Vector4I(3, 2, 4, 1);
            Assert.AreEqual(Vector4I.UnitW, v4.MostOrthogonalAxis);
        }

        [Test]
        public void Zero()
        {
            Assert.AreEqual(0, Vector4I.Zero.X);
            Assert.AreEqual(0, Vector4I.Zero.Y);
            Assert.AreEqual(0, Vector4I.Zero.Z);
            Assert.AreEqual(0, Vector4I.Zero.W);
        }

        [Test]
        public void UnitX()
        {
            Assert.AreEqual(1, Vector4I.UnitX.X);
            Assert.AreEqual(0, Vector4I.UnitX.Y);
            Assert.AreEqual(0, Vector4I.UnitX.Z);
            Assert.AreEqual(0, Vector4I.UnitX.W);
        }

        [Test]
        public void UnitY()
        {
            Assert.AreEqual(0, Vector4I.UnitY.X);
            Assert.AreEqual(1, Vector4I.UnitY.Y);
            Assert.AreEqual(0, Vector4I.UnitY.Z);
            Assert.AreEqual(0, Vector4I.UnitY.W);
        }

        [Test]
        public void UnitZ()
        {
            Assert.AreEqual(0, Vector4I.UnitZ.X);
            Assert.AreEqual(0, Vector4I.UnitZ.Y);
            Assert.AreEqual(1, Vector4I.UnitZ.Z);
            Assert.AreEqual(0, Vector4I.UnitZ.W);
        }

        [Test]
        public void UnitW()
        {
            Assert.AreEqual(0, Vector4I.UnitW.X);
            Assert.AreEqual(0, Vector4I.UnitW.Y);
            Assert.AreEqual(0, Vector4I.UnitW.Z);
            Assert.AreEqual(1, Vector4I.UnitW.W);
        }

        [Test]
        public void TestEquals()
        {
            Vector4I a = new Vector4I(1, 2, 3, 4);
            Vector4I b = new Vector4I(4, 5, 6, 7);
            Vector4I c = new Vector4I(1, 2, 3, 4);

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
            Vector4I a = new Vector4I(1, 2, 3, 4);
            Vector4I b = new Vector4I(4, 5, 6, 7);
            Vector4I c = new Vector4I(1, 2, 3, 4);

            Assert.AreEqual(a.GetHashCode(), c.GetHashCode());
            Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());
        }

        [Test]
        public void Dot()
        {
            Vector4I a = new Vector4I(1, 2, 3, 4);
            Vector4I b = new Vector4I(4, 5, 6, 7);

            double dot = a.Dot(b);
            Assert.AreEqual(1 * 4 + 2 * 5 + 3 * 6 + 4 * 7, dot, 1e-14);
        }

        [Test]
        public void Negate()
        {
            Vector4I a = new Vector4I(1, 2, 3, 4);
            Vector4I negatedA1 = a.Negate();
            Assert.AreEqual(-1, negatedA1.X, 1e-14);
            Assert.AreEqual(-2, negatedA1.Y, 1e-14);
            Assert.AreEqual(-3, negatedA1.Z, 1e-14);
            Assert.AreEqual(-4, negatedA1.W, 1e-14);
            Vector4I negatedA2 = -a;
            Assert.AreEqual(-1, negatedA2.X, 1e-14);
            Assert.AreEqual(-2, negatedA2.Y, 1e-14);
            Assert.AreEqual(-3, negatedA2.Z, 1e-14);
            Assert.AreEqual(-4, negatedA2.W, 1e-14);

            Vector4I b = new Vector4I(-1, -2, -3, -4);
            Vector4I negatedB1 = b.Negate();
            Assert.AreEqual(1, negatedB1.X, 1e-14);
            Assert.AreEqual(2, negatedB1.Y, 1e-14);
            Assert.AreEqual(3, negatedB1.Z, 1e-14);
            Assert.AreEqual(4, negatedB1.W, 1e-14);
            Vector4I negatedB2 = -b;
            Assert.AreEqual(1, negatedB2.X, 1e-14);
            Assert.AreEqual(2, negatedB2.Y, 1e-14);
            Assert.AreEqual(3, negatedB2.Z, 1e-14);
            Assert.AreEqual(4, negatedB2.W, 1e-14);
        }

        [Test]
        public void ToVector4D()
        {
            Vector4I a = new Vector4I(1, 2, 3, 4);
            Vector4D sA = a.ToVector4D();
            Assert.AreEqual(1.0, sA.X, 1e-7);
            Assert.AreEqual(2.0, sA.Y, 1e-7);
            Assert.AreEqual(3.0, sA.Z, 1e-7);
            Assert.AreEqual(4.0, sA.W, 1e-7);
        }

        [Test]
        public void ToVector4S()
        {
            Vector4I a = new Vector4I(1, 2, 3, 4);
            Vector4S sA = a.ToVector4S();
            Assert.AreEqual(1.0f, sA.X, 1e-7);
            Assert.AreEqual(2.0f, sA.Y, 1e-7);
            Assert.AreEqual(3.0f, sA.Z, 1e-7);
            Assert.AreEqual(4.0f, sA.W, 1e-7);
        }

        [Test]
        public void ToVector4H()
        {
            Vector4I a = new Vector4I(1, 2, 3, 4);
            Vector4H sA = a.ToVector4H();
            Assert.AreEqual((Half)1, sA.X, 1e-7);
            Assert.AreEqual((Half)2, sA.Y, 1e-7);
            Assert.AreEqual((Half)3, sA.Z, 1e-7);
            Assert.AreEqual((Half)4, sA.W, 1e-7);
        }

        [Test]
        public void ToVector4B()
        {
            Vector4I a = new Vector4I(1, 0, 0, 1);
            Vector4B sA = a.ToVector4B();
            Assert.IsTrue(sA.X);
            Assert.IsFalse(sA.Y);
            Assert.IsFalse(sA.Z);
            Assert.IsTrue(sA.W);
        }

#if !CSToJava
        [Test]
        public void TestToString()
        {
            CultureInfo originalCulture = Thread.CurrentThread.CurrentCulture;
            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("tr-TR");
                Vector4I a = new Vector4I(1, 2, 3, 4);
                Assert.AreEqual("(1, 2, 3, 4)", a.ToString());
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = originalCulture;
            }
        }
#endif

        [Test]
        public void XY()
        {
            Vector4I a = new Vector4I(1, 2, 3, 4);
            Vector2I xy = a.XY;
            Assert.AreEqual(1, xy.X);
            Assert.AreEqual(2, xy.Y);
        }

        [Test]
        public void XYZ()
        {
            Vector4I a = new Vector4I(1, 2, 3, 4);
            Vector3I xyz = a.XYZ;
            Assert.AreEqual(1, xyz.X);
            Assert.AreEqual(2, xyz.Y);
            Assert.AreEqual(3, xyz.Z);
        }
    }
}
