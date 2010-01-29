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

namespace MiniGlobe.Renderer.GL32
{
    internal class UniformBlockGL32 : UniformBlock
    {
        internal UniformBlockGL32(string name, int sizeInBytes, int bindHandle)
            : base(name, sizeInBytes)
        {
            _bindHandle = bindHandle;
        }

        #region UniformBlock Members

        public override void Bind(UniformBuffer uniformBuffer)
        {
            // TODO: avoid duplicate binds?
            int bufferHandle = (uniformBuffer as UniformBufferGL32).Handle;
            GL.BindBufferBase(BufferTarget.UniformBuffer, _bindHandle, bufferHandle);
        }

        #endregion

        private int _bindHandle;
    }
}
