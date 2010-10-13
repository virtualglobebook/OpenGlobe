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
    internal class UniformFloatMatrix44GL3x : Uniform<Matrix4>, ICleanable
    {
        internal UniformFloatMatrix44GL3x(string name, int location, ICleanableObserver observer)
            : base(name, UniformType.FloatMatrix44)
        {
            _location = location;
            _dirty = true;
            _observer = observer;
            _observer.NotifyDirty(this);
        }

        #region Uniform<> Members

        public override Matrix4 Value
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
            GL.UniformMatrix4(_location, false, ref _value);
            _dirty = false;
        }

        #endregion

        private int _location;
        private Matrix4 _value;
        private bool _dirty;
        private readonly ICleanableObserver _observer;
    }
}
