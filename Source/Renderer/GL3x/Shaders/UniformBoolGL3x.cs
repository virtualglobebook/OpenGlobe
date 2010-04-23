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
    internal class UniformBoolGL3x : Uniform<bool>, ICleanable
    {
        internal UniformBoolGL3x(string name, int location, ICleanableObserver observer)
            : base(name, location, UniformType.Bool)
        {
            _dirty = true;
            _observer = observer;
            _observer.NotifyDirty(this);
        }

        private void Set(bool value)
        {
            if (!_dirty && (_value != value))
            {
                _dirty = true;
                _observer.NotifyDirty(this);
            }

            _value = value;
        }

        #region Uniform<> Members

        public override bool Value 
        {
            set { Set(value); }
            get { return _value; }
        }

        #endregion
        
        #region ICleanable Members

        public void Clean()
        {
            GL.Uniform1(Location, _value ? 1 : 0);
            _dirty = false;
        }

        #endregion

        private bool _value;
        private bool _dirty;
        private readonly ICleanableObserver _observer;
    }
}
