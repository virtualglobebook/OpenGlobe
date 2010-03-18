#region License
//
// (C) Copyright 2010 Patrick Cozzi, Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace MiniGlobe.Core
{
    [TestFixture]
    public class CollectionAlgorithmsTests
    {
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EnumerableCountNull()
        {
            CollectionAlgorithms.EnumerableCount<int>(null);
        }

        [Test]
        public void EnumerableCount()
        {
            int[] list = new int[] { 0, 1};
            Assert.AreEqual(2, CollectionAlgorithms.EnumerableCount(list));

            Dictionary<int, int> dictionary = new Dictionary<int, int>();
            dictionary.Add(0, 0);
            dictionary.Add(1, 1);
            Assert.AreEqual(2, CollectionAlgorithms.EnumerableCount(dictionary));
        }
    }
}
