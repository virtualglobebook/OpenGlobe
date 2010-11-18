#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using NUnit.Framework;

namespace OpenGlobe.Core
{
    [TestFixture]
    public class Matrix3DTests
    {
        [Test]
        public void Construct0()
        {
            Matrix3D m = new Matrix3D();

            for (int i = 0; i < m.NumberOfComponents; ++i)
            {
                Assert.AreEqual(0.0, m.ReadOnlyColumnMajorValues[0], 1e-14);
            }
        }

        [Test]
        public void Construct1()
        {
            Matrix3D m = new Matrix3D(1.0);

            for (int i = 0; i < m.NumberOfComponents; ++i)
            {
                Assert.AreEqual(1.0, m.ReadOnlyColumnMajorValues[0], 1e-14);
            }
        }

        [Test]
        public void Construct2()
        {
            Matrix3D m = new Matrix3D(
                1.0, 2.0, 3.0,
                4.0, 5.0, 6.0,
                7.0, 8.0, 9.0);

            Assert.AreEqual(1.0, m.Column0Row0);
            Assert.AreEqual(2.0, m.Column1Row0);
            Assert.AreEqual(3.0, m.Column2Row0);
            Assert.AreEqual(4.0, m.Column0Row1);
            Assert.AreEqual(5.0, m.Column1Row1);
            Assert.AreEqual(6.0, m.Column2Row1);
            Assert.AreEqual(7.0, m.Column0Row2);
            Assert.AreEqual(8.0, m.Column1Row2);
            Assert.AreEqual(9.0, m.Column2Row2);

            Assert.AreEqual(1.0, m.ReadOnlyColumnMajorValues[0]);
            Assert.AreEqual(4.0, m.ReadOnlyColumnMajorValues[1]);
            Assert.AreEqual(7.0, m.ReadOnlyColumnMajorValues[2]);
            Assert.AreEqual(2.0, m.ReadOnlyColumnMajorValues[3]);
            Assert.AreEqual(5.0, m.ReadOnlyColumnMajorValues[4]);
            Assert.AreEqual(8.0, m.ReadOnlyColumnMajorValues[5]);
            Assert.AreEqual(3.0, m.ReadOnlyColumnMajorValues[6]);
            Assert.AreEqual(6.0, m.ReadOnlyColumnMajorValues[7]);
            Assert.AreEqual(9.0, m.ReadOnlyColumnMajorValues[8]);
        }

        [Test]
        public void DoubleToFloat()
        {
            Matrix3D m = new Matrix3D(
                1.0, 2.0, 3.0,
                4.0, 5.0, 6.0,
                7.0, 8.0, 9.0);

            Matrix3S mf = m.ToMatrix4F();

            Assert.AreEqual(1.0, mf.ReadOnlyColumnMajorValues[0], 1e-7);
            Assert.AreEqual(4.0, mf.ReadOnlyColumnMajorValues[1], 1e-7);
            Assert.AreEqual(7.0, mf.ReadOnlyColumnMajorValues[2], 1e-7);
            Assert.AreEqual(2.0, mf.ReadOnlyColumnMajorValues[3], 1e-7);
            Assert.AreEqual(5.0, mf.ReadOnlyColumnMajorValues[4], 1e-7);
            Assert.AreEqual(8.0, mf.ReadOnlyColumnMajorValues[5], 1e-7);
            Assert.AreEqual(3.0, mf.ReadOnlyColumnMajorValues[6], 1e-7);
            Assert.AreEqual(6.0, mf.ReadOnlyColumnMajorValues[7], 1e-7);
            Assert.AreEqual(9.0, mf.ReadOnlyColumnMajorValues[8], 1e-7);
        }

        [Test]
        public void Identity()
        {
            Matrix3D m = Matrix3D.Identity;

            Assert.AreEqual(1.0, m.Column0Row0);
            Assert.AreEqual(0.0, m.Column1Row0);
            Assert.AreEqual(0.0, m.Column2Row0);
            Assert.AreEqual(0.0, m.Column0Row1);
            Assert.AreEqual(1.0, m.Column1Row1);
            Assert.AreEqual(0.0, m.Column2Row1);
            Assert.AreEqual(0.0, m.Column0Row2);
            Assert.AreEqual(0.0, m.Column1Row2);
            Assert.AreEqual(1.0, m.Column2Row2);
        }

        [Test]
        public void Transpose()
        {
            Matrix3D m = new Matrix3D(
                1.0, 2.0, 3.0,
                4.0, 5.0, 6.0,
                7.0, 8.0, 9.0).Transpose();

            Assert.AreEqual(1.0, m.Column0Row0);
            Assert.AreEqual(4.0, m.Column1Row0);
            Assert.AreEqual(7.0, m.Column2Row0);
            Assert.AreEqual(2.0, m.Column0Row1);
            Assert.AreEqual(5.0, m.Column1Row1);
            Assert.AreEqual(8.0, m.Column2Row1);
            Assert.AreEqual(3.0, m.Column0Row2);
            Assert.AreEqual(6.0, m.Column1Row2);
            Assert.AreEqual(9.0, m.Column2Row2);
        }

        [Test]
        public void MultiplyVector0()
        {
            Matrix3D zero = new Matrix3D(0.0);
            Vector3D v = new Vector3D(1.0, 2.0, 3.0);
            Assert.AreEqual(Vector3D.Zero, zero * v);
        }

        [Test]
        public void MultiplyVector1()
        {
            Vector3D v = new Vector3D(1.0, 2.0, 3.0);
            Assert.AreEqual(v, Matrix3D.Identity * v);
        }

        [Test]
        public void MultiplyVector2()
        {
            Matrix3D m = new Matrix3D(
                1.0, 0.0, 2.0,
                0.0, 1.0, 3.0,
                2.0, 0.0, 1.0);
            Vector3D v = new Vector3D(1.0, 2.0, 3.0);
            Vector3D result = new Vector3D(7.0, 11.0, 5.0);
            Assert.AreEqual(result, m * v);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void MultiplyVector3()
        {
            Matrix3D m = null;
            Vector3D result = m * new Vector3D();
        }

        [Test]
        public void Equals()
        {
            Matrix3D a = new Matrix3D(
                1.0, 2.0, 3.0,
                4.0, 5.0, 6.0,
                7.0, 8.0, 9.0).Transpose();
            Matrix3D b = new Matrix3D(0.0);
            Matrix3D c = new Matrix3D(
                1.0, 2.0, 3.0,
                4.0, 5.0, 6.0,
                7.0, 8.0, 9.0).Transpose();

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
            Matrix3D a = new Matrix3D(
                1.0, 2.0, 3.0,
                4.0, 5.0, 6.0,
                7.0, 8.0, 9.0).Transpose();
            Matrix3D b = new Matrix3D(0.0);
            Matrix3D c = new Matrix3D(
                1.0, 2.0, 3.0,
                4.0, 5.0, 6.0,
                7.0, 8.0, 9.0).Transpose();

            Assert.AreEqual(a.GetHashCode(), c.GetHashCode());
            Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());
        }
    }
}
