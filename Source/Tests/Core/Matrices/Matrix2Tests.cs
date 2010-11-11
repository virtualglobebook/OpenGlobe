#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using NUnit.Framework;

namespace OpenGlobe.Core
{
    [TestFixture]
    public class Matrix2Tests
    {
        [Test]
        public void Construct0()
        {
            Matrix2<double> m = new Matrix2<double>();

            for (int i = 0; i < m.NumberOfComponents; ++i)
            {
                Assert.AreEqual(0.0, m.ReadOnlyColumnMajorValues[0], 1e-14);
            }
        }

        [Test]
        public void Construct1()
        {
            Matrix2<double> m = new Matrix2<double>(1.0);

            for (int i = 0; i < m.NumberOfComponents; ++i)
            {
                Assert.AreEqual(1.0, m.ReadOnlyColumnMajorValues[0], 1e-14);
            }
        }

        [Test]
        public void Construct2()
        {
            Matrix2<double> m = new Matrix2<double>(
                1.0, 2.0,
                3.0, 4.0);

            Assert.AreEqual(1.0, m.Column0Row0);
            Assert.AreEqual(2.0, m.Column1Row0);
            Assert.AreEqual(3.0, m.Column0Row1);
            Assert.AreEqual(4.0, m.Column1Row1);

            Assert.AreEqual(1.0, m.ReadOnlyColumnMajorValues[0]);
            Assert.AreEqual(3.0, m.ReadOnlyColumnMajorValues[1]);
            Assert.AreEqual(2.0, m.ReadOnlyColumnMajorValues[2]);
            Assert.AreEqual(4.0, m.ReadOnlyColumnMajorValues[3]);
        }

        [Test]
        public void DoubleToFloat()
        {
            Matrix2<double> m = new Matrix2<double>(
                1.0, 2.0,
                3.0, 4.0);

            Matrix2<float> mf = Matrix2<double>.DoubleToFloat(m);

            Assert.AreEqual(1.0f, mf.Column0Row0, 1e-7);
            Assert.AreEqual(2.0f, mf.Column1Row0, 1e-7);
            Assert.AreEqual(3.0f, mf.Column0Row1, 1e-7);
            Assert.AreEqual(4.0f, mf.Column1Row1, 1e-7);
        }

        [Test]
        public void Equals()
        {
            Matrix2<double> a = new Matrix2<double>(
                1.0, 2.0,
                3.0, 4.0);
            Matrix2<double> b = new Matrix2<double>(0.0);
            Matrix2<double> c = new Matrix2<double>(
                1.0, 2.0,
                3.0, 4.0);

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
            Matrix2<double> a = new Matrix2<double>(
                1.0, 2.0,
                3.0, 4.0);
            Matrix2<double> b = new Matrix2<double>(0.0);
            Matrix2<double> c = new Matrix2<double>(
                1.0, 2.0,
                3.0, 4.0);

            Assert.AreEqual(a.GetHashCode(), c.GetHashCode());
            Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());
        }
    }
}
