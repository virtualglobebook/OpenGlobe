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
    internal class UniformFloatMatrix44GL32 : Uniform<Matrix4>, ICleanable
    {
        internal UniformFloatMatrix44GL32(int programHandle, string name, int location)
            : base(name, location, UniformType.FloatMatrix44)
        {
            float[] initialValue = new float[16];
            GL.GetUniform(programHandle, location, initialValue);

            Vector4 rowOne = new Vector4(initialValue[0], initialValue[4], initialValue[8], initialValue[12]);
            Vector4 rowTwo = new Vector4(initialValue[1], initialValue[5], initialValue[9], initialValue[13]);
            Vector4 rowThree = new Vector4(initialValue[2], initialValue[6], initialValue[10], initialValue[14]);
            Vector4 rowFour = new Vector4(initialValue[3], initialValue[7], initialValue[11], initialValue[15]);
            
            _value = new Matrix4(rowOne, rowTwo, rowThree, rowFour);
        }

        #region ICleanable Uniform<>

        public override Matrix4 Value
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
                GL.UniformMatrix4(Location, false, ref _value);
                _dirty = false;
            }
        }

        #endregion

        private Matrix4 _value;
        private bool _dirty;
    }
}
