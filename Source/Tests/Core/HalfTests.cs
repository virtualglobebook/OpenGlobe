#region License
//
// (C) Copyright 2010 Patrick Cozzi, Deron Ohlarik, and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using NUnit.Framework;

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
    }
}

