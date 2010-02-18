#region License
//
// (C) Copyright 2010 Patrick Cozzi, Deron Ohlarik, and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using NUnit.Framework;

namespace MiniGlobe.Core.Coordinates
{
    [TestFixture]
    public class Vector3dTests
    {
        [Test]
        public void Construct()
        {
            Vector3d v = new Vector3d(1.0, 2.0, 3.0);
            Assert.AreEqual(1.0, v.X);
            Assert.AreEqual(2.0, v.Y);
            Assert.AreEqual(3.0, v.Z);
        }

        [Test]
        public void Magnitude()
        {
            Vector3d v = new Vector3d(3.0, 4.0, 0.0);
            Assert.AreEqual(25.0, v.MagnitudeSquared, 1e-14);
            Assert.AreEqual(5.0, v.Magnitude, 1e-14);

            v = new Vector3d(3.0, 0.0, 4.0);
            Assert.AreEqual(25.0, v.MagnitudeSquared, 1e-14);
            Assert.AreEqual(5.0, v.Magnitude, 1e-14);

            v = new Vector3d(0.0, 3.0, 4.0);
            Assert.AreEqual(25.0, v.MagnitudeSquared, 1e-14);
            Assert.AreEqual(5.0, v.Magnitude, 1e-14);
        }

        [Test]
        public void Normalize()
        {
            Vector3d v, n1, n2;
            double magnitude;

            v = new Vector3d(3.0, 4.0, 0.0);
            n1 = v.Normalize();
            n2 = v.Normalize(out magnitude);
            Assert.AreEqual(1.0, n1.Magnitude, 1e-14);
            Assert.AreEqual(1.0, n2.Magnitude, 1e-14);
            Assert.AreEqual(5.0, magnitude, 1e-14);

            v = new Vector3d(3.0, 0.0, 4.0);
            n1 = v.Normalize();
            n2 = v.Normalize(out magnitude);
            Assert.AreEqual(1.0, n1.Magnitude, 1e-14);
            Assert.AreEqual(1.0, n2.Magnitude, 1e-14);
            Assert.AreEqual(5.0, magnitude, 1e-14);

            v = new Vector3d(0.0, 3.0, 4.0);
            n1 = v.Normalize();
            n2 = v.Normalize(out magnitude);
            Assert.AreEqual(1.0, n1.Magnitude, 1e-14);
            Assert.AreEqual(1.0, n2.Magnitude, 1e-14);
            Assert.AreEqual(5.0, magnitude, 1e-14);
        }

        [Test]
        public void NormalizeZeroVector()
        {
            Vector3d v = new Vector3d(0.0, 0.0, 0.0);
            
            Vector3d n1 = v.Normalize();
            Assert.IsNaN(n1.X);
            Assert.IsNaN(n1.Y);
            Assert.IsNaN(n1.Z);
            Assert.IsTrue(n1.IsUndefined);

            double magnitude;
            Vector3d n2 = v.Normalize(out magnitude);
            Assert.IsNaN(n2.X);
            Assert.IsNaN(n2.Y);
            Assert.IsNaN(n2.Z);
            Assert.IsTrue(n2.IsUndefined);
            Assert.AreEqual(0.0, magnitude);
        }

        [Test]
        public void Add()
        {
            Vector3d v1 = new Vector3d(1.0, 2.0, 3.0);
            Vector3d v2 = new Vector3d(4.0, 5.0, 6.0);
            
            Vector3d v3 = v1 + v2;
            Assert.AreEqual(5.0, v3.X, 1e-14);
            Assert.AreEqual(7.0, v3.Y, 1e-14);
            Assert.AreEqual(9.0, v3.Z, 1e-14);

            Vector3d v4 = v1.Add(v2);
            Assert.AreEqual(5.0, v4.X, 1e-14);
            Assert.AreEqual(7.0, v4.Y, 1e-14);
            Assert.AreEqual(9.0, v4.Z, 1e-14);
        }

        [Test]
        public void Subtract()
        {
            Vector3d v1 = new Vector3d(1.0, 2.0, 3.0);
            Vector3d v2 = new Vector3d(4.0, 5.0, 6.0);

            Vector3d v3 = v1 - v2;
            Assert.AreEqual(-3.0, v3.X, 1e-14);
            Assert.AreEqual(-3.0, v3.Y, 1e-14);
            Assert.AreEqual(-3.0, v3.Z, 1e-14);

            Vector3d v4 = v1.Subtract(v2);
            Assert.AreEqual(-3.0, v4.X, 1e-14);
            Assert.AreEqual(-3.0, v4.Y, 1e-14);
            Assert.AreEqual(-3.0, v4.Z, 1e-14);
        }

        [Test]
        public void Multiply()
        {
            Vector3d v1 = new Vector3d(1.0, 2.0, 3.0);

            Vector3d v2 = v1 * 5.0;
            Assert.AreEqual(5.0, v2.X, 1e-14);
            Assert.AreEqual(10.0, v2.Y, 1e-14);
            Assert.AreEqual(15.0, v2.Z, 1e-14);

            Vector3d v3 = 5.0 * v1;
            Assert.AreEqual(5.0, v3.X, 1e-14);
            Assert.AreEqual(10.0, v3.Y, 1e-14);
            Assert.AreEqual(15.0, v3.Z, 1e-14);

            Vector3d v4 = v1.Multiply(5.0);
            Assert.AreEqual(5.0, v4.X, 1e-14);
            Assert.AreEqual(10.0, v4.Y, 1e-14);
            Assert.AreEqual(15.0, v4.Z, 1e-14);
        }

        [Test]
        public void Divide()
        {
            Vector3d v1 = new Vector3d(10.0, 20.0, 30.0);

            Vector3d v2 = v1 / 5.0;
            Assert.AreEqual(2.0, v2.X, 1e-14);
            Assert.AreEqual(4.0, v2.Y, 1e-14);
            Assert.AreEqual(6.0, v2.Z, 1e-14);

            Vector3d v3 = v1.Divide(5.0);
            Assert.AreEqual(2.0, v3.X, 1e-14);
            Assert.AreEqual(4.0, v3.Y, 1e-14);
            Assert.AreEqual(6.0, v3.Z, 1e-14);
        }

        [Test]
        public void Zero()
        {
            Assert.AreEqual(0.0, Vector3d.Zero.X);
            Assert.AreEqual(0.0, Vector3d.Zero.Y);
            Assert.AreEqual(0.0, Vector3d.Zero.Z);
        }

        [Test]
        public void UnitX()
        {
            Assert.AreEqual(1.0, Vector3d.UnitX.X);
            Assert.AreEqual(0.0, Vector3d.UnitX.Y);
            Assert.AreEqual(0.0, Vector3d.UnitX.Z);
        }

        [Test]
        public void UnitY()
        {
            Assert.AreEqual(0.0, Vector3d.UnitY.X);
            Assert.AreEqual(1.0, Vector3d.UnitY.Y);
            Assert.AreEqual(0.0, Vector3d.UnitY.Z);
        }

        [Test]
        public void UnitZ()
        {
            Assert.AreEqual(0.0, Vector3d.UnitZ.X);
            Assert.AreEqual(0.0, Vector3d.UnitZ.Y);
            Assert.AreEqual(1.0, Vector3d.UnitZ.Z);
        }

        [Test]
        public void Undefined()
        {
            Assert.IsNaN(Vector3d.Undefined.X);
            Assert.IsNaN(Vector3d.Undefined.Y);
            Assert.IsNaN(Vector3d.Undefined.Z);
        }

        [Test]
        public void TestUndefined()
        {
            Assert.IsTrue(Vector3d.Undefined.IsUndefined);
            Assert.IsFalse(Vector3d.UnitX.IsUndefined);
            Assert.IsFalse(Vector3d.UnitY.IsUndefined);
            Assert.IsFalse(Vector3d.UnitZ.IsUndefined);
        }
    }
}
