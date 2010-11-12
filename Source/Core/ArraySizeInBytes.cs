#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;

namespace OpenGlobe.Core
{
    public static class ArraySizeInBytes
    {
        public static int Size<T>(T[] values)
        {
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }

            return values.Length * SizeInBytes<T>.Value;
        }
    }
}