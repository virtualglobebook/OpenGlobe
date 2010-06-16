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
            _attachedVertexBuffers = new AttachedVertexBuffersGL3x();
        }

        internal void Bind()
        {
            GL.BindVertexArray(_handle.Value);
        }

        internal void Clean()
        {
            _attachedVertexBuffers.Clean();

            if (_dirtyIndexBuffer)
            {
                if (_attachedIndexBuffer != null)
                {
                    IndexBufferGL3x bufferObjectGL = _attachedIndexBuffer as IndexBufferGL3x;
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
            return _attachedVertexBuffers.MaximumArrayIndex;
        }

        #region VertexArray Members

        public override AttachedVertexBuffers VertexBuffers
        {
            get { return _attachedVertexBuffers; }
        }

        public override IndexBuffer IndexBuffer
        {
            get { return _attachedIndexBuffer; }

            set
            {
                _attachedIndexBuffer = value;
                _dirtyIndexBuffer = true;
            }
        }

        #endregion

        #region Disposable Members

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _attachedVertexBuffers.Dispose();
                if (_attachedIndexBuffer != null)
                {
                    _attachedIndexBuffer.Dispose();
                }
                _handle.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion

        private VertexArrayNameGL3x _handle;
        private AttachedVertexBuffersGL3x _attachedVertexBuffers;
        private IndexBuffer _attachedIndexBuffer;
        private bool _dirtyIndexBuffer;
    }
}
