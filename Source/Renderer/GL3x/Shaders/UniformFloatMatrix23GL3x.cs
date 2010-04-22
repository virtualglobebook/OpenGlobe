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

namespace MiniGlobe.Renderer.GL3x
{
    internal class UniformFloatMatrix23GL3x : Uniform<Matrix23>, ICleanable
    {
        internal UniformFloatMatrix23GL3x(string name, int location)
            : base(name, location, UniformType.FloatMatrix23)
        {
            Set(new Matrix23());
        }

        private void Set(Matrix23 value)
        {
            _value = value;
            _dirty = true;
        }

        #region Uniform<> Members

        public override Matrix23 Value
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
