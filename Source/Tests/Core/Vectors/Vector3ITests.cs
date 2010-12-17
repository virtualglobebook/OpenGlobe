#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using NUnit.Framework;
using System.Globalization;
using System.Threading;

namespace OpenGlobe.Core
{
    [TestFixture]
    public class Vector3ITests
    {
        [Test]
        public void Construct0()
        {
            Vector3I v = new Vector3I(1, 2, 3);
            Assert.AreEqual(1, v.X);
            Assert.AreEqual(2, v.Y);
            Assert.AreEqual(3, v.Z);
        }

        [Test]
        public void Construct1()
        {
            Vector3I v = new Vector3I(new Vector2I(1, 2), 3);
            Assert.AreEqual(1, v.X);
            Assert.AreEqual(2, v.Y);
            Assert.AreEqual(3, v.Z);
        }

        [Test]
        public void Magnitude()
        {
            Vector3I v = new Vector3I(3, 4, 0);
            Assert.AreEqual(25, v.MagnitudeSquared);
            Assert.AreEqual(5.0, v.Magnitude, 1e-14);

            v = new Vector3I(3, 0, 4);
            Assert.AreEqual(25, v.MagnitudeSquared);
            Assert.AreEqual(5.0, v.Magnitude, 1e-14);

            v = new Vector3I(0, 3, 4);
            Assert.AreEqual(25, v.MagnitudeSquared);
            Assert.AreEqual(5.0, v.Magnitude, 1e-14);
        }

        [Test]
        public void Add()
        {
            Vector3I v1 = new Vector3I(1, 2, 3);
            Vector3I v2 = new Vector3I(4, 5, 6);

            Vector3I v3 = v1 + v2;
            Assert.AreEqual(5, v3.X);
            Assert.AreEqual(7, v3.Y);
            Assert.AreEqual(9, v3.Z);

            Vector3I v4 = v1.Add(v2);
            Assert.AreEqual(5, v4.X);
            Assert.AreEqual(7, v4.Y);
            Assert.AreEqual(9, v4.Z);
        }

        [Test]
        public void Subtract()
        {
            Vector3I v1 = new Vector3I(1, 2, 3);
            Vector3I v2 = new Vector3I(4, 5, 6);

            Vector3I v3 = v1 - v2;
            Assert.AreEqual(-3.0, v3.X);
            Assert.AreEqual(-3.0, v3.Y);
            Assert.AreEqual(-3.0, v3.Z);

            Vector3I v4 = v1.Subtract(v2);
            Assert.AreEqual(-3.0, v4.X);
            Assert.AreEqual(-3.0, v4.Y);
            Assert.AreEqual(-3.0, v4.Z);
        }

        [Test]
        public void Multiply()
        {
            Vector3I v1 = new Vector3I(1, 2, 3);

            Vector3I v2 = v1 * 5;
            Assert.AreEqual(5, v2.X);
            Assert.AreEqual(10, v2.Y);
            Assert.AreEqual(15, v2.Z);

            Vector3I v3 = 5 * v1;
            Assert.AreEqual(5, v3.X);
            Assert.AreEqual(10, v3.Y);
            Assert.AreEqual(15, v3.Z);

            Vector3I v4 = v1.Multiply(5);
            Assert.AreEqual(5, v4.X);
            Assert.AreEqual(10, v4.Y);
            Assert.AreEqual(15, v4.Z);
        }

        [Test]
        public void MultiplyComponents()
        {
            Vector3I v1 = new Vector3I(1, 2, 3);
            Vector3I v2 = new Vector3I(4, 8, 12);

            Assert.AreEqual(new Vector3I(4, 16, 36), v1.MultiplyComponents(v2));
        }

        [Test]
        public void Divide()
        {
            Vector3I v1 = new Vector3I(10, 20, 30);

            Vector3I v2 = v1 / 5;
            Assert.AreEqual(2, v2.X);
            Assert.AreEqual(4, v2.Y);
            Assert.AreEqual(6, v2.Z);

            Vector3I v3 = v1.Divide(5);
            Assert.AreEqual(2, v3.X);
            Assert.AreEqual(4, v3.Y);
            Assert.AreEqual(6, v3.Z);
        }

        [Test]
        public void MostOrthogonalAxis()
        {
            Vector3I v1 = new Vector3I(1, 2, 3);
            Assert.AreEqual(Vector3I.UnitX, v1.MostOrthogonalAxis);

            Vector3I v2 = new Vector3I(-3, -1, -2);
            Assert.AreEqual(Vector3I.UnitY, v2.MostOrthogonalAxis);

            Vector3I v3 = new Vector3I(3, 2, 1);
            Assert.AreEqual(Vector3I.UnitZ, v3.MostOrthogonalAxis);
        }

        [Test]
        public void Zero()
        {
            Assert.AreEqual(0, Vector3I.Zero.X);
            Assert.AreEqual(0, Vector3I.Zero.Y);
            Assert.AreEqual(0, Vector3I.Zero.Z);
        }

        [Test]
        public void UnitX()
        {
            Assert.AreEqual(1, Vector3I.UnitX.X);
            Assert.AreEqual(0, Vector3I.UnitX.Y);
            Assert.AreEqual(0, Vector3I.UnitX.Z);
        }

        [Test]
        public void UnitY()
        {
            Assert.AreEqual(0, Vector3I.UnitY.X);
            Assert.AreEqual(1, Vector3I.UnitY.Y);
            Assert.AreEqual(0, Vector3I.UnitY.Z);
        }

        [Test]
        public void UnitZ()
        {
            Assert.AreEqual(0, Vector3I.UnitZ.X);
            Assert.AreEqual(0, Vector3I.UnitZ.Y);
            Assert.AreEqual(1, Vector3I.UnitZ.Z);
        }

        [Test]
        public void TestEquals()
        {
            Vector3I a = new Vector3I(1, 2, 3);
            Vector3I b = new Vector3I(4, 5, 6);
            Vector3I c = new Vector3I(1, 2, 3);

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
        public void Comparison()
        {
            Vector3I a = new Vector3I(1, 2, 3);
            Vector3I b = new Vector3I(2, 3, 4);

            Assert.IsTrue(a < b);
            Assert.IsTrue(a <= b);
            Assert.IsFalse(a > b);
            Assert.IsFalse(a >= b);

            Vector3I c = new Vector3I(1, 2, 3);
            Assert.IsTrue(a <= c);
            Assert.IsTrue(c >= a);
        }

        [Test]
        public void TestGetHashCode()
        {
            Vector3I a = new Vector3I(1, 2, 3);
            Vector3I b = new Vector3I(4, 5, 6);
            Vector3I c = new Vector3I(1, 2, 3);

            Assert.AreEqual(a.GetHashCode(), c.GetHashCode());
            Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());
        }

        [Test]
        public void Dot()
        {
            Vector3I a = new Vector3I(1, 2, 3);
            Vector3I b = new Vector3I(4, 5, 6);

            double dot = a.Dot(b);
            Assert.AreEqual(1 * 4 + 2 * 5 + 3 * 6, dot, 1e-14);
        }

        [Test]
        public void Negate()
        {
            Vector3I a = new Vector3I(1, 2, 3);
            Vector3I negatedA1 = a.Negate();
            Assert.AreEqual(-1, negatedA1.X);
            Assert.AreEqual(-2, negatedA1.Y);
            Assert.AreEqual(-3, negatedA1.Z);
            Vector3I negatedA2 = -a;
            Assert.AreEqual(-1, negatedA2.X);
            Assert.AreEqual(-2, negatedA2.Y);
            Assert.AreEqual(-3, negatedA2.Z);

            Vector3I b = new Vector3I(-1, -2, -3);
            Vector3I negatedB1 = b.Negate();
            Assert.AreEqual(1, negatedB1.X);
            Assert.AreEqual(2, negatedB1.Y);
            Assert.AreEqual(3, negatedB1.Z);
            Vector3I negatedB2 = -b;
            Assert.AreEqual(1, negatedB2.X);
            Assert.AreEqual(2, negatedB2.Y);
            Assert.AreEqual(3, negatedB2.Z);
        }

        [Test]
        public void ToVector3D()
        {
            Vector3I a = new Vector3I(1, 2, 3);
            Vector3D sA = a.ToVector3D();
            Assert.AreEqual(1.0, sA.X);
            Assert.AreEqual(2.0, sA.Y);
            Assert.AreEqual(3.0, sA.Z);
        }

        [Test]
        public void ToVector3F()
        {
            Vector3I a = new Vector3I(1, 2, 3);
            Vector3F sA = a.ToVector3F();
            Assert.AreEqual(1.0f, sA.X);
            Assert.AreEqual(2.0f, sA.Y);
            Assert.AreEqual(3.0f, sA.Z);
        }

        [Test]
        public void ToVector3H()
        {
            Vector3I a = new Vector3I(1, 2, 3);
            Vector3H sA = a.ToVector3H();
            Assert.AreEqual((Half)1, sA.X);
            Assert.AreEqual((Half)2, sA.Y);
            Assert.AreEqual((Half)3, sA.Z);
        }

        [Test]
        public void ToVector3B()
        {
            Vector3I a = new Vector3I(0, 1, 0);
            Vector3B sA = a.ToVector3B();
            Assert.IsFalse(sA.X);
            Assert.IsTrue(sA.Y);
            Assert.IsFalse(sA.Z);
        }

        [Test]
        public void Cross()
        {
            Vector3I a = new Vector3I(1, 0, 0);
            Vector3I b = new Vector3I(0, 1, 0);

            Vector3I cross1 = a.Cross(b);
            Assert.AreEqual(0, cross1.X);
            Assert.AreEqual(0, cross1.Y);
            Assert.AreEqual(1, cross1.Z);

            Vector3I cross2 = b.Cross(a);
            Assert.AreEqual(0, cross2.X);
            Assert.AreEqual(0, cross2.Y);
            Assert.AreEqual(-1, cross2.Z);
        }

#if !CSToJava
        [Test]
        public void TestToString()
        {
            CultureInfo originalCulture = Thread.CurrentThread.CurrentCulture;
            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("tr-TR");
                Vector3I a = new Vector3I(1, 2, 3);
                Assert.AreEqual("(1, 2, 3)", a.ToString());
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
            Vector3I a = new Vector3I(1, 2, 3);
            Vector2I xy = a.XY;
            Assert.AreEqual(1, xy.X);
            Assert.AreEqual(2, xy.Y);
        }
    }
}
