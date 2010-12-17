#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System.Collections;
using OpenGlobe.Core;

namespace OpenGlobe.Renderer
{
    public abstract class VertexBufferAttributes
    {
        public abstract VertexBufferAttribute this[int index] { get; set; }
        public abstract int Count { get; }
        public abstract int MaximumCount { get; }
        public abstract IEnumerator GetEnumerator();
    }
}
