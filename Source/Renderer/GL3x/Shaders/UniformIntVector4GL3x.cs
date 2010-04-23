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
    internal class UniformIntVector4GL3x : Uniform<Vector4i>, ICleanable
    {
        internal UniformIntVector4GL3x(string name, int location, ICleanableObserver observer)
            : base(name, location, UniformType.IntVector4)
        {
            _dirty = true;
            _observer = observer;
            _observer.NotifyDirty(this);
        }

        private void Set(Vector4i value)
        {
            if (!_dirty && (_value != value))
            {
                _dirty = true;
                _observer.NotifyDirty(this);
            }

            _value = value;
        }

        #region Uniform<> Members

        public override Vector4i Value
        {
            set { Set(value); }
            get { return _value; }
        }

        #endregion

        #region ICleanable Members

        public void Clean()
        {
            GL.Uniform4(Location, _value.X, _value.Y, _value.Z, _value.Y);
            _dirty = false;
        }

        #endregion

        private Vector4i _value;
        private bool _dirty;
        private readonly ICleanableObserver _observer;
    }
}
