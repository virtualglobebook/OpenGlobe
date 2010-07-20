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
        public virtual AttachedVertexBuffers VertexBuffers
        {
            get { return _attachedVertexBuffers; }
        }

        public IndexBuffer IndexBuffer { get; set; }

        private MeshAttachedVertexBuffers _attachedVertexBuffers = new MeshAttachedVertexBuffers();
    }
}
