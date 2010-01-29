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
    internal class UniformFloatMatrix32GL32 : Uniform<Matrix32>, ICleanable
    {
        internal UniformFloatMatrix32GL32(int programHandle, string name, int location)
            : base(name, location, UniformType.FloatMatrix32)
        {
            float[] initialValue = new float[6];
            GL.GetUniform(programHandle, location, initialValue); // TODO:  These come back wrong.  Driver bug?

            Vector3 rowOne = new Vector3(initialValue[0], initialValue[2], initialValue[4]);
            Vector3 rowTwo = new Vector3(initialValue[1], initialValue[3], initialValue[5]);

            _value = new Matrix32(rowOne, rowTwo);
        }

        #region ICleanable Uniform<>

        public override Matrix32 Value
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
                Vector2 column0 = _value.Column0;
                Vector2 column1 = _value.Column1;
                Vector2 column2 = _value.Column2;

                float[] columnMajorElements = new float[] { 
                column0.X, column0.Y, 
                column1.X, column1.Y, 
                column2.X, column2.Y };

                GL.UniformMatrix3x2(Location, 1, false, columnMajorElements);

                _dirty = false;
            }
        }

        #endregion

        private Matrix32 _value;
        private bool _dirty;
    }
}
