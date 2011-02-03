#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using OpenGlobe.Renderer;

namespace OpenGlobe.Renderer.GL3x
{
    internal class FragmentOutputsGL3x : FragmentOutputs
    {
        public FragmentOutputsGL3x(int programHandle)
        {
            _programHandle = programHandle;
        }

        #region FragmentOutputs Members

        public override int this[string index]
        {
            get 
            {
                int i = GL.GetFragDataLocation(_programHandle, index);

                if (i == -1)
                {
                    throw new KeyNotFoundException(index);
                }

                return i;
            }
        }

        #endregion

        private int _programHandle;
    }
}
