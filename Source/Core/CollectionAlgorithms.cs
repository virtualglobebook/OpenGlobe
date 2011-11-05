#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
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
                return new List<T>(enumerable);
            }
        }

        public static T[] EnumerableToArray<T>(IEnumerable<T> enumerable)
        {
            if (enumerable == null)
            {
                throw new ArgumentNullException("enumerable");
            }

            T[] array = enumerable as T[];

            if (array != null)
            {
                return array;
            }
            else
            {
                int count = EnumerableCount(enumerable);
                T[] newArray = new T[count];

                int i = 0;
                foreach (T t in enumerable)
                {
                    newArray[i++] = t;
                }

                return newArray;
            }
        }

        public static IList<T> CopyEnumerableToList<T>(IEnumerable<T> enumerable)
        {
            if (enumerable == null)
            {
                throw new ArgumentNullException("enumerable");
            }

            return new List<T>(enumerable);
        }
    }
}