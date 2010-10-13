#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using OpenTK.Graphics.OpenGL;
using OpenGlobe.Renderer;

namespace OpenGlobe.Renderer.GL3x
{
    internal class UniformIntGL3x : Uniform<int>, ICleanable
    {
        internal UniformIntGL3x(string name, int location, UniformType type, ICleanableObserver observer)
            : base(name, location, type)
        {
            _dirty = true;
            _observer = observer;
            _observer.NotifyDirty(this);
        }


        #region Uniform<> Members

        public override int Value
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
            GL.Uniform1(Location, _value);
            _dirty = false;
        }

        #endregion

        private int _value;
        private bool _dirty;
        private readonly ICleanableObserver _observer;
    }
}
