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
    internal class UniformFloatMatrix23GL32 : Uniform<Matrix23>, ICleanable
    {
        internal UniformFloatMatrix23GL32(int programHandle, string name, int location)
            : base(name, location, UniformType.FloatMatrix23)
        {
            float[] initialValue = new float[6];
            GL.GetUniform(programHandle, location, initialValue); // TODO:  These come back wrong.  Driver bug?

            Vector2 rowOne = new Vector2(initialValue[0], initialValue[3]);
            Vector2 rowTwo = new Vector2(initialValue[1], initialValue[4]);
            Vector2 rowThree = new Vector2(initialValue[2], initialValue[5]);

            _value = new Matrix23(rowOne, rowTwo, rowThree);
        }

        #region ICleanable Uniform<>

        public override Matrix23 Value
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

                float[] columnMajorElements = new float[] { 
                column0.X, column0.Y, column0.Z,
                column1.X, column1.Y, column1.Z };

                GL.UniformMatrix2x3(Location, 1, false, columnMajorElements);

                _dirty = false;
            }
        }

        #endregion

        private Matrix23 _value;
        private bool _dirty;
    }
}
