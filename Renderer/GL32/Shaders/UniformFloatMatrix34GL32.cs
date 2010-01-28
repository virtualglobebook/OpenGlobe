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
    internal class UniformFloatMatrix34GL32 : Uniform<Matrix34>, ICleanable
    {
        internal UniformFloatMatrix34GL32(int programHandle, string name, int location)
            : base(name, location, UniformType.FloatMatrix34)
        {
            float[] initialValue = new float[12];
            GL.GetUniform(programHandle, location, initialValue);

            Vector3 rowOne = new Vector3(initialValue[0], initialValue[4], initialValue[8]);
            Vector3 rowTwo = new Vector3(initialValue[1], initialValue[5], initialValue[9]);
            Vector3 rowThree = new Vector3(initialValue[2], initialValue[6], initialValue[10]);
            Vector3 rowFour = new Vector3(initialValue[3], initialValue[7], initialValue[11]);

            _value = new Matrix34(rowOne, rowTwo, rowThree, rowFour);
        }

        #region ICleanable Uniform<>

        public override Matrix34 Value
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
                Vector4 column0 = _value.Column0;
                Vector4 column1 = _value.Column1;
                Vector4 column2 = _value.Column2;

                float[] columnMajorElements = new float[] { 
                column0.X, column0.Y, column0.Z, column0.W, 
                column1.X, column1.Y, column1.Z, column1.W, 
                column2.X, column2.Y, column2.Z, column2.W };

                GL.UniformMatrix3x4(Location, 1, false, columnMajorElements);

                _dirty = false;
            }
        }

        #endregion

        private Matrix34 _value;
        private bool _dirty;
    }
}
