#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using OpenTK.Graphics.OpenGL;
using MiniGlobe.Renderer;

namespace MiniGlobe.Renderer.GL3x
{
    internal class UniformFloatGL3x : Uniform<float>, ICleanable
    {
        internal UniformFloatGL3x(string name, int location, ICleanableObserver observer)
            : base(name, location, UniformType.Float)
        {
            _dirty = true;
            _observer = observer;
            _observer.NotifyDirty(this);
        }

        private void Set(float value)
        {
            if (!_dirty && (_value != value))
            {
                _dirty = true;
                _observer.NotifyDirty(this);
            }

            _value = value;
        }

        #region Uniform<> Members

        public override float Value
        {
            set { Set(value); }
            get { return _value; }
        }

        #endregion

        #region ICleanable Members

        public void Clean()
        {
            GL.Uniform1(Location, _value);
            _dirty = false;
        }

        #endregion

        private float _value;
        private bool _dirty;
        private ICleanableObserver _observer;
    }
}
