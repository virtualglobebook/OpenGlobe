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
    internal class UniformFloatMatrix24GL32 : Uniform<Matrix24>, ICleanable
    {
        internal UniformFloatMatrix24GL32(int programHandle, string name, int location)
            : base(name, location, UniformType.FloatMatrix24)
        {
            float[] initialValue = new float[8];
            GL.GetUniform(programHandle, location, initialValue);

            Vector2 rowOne = new Vector2(initialValue[0], initialValue[4]);
            Vector2 rowTwo = new Vector2(initialValue[1], initialValue[5]);
            Vector2 rowThree = new Vector2(initialValue[2], initialValue[6]);
            Vector2 rowFour = new Vector2(initialValue[3], initialValue[7]);

            _value = new Matrix24(rowOne, rowTwo, rowThree, rowFour);
        }

        #region ICleanable Uniform<>

        public override Matrix24 Value
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

                float[] columnMajorElements = new float[] { 
                column0.X, column0.Y, column0.Z, column0.W,
                column1.X, column1.Y, column1.Z, column0.W };

                GL.UniformMatrix2x4(Location, 1, false, columnMajorElements);

                _dirty = false;
            }
        }

        #endregion

        private Matrix24 _value;
        private bool _dirty;
    }
}
