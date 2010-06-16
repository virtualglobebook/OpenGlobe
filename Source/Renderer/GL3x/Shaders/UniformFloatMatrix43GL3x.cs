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
using OpenGlobe.Renderer;

namespace OpenGlobe.Renderer.GL3x
{
    internal class UniformFloatMatrix43GL3x : Uniform<Matrix43>, ICleanable
    {
        internal UniformFloatMatrix43GL3x(string name, int location, ICleanableObserver observer)
            : base(name, location, UniformType.FloatMatrix43)
        {
            _dirty = true;
            _observer = observer;
            _observer.NotifyDirty(this);
        }

        private void Set(Matrix43 value)
        {
            if (!_dirty && (_value != value))
            {
                _dirty = true;
                _observer.NotifyDirty(this);
            }

            _value = value;
        }

        #region Uniform<> Members

        public override Matrix43 Value
        {
            set { Set(value); }
            get { return _value; }
        }

        #endregion

        #region ICleanable Members

        public void Clean()
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

        #endregion

        private Matrix43 _value;
        private bool _dirty;
        private readonly ICleanableObserver _observer;
    }
}
