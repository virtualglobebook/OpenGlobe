#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using OpenGlobe.Core;

namespace OpenGlobe.Renderer
{
    /// <summary>
    /// Does not own vertex and index buffers.  They must be disposed.
    /// </summary>
    public class MeshBuffers
    {
        public virtual VertexBufferAttributes Attributes
        {
            get { return _attributes; }
        }

        public IndexBuffer IndexBuffer { get; set; }

        private MeshVertexBufferAttributes _attributes = new MeshVertexBufferAttributes();
    }
}
