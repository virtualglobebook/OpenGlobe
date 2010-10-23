#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Diagnostics;
using OpenTK.Graphics.OpenGL;
using OpenGlobe.Renderer;

namespace OpenGlobe.Renderer.GL3x
{
    internal class VertexArrayGL3x : VertexArray
    {
        public VertexArrayGL3x()
	    {
            _handle = new VertexArrayNameGL3x();
            _attributes = new VertexBufferAttributesGL3x();
        }

        internal void Bind()
        {
            GL.BindVertexArray(_handle.Value);
        }

        internal void Clean()
        {
            _attributes.Clean();

            if (_dirtyIndexBuffer)
            {
                if (_indexBuffer != null)
                {
                    IndexBufferGL3x bufferObjectGL = _indexBuffer as IndexBufferGL3x;
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
                _attributes.Dispose();
                if (_indexBuffer != null)
                {
                    _indexBuffer.Dispose();
                }
                _handle.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion

        private VertexArrayNameGL3x _handle;
        private VertexBufferAttributesGL3x _attributes;
        private IndexBuffer _indexBuffer;
        private bool _dirtyIndexBuffer;
    }
}
