#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTK.Graphics.OpenGL;
using OpenGlobe.Renderer;

namespace OpenGlobe.Renderer.GL3x
{
    internal class VertexArrayGL3x : VertexArray
    {
        public VertexArrayGL3x()
	    {
            _name = new VertexArrayNameGL3x();
            _attributes = new VertexBufferAttributesGL3x();
        }

        internal void Bind()
        {
            GL.BindVertexArray(_name.Value);
        }

        internal void Clean()
        {
            _attributes.Clean();

            if (_dirtyIndexBuffer)
            {
                if (_indexBuffer != null)
                {
                    IndexBufferGL3x bufferObjectGL = (IndexBufferGL3x)_indexBuffer;
                    bufferObjectGL.Bind();
                }
                else
                {
                    IndexBufferGL3x.UnBind();
                }

                _dirtyIndexBuffer = false;
            }
        }

        internal int MaximumArrayIndex()
        {
            return _attributes.MaximumArrayIndex;
        }

        #region VertexArray Members

        public override VertexBufferAttributes Attributes
        {
            get { return _attributes; }
        }

        public override IndexBuffer IndexBuffer
        {
            get { return _indexBuffer; }

            set
            {
                _indexBuffer = value;
                _dirtyIndexBuffer = true;
            }
        }

        #endregion

        #region Disposable Members

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (DisposeBuffers)
                {
                    //
                    // Multiple components may share the same vertex buffer, so
                    // find the unique set of vertex buffers used by this vertex array.
                    //
                    HashSet<VertexBuffer> vertexBuffers = new HashSet<VertexBuffer>();

                    foreach (VertexBufferAttribute attribute in _attributes)
                    {
                        vertexBuffers.Add(attribute.VertexBuffer);
                    }

                    foreach (VertexBuffer vb in vertexBuffers)
                    {
                        vb.Dispose();
                    }

                    if (_indexBuffer != null)
                    {
                        _indexBuffer.Dispose();
                    }
                }

                _name.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion

        private VertexArrayNameGL3x _name;
        private VertexBufferAttributesGL3x _attributes;
        private IndexBuffer _indexBuffer;
        private bool _dirtyIndexBuffer;
    }
}
