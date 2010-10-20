#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using OpenGlobe.Core;

namespace OpenGlobe.Renderer
{
    public class MeshBuffers : Disposable
    {
        public virtual VertexBufferAttributes VertexBuffers
        {
            get { return _vertexBufferAttributes; }
        }

        public IndexBuffer IndexBuffer { get; set; }

        private MeshVertexBufferAttributes _vertexBufferAttributes = new MeshVertexBufferAttributes();
    }
}
