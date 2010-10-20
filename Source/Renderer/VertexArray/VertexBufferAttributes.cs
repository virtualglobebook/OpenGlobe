#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System.Collections;
using OpenGlobe.Core;

namespace OpenGlobe.Renderer
{
    public abstract class VertexBufferAttributes : Disposable
    {
        public abstract VertexBufferAttribute this[int index] { get; set; }
        public abstract int Count { get; }
        public abstract int MaximumCount { get; }
        public abstract IEnumerator GetEnumerator();
    }
}
