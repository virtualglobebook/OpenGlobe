#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System.Runtime.InteropServices;

namespace OpenGlobe.Core
{
    public static class SizeInBytes<T>
    {
        public static readonly int Value = Marshal.SizeOf(typeof(T));
    }
}