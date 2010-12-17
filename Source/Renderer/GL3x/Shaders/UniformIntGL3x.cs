#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using OpenTK.Graphics.OpenGL;
using OpenGlobe.Renderer;

namespace OpenGlobe.Renderer.GL3x
{
    internal class UniformIntGL3x : Uniform<int>, ICleanable
    {
        internal UniformIntGL3x(string name, int location, UniformType type, ICleanableObserver observer)
            : base(name, type)
        {
            _location = location;
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
            GL.Uniform1(_location, _value);
            _dirty = false;
        }

        #endregion

        private int _location;
        private int _value;
        private bool _dirty;
        private readonly ICleanableObserver _observer;
    }
}
