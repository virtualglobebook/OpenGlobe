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
    internal class UniformFloatMatrix24GL3x : Uniform<Matrix24>, ICleanable
    {
        internal UniformFloatMatrix24GL3x(string name, int location, ICleanableObserver observer)
            : base(name, UniformType.FloatMatrix24)
        {
            _location = location;
            _dirty = true;
            _observer = observer;
            _observer.NotifyDirty(this);
        }

        #region Uniform<> Members

        public override Matrix24 Value
        {
            set
            {
                if (!_dirty && (_value != value))
                {
                    _dirty = true;
                    _observer.NotifyDirty(this);
                }

                _value = value;
            }

            get { return _value; }
        }

        #endregion

        #region ICleanable Members

        public void Clean()
        {
            Vector4 column0 = _value.Column0;
            Vector4 column1 = _value.Column1;

            float[] columnMajorElements = new float[] { 
            column0.X, column0.Y, column0.Z, column0.W,
            column1.X, column1.Y, column1.Z, column0.W };

            GL.UniformMatrix2x4(_location, 1, false, columnMajorElements);

            _dirty = false;
        }

        #endregion

        private int _location;
        private Matrix24 _value;
        private bool _dirty;
        private readonly ICleanableObserver _observer;
    }
}
