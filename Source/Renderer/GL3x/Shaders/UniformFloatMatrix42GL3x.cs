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
    internal class UniformFloatMatrix42GL3x : Uniform<Matrix42>, ICleanable
    {
        internal UniformFloatMatrix42GL3x(string name, int location, ICleanableObserver observer)
            : base(name, location, UniformType.FloatMatrix42)
        {
            _dirty = true;
            _observer = observer;
            _observer.NotifyDirty(this);
        }

        private void Set(Matrix42 value)
        {
            if (!_dirty && (_value != value))
            {
                _dirty = true;
                _observer.NotifyDirty(this);
            }

            _value = value;
        }

        #region Uniform<> Members

        public override Matrix42 Value
        {
            set { Set(value); }
            get { return _value; }
        }

        #endregion

        #region ICleanable Members

        public void Clean()
        {
            Vector2 column0 = _value.Column0;
            Vector2 column1 = _value.Column1;
            Vector2 column2 = _value.Column2;
            Vector2 column3 = _value.Column3;

            float[] columnMajorElements = new float[] { 
            column0.X, column0.Y, 
            column1.X, column1.Y, 
            column2.X, column2.Y,
            column3.X, column3.Y};

            GL.UniformMatrix4x2(Location, 1, false, columnMajorElements);

            _dirty = false;
        }

        #endregion

        private Matrix42 _value;
        private bool _dirty;
        private ICleanableObserver _observer;
    }
}
