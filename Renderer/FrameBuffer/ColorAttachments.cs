#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System.Collections;

namespace MiniGlobe.Renderer
{
    public abstract class ColorAttachments
    {
        public abstract Texture2D this[int index] { get; set; }
        public abstract int Count { get; }
        public abstract IEnumerator GetEnumerator();
    }
}
