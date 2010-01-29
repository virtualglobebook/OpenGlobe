#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using OpenTK;
using OpenTK.Graphics.OpenGL;
using MiniGlobe.Renderer;

namespace MiniGlobe.Renderer.GL32
{
    internal class UniformIntVector4GL32 : Uniform<Vector4i>, ICleanable
    {
        internal UniformIntVector4GL32(int programHandle, string name, int location)
            : base(name, location, UniformType.IntVector4)
        {
            int[] initialValue = new int[4];
            GL.GetUniform(programHandle, location, initialValue);
            _value = new Vector4i(initialValue[0], initialValue[1], initialValue[2], initialValue[3]);
        }

        #region ICleanable Uniform<>

        public override Vector4i Value
        {
            set
            {
                if (_value != value)
                {
                    _value = value;
                    _dirty = true;
                }
            }

            get { return _value; }
        }

        #endregion

        #region ICleanable Members

        public void Clean()
        {
            if (_dirty)
            {
                GL.Uniform4(Location, _value.X, _value.Y, _value.Z, _value.Y);
                _dirty = false;
            }
        }

        #endregion

        private Vector4i _value;
        private bool _dirty;
    }
}
