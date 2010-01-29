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
    internal class UniformFloatMatrix33GL32 : Uniform<Matrix3>, ICleanable
    {
        internal UniformFloatMatrix33GL32(int programHandle, string name, int location)
            : base(name, location, UniformType.FloatMatrix33)
        {
            float[] initialValue = new float[9];
            GL.GetUniform(programHandle, location, initialValue); // TODO:  These come back wrong.  Driver bug?

            Vector3 rowOne = new Vector3(initialValue[0], initialValue[3], initialValue[6]);
            Vector3 rowTwo = new Vector3(initialValue[1], initialValue[4], initialValue[7]);
            Vector3 rowThree = new Vector3(initialValue[2], initialValue[5], initialValue[8]);

            _value = new Matrix3(rowOne, rowTwo, rowThree);
        }

        #region ICleanable Uniform<>

        public override Matrix3 Value
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
                Vector3 column0 = _value.Column0;
                Vector3 column1 = _value.Column1;
                Vector3 column2 = _value.Column2;

                float[] columnMajorElements = new float[] { 
                column0.X, column0.Y, column0.Z,
                column1.X, column1.Y, column1.Z,
                column2.X, column2.Y, column2.Z };

                GL.UniformMatrix3(Location, 1, false, columnMajorElements);

                _dirty = false;
            }
        }

        #endregion

        private Matrix3 _value;
        private bool _dirty;
    }
}
