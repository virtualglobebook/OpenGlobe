#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System.Runtime.InteropServices;

namespace MiniGlobe.Core
{
    public static class SizeInBytes<T>
    {
        public static readonly int Value = Marshal.SizeOf(typeof(T));
    }
}