#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using NUnit.Framework;

namespace OpenGlobe.Core
{
    [TestFixture]
    public class IntervalTests
    {
        [Test]
        public void Construct()
        {
            Interval interval = new Interval(0, 1);
            Assert.AreEqual(0, interval.Minimum);
            Assert.AreEqual(1, interval.Maximum);
            Assert.AreEqual(IntervalEndpoint.Closed, interval.MinimumEndpoint);
            Assert.AreEqual(IntervalEndpoint.Closed, interval.MaximumEndpoint);

            Interval interval2 = new Interval(1, 2, IntervalEndpoint.Open, IntervalEndpoint.Open);

            Interval interval3 = interval2;
            Assert.AreEqual(interval2, interval3);
            Assert.AreNotEqual(interval, interval3);
        }

        [Test]
        public void Contains()
        {
            Interval interval = new Interval(0, 1, IntervalEndpoint.Closed, IntervalEndpoint.Open);
            Assert.IsFalse(interval.Contains(-1));
            Assert.IsTrue(interval.Contains(0));
            Assert.IsTrue(interval.Contains(0.5));
            Assert.IsFalse(interval.Contains(1));
            Assert.IsFalse(interval.Contains(2));
        }
    }
}
