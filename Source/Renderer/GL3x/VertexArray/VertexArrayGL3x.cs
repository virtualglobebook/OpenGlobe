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
using MiniGlobe.Renderer;

namespace MiniGlobe.Renderer.GL3x
{
    internal class VertexArrayGL3x : VertexArray
    {
        public VertexArrayGL3x()
	    {
            GL.GenVertexArrays(1, out _handle);
            _attachedVertexBuffers = new AttachedVertexBuffersGL3x();
        }

        internal void Bind()
        {
            GL.BindVertexArray(_handle);
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
                GL.DeleteVertexArrays(1, ref _handle);
            }
            base.Dispose(disposing);
        }

        #endregion
        
        private int _handle;
        private AttachedVertexBuffersGL3x _attachedVertexBuffers;
        private IndexBuffer _attachedIndexBuffer;
        private bool _dirtyIndexBuffer;
    }
}
