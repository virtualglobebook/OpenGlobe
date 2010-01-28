#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace MiniGlobe
{
    public static class MiniGlobeAssert
    {
        public static void ListsAreEqual<T>(IList<T> left, IList<T> right) where T : IEquatable<T>
        {
            Assert.AreEqual(left.Count, right.Count);

            for (int i = 0; i < left.Count; ++i)
            {
                Assert.IsTrue(left[i].Equals(right[i]), "i = " + i + "\nleft[i] == " + left[i].ToString() + "\nright[i] == " + right[i].ToString());
            }
        }
    }
}
