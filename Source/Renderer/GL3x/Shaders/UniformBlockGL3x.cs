#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System.Diagnostics;
using OpenTK.Graphics.OpenGL;

namespace MiniGlobe.Renderer.GL3x
{
    internal class UniformBlockGL3x : UniformBlock
    {
        internal UniformBlockGL3x(string name, int sizeInBytes, int bindHandle)
            : base(name, sizeInBytes)
        {
            _bindHandle = bindHandle;
        }

        #region UniformBlock Members

        public override void Bind(UniformBuffer uniformBuffer)
        {
            // TODO: avoid duplicate binds?
            BufferHandleGL3x bufferHandle = (uniformBuffer as UniformBufferGL3x).Handle;
            GL.BindBufferBase(BufferTarget.UniformBuffer, _bindHandle, bufferHandle.Value);
        }

        #endregion

        private int _bindHandle;
    }
}
