#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System.Diagnostics;
using OpenTK.Graphics.OpenGL;

namespace OpenGlobe.Renderer.GL3x
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
            BufferNameGL3x bufferHandle = ((UniformBufferGL3x)uniformBuffer).Handle;
            GL.BindBufferBase(BufferTarget.UniformBuffer, _bindHandle, bufferHandle.Value);
        }

        #endregion

        private int _bindHandle;
    }
}
