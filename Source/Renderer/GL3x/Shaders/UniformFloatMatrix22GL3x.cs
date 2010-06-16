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
    internal class UniformFloatMatrix22GL3x : Uniform<Matrix2>, ICleanable
    {
        internal UniformFloatMatrix22GL3x(string name, int location, ICleanableObserver observer)
            : base(name, location, UniformType.FloatMatrix22)
        {
            _dirty = true;
            _observer = observer;
            _observer.NotifyDirty(this);
        }

        private void Set(Matrix2 value)
        {
            if (!_dirty && (_value != value))
            {
                _dirty = true;
                _observer.NotifyDirty(this);
            }

            _value = value;
        }

        #region Uniform<> Members

        public override Matrix2 Value
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

            float[] columnMajorElements = new float[] { 
            column0.X, column0.Y,
            column1.X, column1.Y };

            GL.UniformMatrix2(Location, 1, false, columnMajorElements);

            _dirty = false;
        }

        #endregion

        private Matrix2 _value;
        private bool _dirty;
        private readonly ICleanableObserver _observer;
    }
}
