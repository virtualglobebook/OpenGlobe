#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Collections.Generic;

namespace OpenGlobe.Core
{
    public static class CollectionAlgorithms
    {
        public static int EnumerableCount<T>(IEnumerable<T> enumerable)
        {
            if (enumerable == null)
            {
                throw new ArgumentNullException("enumerable");
            }

            IList<T> list = enumerable as IList<T>;

            if (list != null)
            {
                return list.Count;
            }

            int count = 0;
            #pragma warning disable 219
            foreach (T t in enumerable)
            {
                ++count;
            }
            #pragma warning restore 219

            return count;
        }

        public static bool EnumerableCountGreaterThanOrEqual<T>(IEnumerable<T> enumerable, int minimumCount)
        {
            if (enumerable == null)
            {
                throw new ArgumentNullException("enumerable");
            }

            if (minimumCount < 0)
            {
                throw new ArgumentOutOfRangeException("minimumCount");
            }

            IList<T> list = enumerable as IList<T>;

            if (list != null)
            {
                return list.Count >= minimumCount;
            }

            int count = 0;
            #pragma warning disable 219
            foreach (T t in enumerable)
            {
                if (++count >= minimumCount)
                {
                    return true;
                }
            }
            #pragma warning restore 219

            return false;
        }

        public static IList<T> EnumerableToList<T>(IEnumerable<T> enumerable)
        {
            if (enumerable == null)
            {
                throw new ArgumentNullException("enumerable");
            }

            IList<T> list = enumerable as IList<T>;

            if (list != null)
            {
                return list;
            }
            else
            {
                int count = EnumerableCount(enumerable);
                IList<T> newList = new List<T>(count);

                foreach (T t in enumerable)
                {
                    newList.Add(t);
                }

                return newList;
            }
        }

        public static IList<T> CopyEnumerableToList<T>(IEnumerable<T> enumerable)
        {
            if (enumerable == null)
            {
                throw new ArgumentNullException("enumerable");
            }

            int count = EnumerableCount(enumerable);
            IList<T> newList = new List<T>(count);

            foreach (T t in enumerable)
            {
                newList.Add(t);
            }

            return newList;
        }
    }
}