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
    public class Matrix34Tests
    {
        [Test]
        public void Construct0()
        {
            Matrix34<double> m = new Matrix34<double>();

            for (int i = 0; i < m.NumberOfComponents; ++i)
            {
                Assert.AreEqual(0.0, m.ReadOnlyColumnMajorValues[0], 1e-14);
            }
        }

        [Test]
        public void Construct1()
        {
            Matrix34<double> m = new Matrix34<double>(1.0);

            for (int i = 0; i < m.NumberOfComponents; ++i)
            {
                Assert.AreEqual(1.0, m.ReadOnlyColumnMajorValues[0], 1e-14);
            }
        }

        [Test]
        public void Construct2()
        {
            Matrix34<double> m = new Matrix34<double>(
                1.0, 2.0, 3.0,
                4.0, 5.0, 6.0,
                7.0, 8.0, 9.0, 
                10.0, 11.0, 12.0);

            Assert.AreEqual(1.0, m.Column0Row0);
            Assert.AreEqual(2.0, m.Column1Row0);
            Assert.AreEqual(3.0, m.Column2Row0);
            Assert.AreEqual(4.0, m.Column0Row1);
            Assert.AreEqual(5.0, m.Column1Row1);
            Assert.AreEqual(6.0, m.Column2Row1);
            Assert.AreEqual(7.0, m.Column0Row2);
            Assert.AreEqual(8.0, m.Column1Row2);
            Assert.AreEqual(9.0, m.Column2Row2);
            Assert.AreEqual(10.0, m.Column0Row3);
            Assert.AreEqual(11.0, m.Column1Row3);
            Assert.AreEqual(12.0, m.Column2Row3);

            Assert.AreEqual(1.0, m.ReadOnlyColumnMajorValues[0]);
            Assert.AreEqual(4.0, m.ReadOnlyColumnMajorValues[1]);
            Assert.AreEqual(7.0, m.ReadOnlyColumnMajorValues[2]);
            Assert.AreEqual(10.0, m.ReadOnlyColumnMajorValues[3]);
            Assert.AreEqual(2.0, m.ReadOnlyColumnMajorValues[4]);
            Assert.AreEqual(5.0, m.ReadOnlyColumnMajorValues[5]);
            Assert.AreEqual(8.0, m.ReadOnlyColumnMajorValues[6]);
            Assert.AreEqual(11.0, m.ReadOnlyColumnMajorValues[7]);
            Assert.AreEqual(3.0, m.ReadOnlyColumnMajorValues[8]);
            Assert.AreEqual(6.0, m.ReadOnlyColumnMajorValues[9]);
            Assert.AreEqual(9.0, m.ReadOnlyColumnMajorValues[10]);
            Assert.AreEqual(12.0, m.ReadOnlyColumnMajorValues[11]);
        }

        [Test]
        public void DoubleToFloat()
        {
            Matrix34<double> m = new Matrix34<double>(
                1.0, 2.0, 3.0,
                4.0, 5.0, 6.0,
                7.0, 8.0, 9.0,
                10.0, 11.0, 12.0);

            Matrix34<float> mf = Matrix34<double>.DoubleToFloat(m);

            Assert.AreEqual(1.0f, mf.Column0Row0, 1e-7);
            Assert.AreEqual(2.0f, mf.Column1Row0, 1e-7);
            Assert.AreEqual(3.0f, mf.Column2Row0, 1e-7);
            Assert.AreEqual(4.0f, mf.Column0Row1, 1e-7);
            Assert.AreEqual(5.0f, mf.Column1Row1, 1e-7);
            Assert.AreEqual(6.0f, mf.Column2Row1, 1e-7);
            Assert.AreEqual(7.0f, mf.Column0Row2, 1e-7);
            Assert.AreEqual(8.0f, mf.Column1Row2, 1e-7);
            Assert.AreEqual(9.0f, mf.Column2Row2, 1e-7);
            Assert.AreEqual(10.0f, mf.Column0Row3, 1e-7);
            Assert.AreEqual(11.0f, mf.Column1Row3, 1e-7);
            Assert.AreEqual(12.0f, mf.Column2Row3, 1e-7);
        }

        [Test]
        public void Equals()
        {
            Matrix34<double> a = new Matrix34<double>(
                1.0, 2.0, 3.0,
                4.0, 5.0, 6.0,
                7.0, 8.0, 9.0,
                10.0, 11.0, 12.0);
            Matrix34<double> b = new Matrix34<double>(0.0);
            Matrix34<double> c = new Matrix34<double>(
                1.0, 2.0, 3.0,
                4.0, 5.0, 6.0,
                7.0, 8.0, 9.0,
                10.0, 11.0, 12.0);

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
            Matrix34<double> a = new Matrix34<double>(
                1.0, 2.0, 3.0,
                4.0, 5.0, 6.0,
                7.0, 8.0, 9.0,
                10.0, 11.0, 12.0);
            Matrix34<double> b = new Matrix34<double>(0.0);
            Matrix34<double> c = new Matrix34<double>(
                1.0, 2.0, 3.0,
                4.0, 5.0, 6.0,
                7.0, 8.0, 9.0,
                10.0, 11.0, 12.0);

            Assert.AreEqual(a.GetHashCode(), c.GetHashCode());
            Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());
        }
    }
}
