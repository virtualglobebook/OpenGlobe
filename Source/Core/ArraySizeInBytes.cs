#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
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