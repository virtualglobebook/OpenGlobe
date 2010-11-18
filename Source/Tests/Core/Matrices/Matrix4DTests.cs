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
    public class Matrix4DTests
    {
        [Test]
        public void Construct0()
        {
            Matrix4D m = new Matrix4D();

            for (int i = 0; i < m.NumberOfComponents; ++i)
            {
                Assert.AreEqual(0.0, m.ReadOnlyColumnMajorValues[0], 1e-14);
            }
        }

        [Test]
        public void Construct1()
        {
            Matrix4D m = new Matrix4D(1.0);

            for (int i = 0; i < m.NumberOfComponents; ++i)
            {
                Assert.AreEqual(1.0, m.ReadOnlyColumnMajorValues[0], 1e-14);
            }
        }

        [Test]
        public void Construct2()
        {
            Matrix4D m = new Matrix4D(
                1.0, 2.0, 3.0, 4.0,
                5.0, 6.0, 7.0, 8.0,
                9.0, 10.0, 11.0, 12.0,
                13.0, 14.0, 15.0, 16.0);

            Assert.AreEqual(1.0, m.Column0Row0);
            Assert.AreEqual(2.0, m.Column1Row0);
            Assert.AreEqual(3.0, m.Column2Row0);
            Assert.AreEqual(4.0, m.Column3Row0);
            Assert.AreEqual(5.0, m.Column0Row1);
            Assert.AreEqual(6.0, m.Column1Row1);
            Assert.AreEqual(7.0, m.Column2Row1);
            Assert.AreEqual(8.0, m.Column3Row1);
            Assert.AreEqual(9.0, m.Column0Row2);
            Assert.AreEqual(10.0, m.Column1Row2);
            Assert.AreEqual(11.0, m.Column2Row2);
            Assert.AreEqual(12.0, m.Column3Row2);
            Assert.AreEqual(13.0, m.Column0Row3);
            Assert.AreEqual(14.0, m.Column1Row3);
            Assert.AreEqual(15.0, m.Column2Row3);
            Assert.AreEqual(16.0, m.Column3Row3);

            Assert.AreEqual(1.0, m.ReadOnlyColumnMajorValues[0]);
            Assert.AreEqual(5.0, m.ReadOnlyColumnMajorValues[1]);
            Assert.AreEqual(9.0, m.ReadOnlyColumnMajorValues[2]);
            Assert.AreEqual(13.0, m.ReadOnlyColumnMajorValues[3]);
            Assert.AreEqual(2.0, m.ReadOnlyColumnMajorValues[4]);
            Assert.AreEqual(6.0, m.ReadOnlyColumnMajorValues[5]);
            Assert.AreEqual(10.0, m.ReadOnlyColumnMajorValues[6]);
            Assert.AreEqual(14.0, m.ReadOnlyColumnMajorValues[7]);
            Assert.AreEqual(3.0, m.ReadOnlyColumnMajorValues[8]);
            Assert.AreEqual(7.0, m.ReadOnlyColumnMajorValues[9]);
            Assert.AreEqual(11.0, m.ReadOnlyColumnMajorValues[10]);
            Assert.AreEqual(15.0, m.ReadOnlyColumnMajorValues[11]);
            Assert.AreEqual(4.0, m.ReadOnlyColumnMajorValues[12]);
            Assert.AreEqual(8.0, m.ReadOnlyColumnMajorValues[13]);
            Assert.AreEqual(12.0, m.ReadOnlyColumnMajorValues[14]);
            Assert.AreEqual(16.0, m.ReadOnlyColumnMajorValues[15]);
        }

        [Test]
        public void DoubleToFloat()
        {
            Matrix4D m = new Matrix4D(
                1.0, 2.0, 3.0, 4.0,
                5.0, 6.0, 7.0, 8.0,
                9.0, 10.0, 11.0, 12.0,
                13.0, 14.0, 15.0, 16.0);

            Matrix4F mf = m.ToMatrix4F();

            Assert.AreEqual(1.0f, mf.Column0Row0, 1e-7);
            Assert.AreEqual(2.0f, mf.Column1Row0, 1e-7);
            Assert.AreEqual(3.0f, mf.Column2Row0, 1e-7);
            Assert.AreEqual(4.0f, mf.Column3Row0, 1e-7);
            Assert.AreEqual(5.0f, mf.Column0Row1, 1e-7);
            Assert.AreEqual(6.0f, mf.Column1Row1, 1e-7);
            Assert.AreEqual(7.0f, mf.Column2Row1, 1e-7);
            Assert.AreEqual(8.0f, mf.Column3Row1, 1e-7);
            Assert.AreEqual(9.0f, mf.Column0Row2, 1e-7);
            Assert.AreEqual(10.0f, mf.Column1Row2, 1e-7);
            Assert.AreEqual(11.0f, mf.Column2Row2, 1e-7);
            Assert.AreEqual(12.0f, mf.Column3Row2, 1e-7);
            Assert.AreEqual(13.0f, mf.Column0Row3, 1e-7);
            Assert.AreEqual(14.0f, mf.Column1Row3, 1e-7);
            Assert.AreEqual(15.0f, mf.Column2Row3, 1e-7);
            Assert.AreEqual(16.0f, mf.Column3Row3, 1e-7);
        }

        [Test]
        public void Identity()
        {
            Matrix4D m = Matrix4D.Identity;

            Assert.AreEqual(1.0, m.Column0Row0);
            Assert.AreEqual(0.0, m.Column1Row0);
            Assert.AreEqual(0.0, m.Column2Row0);
            Assert.AreEqual(0.0, m.Column3Row0);
            Assert.AreEqual(0.0, m.Column0Row1);
            Assert.AreEqual(1.0, m.Column1Row1);
            Assert.AreEqual(0.0, m.Column2Row1);
            Assert.AreEqual(0.0, m.Column3Row1);
            Assert.AreEqual(0.0, m.Column0Row2);
            Assert.AreEqual(0.0, m.Column1Row2);
            Assert.AreEqual(1.0, m.Column2Row2);
            Assert.AreEqual(0.0, m.Column3Row2);
            Assert.AreEqual(0.0, m.Column0Row3);
            Assert.AreEqual(0.0, m.Column1Row3);
            Assert.AreEqual(0.0, m.Column2Row3);
            Assert.AreEqual(1.0, m.Column3Row3);
        }

        [Test]
        public void Transpose()
        {
            Matrix4D m = new Matrix4D(
                1.0, 2.0, 3.0, 4.0,
                5.0, 6.0, 7.0, 8.0,
                9.0, 10.0, 11.0, 12.0,
                13.0, 14.0, 15.0, 16.0).Transpose();

            Assert.AreEqual(1.0, m.Column0Row0);
            Assert.AreEqual(5.0, m.Column1Row0);
            Assert.AreEqual(9.0, m.Column2Row0);
            Assert.AreEqual(13.0, m.Column3Row0);
            Assert.AreEqual(2.0, m.Column0Row1);
            Assert.AreEqual(6.0, m.Column1Row1);
            Assert.AreEqual(10.0, m.Column2Row1);
            Assert.AreEqual(14.0, m.Column3Row1);
            Assert.AreEqual(3.0, m.Column0Row2);
            Assert.AreEqual(7.0, m.Column1Row2);
            Assert.AreEqual(11.0, m.Column2Row2);
            Assert.AreEqual(15.0, m.Column3Row2);
            Assert.AreEqual(4.0, m.Column0Row3);
            Assert.AreEqual(8.0, m.Column1Row3);
            Assert.AreEqual(12.0, m.Column2Row3);
            Assert.AreEqual(16.0, m.Column3Row3);
        }

        [Test]
        public void Multiply0()
        {
            Matrix4D zero = new Matrix4D(0.0);
            Matrix4D m = new Matrix4D(
                1.0, 2.0, 3.0, 4.0,
                5.0, 6.0, 7.0, 8.0,
                9.0, 10.0, 11.0, 12.0,
                13.0, 14.0, 15.0, 16.0);
            Assert.AreEqual(zero, zero * m);
        }

        [Test]
        public void Multiply1()
        {
            Matrix4D m = new Matrix4D(
                1.0, 2.0, 3.0, 4.0,
                5.0, 6.0, 7.0, 8.0,
                9.0, 10.0, 11.0, 12.0,
                13.0, 14.0, 15.0, 16.0);
            Assert.AreEqual(m, Matrix4D.Identity * m);
        }

        [Test]
        public void Multiply2()
        {
            Matrix4D left = new Matrix4D(1.0);
            Matrix4D right = new Matrix4D(1.0);
            Matrix4D result = new Matrix4D(
                4.0, 4.0, 4.0, 4.0,
                4.0, 4.0, 4.0, 4.0,
                4.0, 4.0, 4.0, 4.0,
                4.0, 4.0, 4.0, 4.0);
            Assert.AreEqual(result, left * right);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Multiply3()
        {
            Matrix4D m = null * new Matrix4D();
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Multiply4()
        {
            Matrix4D m = new Matrix4D() * null;
        }

        [Test]
        public void MultiplyVector0()
        {
            Matrix4D zero = new Matrix4D(0.0);
            Vector4D v = new Vector4D(1.0, 2.0, 3.0, 4.0);
            Assert.AreEqual(Vector4D.Zero, zero * v);
        }

        [Test]
        public void MultiplyVector1()
        {
            Vector4D v = new Vector4D(1.0, 2.0, 3.0, 4.0);
            Assert.AreEqual(v, Matrix4D.Identity * v);
        }

        [Test]
        public void MultiplyVector2()
        {
            Matrix4D m = new Matrix4D(
                1.0, 0.0, 2.0, 0.0,
                0.0, 1.0, 0.0, 2.0,
                2.0, 0.0, 1.0, 0.0,
                0.0, 2.0, 0.0, 1.0);
            Vector4D v = new Vector4D(1.0, 2.0, 3.0, 4.0);
            Vector4D result = new Vector4D(7.0, 10.0, 5.0, 8.0);
            Assert.AreEqual(result, m * v);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void MultiplyVector3()
        {
            Matrix4D m = null;
            Vector4D result = m * new Vector4D();
        }

        [Test]
        public void Equals()
        {
            Matrix4D a = new Matrix4D(
                1.0, 2.0, 3.0, 4.0,
                5.0, 6.0, 7.0, 8.0,
                9.0, 10.0, 11.0, 12.0,
                13.0, 14.0, 15.0, 16.0);
            Matrix4D b = new Matrix4D(0.0);
            Matrix4D c = new Matrix4D(
                1.0, 2.0, 3.0, 4.0,
                5.0, 6.0, 7.0, 8.0,
                9.0, 10.0, 11.0, 12.0,
                13.0, 14.0, 15.0, 16.0);

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
            Matrix4D a = new Matrix4D(
                1.0, 2.0, 3.0, 4.0,
                5.0, 6.0, 7.0, 8.0,
                9.0, 10.0, 11.0, 12.0,
                13.0, 14.0, 15.0, 16.0);
            Matrix4D b = new Matrix4D(0.0);
            Matrix4D c = new Matrix4D(
                1.0, 2.0, 3.0, 4.0,
                5.0, 6.0, 7.0, 8.0,
                9.0, 10.0, 11.0, 12.0,
                13.0, 14.0, 15.0, 16.0);

            Assert.AreEqual(a.GetHashCode(), c.GetHashCode());
            Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());
        }
    }
}
