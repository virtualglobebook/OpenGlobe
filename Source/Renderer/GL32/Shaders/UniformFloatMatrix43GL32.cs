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
    internal class UniformFloatMatrix43GL32 : Uniform<Matrix43>, ICleanable
    {
        internal UniformFloatMatrix43GL32(int programHandle, string name, int location)
            : base(name, location, UniformType.FloatMatrix43)
        {
            Set(new Matrix43());
        }

        private void Set(Matrix43 value)
        {
            _value = value;
            _dirty = true;
        }

        #region Uniform<> Members

        public override Matrix43 Value
        {
            set
            {
                if (_value != value)
                {
                    Set(value);
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
                Vector3 column3 = _value.Column3;

                float[] columnMajorElements = new float[] { 
                column0.X, column0.Y, column0.Z, 
                column1.X, column1.Y, column1.Z, 
                column2.X, column2.Y, column2.Z, 
                column3.X, column3.Y, column3.Z };

                GL.UniformMatrix4x3(Location, 1, false, columnMajorElements);

                _dirty = false;
            }
        }

        #endregion

        private Matrix43 _value;
        private bool _dirty;
    }
}
