#region License
//
// (C) Copyright 2010 Patrick Cozzi, Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace OpenGlobe.Core
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
            int[] list = new int[] { 0, 1 };
            Assert.AreEqual(2, CollectionAlgorithms.EnumerableCount(list));

            Dictionary<int, int> dictionary = new Dictionary<int, int>();
            dictionary.Add(0, 0);
            dictionary.Add(1, 1);

            IEnumerable<KeyValuePair<int, int>> enumerable = dictionary;
            Assert.AreEqual(2, CollectionAlgorithms.EnumerableCount(enumerable));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EnumerableToListNull()
        {
            CollectionAlgorithms.EnumerableToList<int>(null);
        }

        [Test]
        public void EnumerableToList()
        {
            IList<int> list = new List<int>();
            list.Add(0);
            list.Add(1);

            IList<int> returnedList = CollectionAlgorithms.EnumerableToList(list);

            Assert.AreEqual(2, returnedList.Count);
            Assert.AreEqual(0, returnedList[0]);
            Assert.AreEqual(1, returnedList[1]);
        }

        [Test]
        public void EnumerableToList2()
        {
            Dictionary<int, int> dictionary = new Dictionary<int, int>();
            dictionary.Add(0, 0);
            dictionary.Add(1, 1);

            IList<KeyValuePair<int, int>> returnedList = CollectionAlgorithms.EnumerableToList(dictionary);

            Assert.AreEqual(2, returnedList.Count);
            Assert.AreEqual(new KeyValuePair<int, int>(0, 0), returnedList[0]);
            Assert.AreEqual(new KeyValuePair<int, int>(1, 1), returnedList[1]);
        }
    }
}
