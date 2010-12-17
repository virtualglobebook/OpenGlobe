#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using NUnit.Framework;

namespace OpenGlobe.Core
{
    [TestFixture]
    public class Matrix32Tests
    {
        [Test]
        public void Construct0()
        {
            Matrix32<double> m = new Matrix32<double>();

            for (int i = 0; i < m.NumberOfComponents; ++i)
            {
                Assert.AreEqual(0.0, m.ReadOnlyColumnMajorValues[0], 1e-14);
            }
        }

        [Test]
        public void Construct1()
        {
            Matrix32<double> m = new Matrix32<double>(1.0);

            for (int i = 0; i < m.NumberOfComponents; ++i)
            {
                Assert.AreEqual(1.0, m.ReadOnlyColumnMajorValues[0], 1e-14);
            }
        }

        [Test]
        public void Construct2()
        {
            Matrix32<double> m = new Matrix32<double>(
                1.0, 2.0, 3.0,
                4.0, 5.0, 6.0);

            Assert.AreEqual(1.0, m.Column0Row0);
            Assert.AreEqual(2.0, m.Column1Row0);
            Assert.AreEqual(3.0, m.Column2Row0);
            Assert.AreEqual(4.0, m.Column0Row1);
            Assert.AreEqual(5.0, m.Column1Row1);
            Assert.AreEqual(6.0, m.Column2Row1);

            Assert.AreEqual(1.0, m.ReadOnlyColumnMajorValues[0]);
            Assert.AreEqual(4.0, m.ReadOnlyColumnMajorValues[1]);
            Assert.AreEqual(2.0, m.ReadOnlyColumnMajorValues[2]);
            Assert.AreEqual(5.0, m.ReadOnlyColumnMajorValues[3]);
            Assert.AreEqual(3.0, m.ReadOnlyColumnMajorValues[4]);
            Assert.AreEqual(6.0, m.ReadOnlyColumnMajorValues[5]);
        }

        [Test]
        public void DoubleToFloat()
        {
            Matrix32<double> m = new Matrix32<double>(
                1.0, 2.0, 3.0, 
                4.0, 5.0, 6.0);

            Matrix32<float> mf = Matrix32<double>.DoubleToFloat(m);

            Assert.AreEqual(1.0f, mf.Column0Row0, 1e-7);
            Assert.AreEqual(2.0f, mf.Column1Row0, 1e-7);
            Assert.AreEqual(3.0f, mf.Column2Row0, 1e-7);
            Assert.AreEqual(4.0f, mf.Column0Row1, 1e-7);
            Assert.AreEqual(5.0f, mf.Column1Row1, 1e-7);
            Assert.AreEqual(6.0f, mf.Column2Row1, 1e-7);
        }

        [Test]
        public void Equals()
        {
            Matrix32<double> a = new Matrix32<double>(
                1.0, 2.0, 3.0, 
                4.0, 5.0, 6.0);
            Matrix32<double> b = new Matrix32<double>(0.0);
            Matrix32<double> c = new Matrix32<double>(
                1.0, 2.0, 3.0,
                4.0, 5.0, 6.0);

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
            Matrix32<double> a = new Matrix32<double>(
                1.0, 2.0, 3.0, 
                4.0, 5.0, 6.0);
            Matrix32<double> b = new Matrix32<double>(0.0);
            Matrix32<double> c = new Matrix32<double>(
                1.0, 2.0, 3.0,
                4.0, 5.0, 6.0);

            Assert.AreEqual(a.GetHashCode(), c.GetHashCode());
            Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());
        }
    }
}
