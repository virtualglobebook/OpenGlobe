#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using OpenGlobe.Core;

namespace OpenGlobe.Renderer
{
    public abstract class VertexArray : Disposable
    {
        public virtual VertexBufferAttributes Attributes
        {
            get { return null; }
        }

        public virtual IndexBuffer IndexBuffer
        {
            get { return null; }
            set { }
        }

        public bool DisposeBuffers { get; set; }
    }
}
