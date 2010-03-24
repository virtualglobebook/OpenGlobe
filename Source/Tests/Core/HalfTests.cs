#region License
//
// (C) Copyright 2010 Patrick Cozzi, Deron Ohlarik, and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using NUnit.Framework;
using System;

namespace MiniGlobe.Core
{
    [TestFixture]
    public class HalfTests
    {
        [Test]
        public void TestNaN()
        {
            Assert.IsTrue(Half.NaN.IsNaN);
            Assert.IsFalse(Half.PositiveInfinity.IsNaN);
            Assert.IsFalse(Half.NegativeInfinity.IsNaN);
            Assert.IsFalse(new Half(1.23).IsNaN);
            Assert.IsTrue(new Half(float.NaN).IsNaN);
            Assert.IsTrue(new Half(double.NaN).IsNaN);
        }

        [Test]
        public void TestPositiveInfinity()
        {
            Assert.IsFalse(Half.NaN.IsPositiveInfinity);
            Assert.IsTrue(Half.PositiveInfinity.IsPositiveInfinity);
            Assert.IsFalse(Half.NegativeInfinity.IsPositiveInfinity);
            Assert.IsFalse(new Half(1.23).IsPositiveInfinity);
            Assert.IsTrue(new Half(float.PositiveInfinity).IsPositiveInfinity);
            Assert.IsTrue(new Half(double.PositiveInfinity).IsPositiveInfinity);
        }

        [Test]
        public void TestNegativeInfinity()
        {
            Assert.IsFalse(Half.NaN.IsNegativeInfinity);
            Assert.IsFalse(Half.PositiveInfinity.IsNegativeInfinity);
            Assert.IsTrue(Half.NegativeInfinity.IsNegativeInfinity);
            Assert.IsFalse(new Half(1.23).IsNegativeInfinity);
            Assert.IsTrue(new Half(float.NegativeInfinity).IsNegativeInfinity);
            Assert.IsTrue(new Half(double.NegativeInfinity).IsNegativeInfinity);
        }

        [Test]
        public void TestEquals()
        {
            Assert.IsTrue(new Half(1.23).Equals(new Half(1.23)));
            Assert.AreEqual(new Half(1.23), new Half(1.23));
            Assert.IsFalse(new Half(1.23).Equals(new Half(1.24)));
            Assert.AreNotEqual(new Half(1.23), new Half(1.24));
            Assert.IsFalse(new Half(1.23).Equals(1.23));
        }

        [Test]
        public void TestDoubleRoundTrip()
        {
            Assert.AreEqual(1.23, new Half(1.23).ToDouble(), 1e-3);
            Assert.AreEqual(0.0, new Half(0.0).ToDouble(), 1e-3);
            Assert.AreEqual(-1.0, new Half(-1.0).ToDouble(), 1e-3);
            Assert.AreEqual(0.0, new Half(1e-30).ToDouble());
            Assert.AreEqual(0.0, new Half(-1e-30).ToDouble());
            Assert.AreEqual(1e-5, new Half(1e-5).ToDouble(), 1e-7);
            Assert.AreEqual(Double.PositiveInfinity, new Half(Double.PositiveInfinity).ToDouble());
            Assert.AreEqual(Double.NegativeInfinity, new Half(Double.NegativeInfinity).ToDouble());
            Assert.AreEqual(255.99999999999997, new Half(255.99999999999997).ToDouble(), 1e-3);
        }

        [Test]
        [ExpectedException(typeof(ArithmeticException))]
        public void TestOverflow()
        {
            new Half(1e30);
        }
    }
}

