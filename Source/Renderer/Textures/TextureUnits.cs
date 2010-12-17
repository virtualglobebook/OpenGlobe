#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System.Collections;

namespace OpenGlobe.Renderer
{
    public abstract class TextureUnits
    {
        public abstract TextureUnit this[int index] { get; }
        public abstract int Count { get; }
        public abstract IEnumerator GetEnumerator();
    }
}
