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

namespace MiniGlobe.Renderer.GL32
{
    internal class UniformFloatMatrix44GL32 : Uniform<Matrix4>, ICleanable
    {
        internal UniformFloatMatrix44GL32(int programHandle, string name, int location)
            : base(name, location, UniformType.FloatMatrix44)
        {
            Set(new Matrix4());
        }

        private void Set(Matrix4 value)
        {
            _value = value;
            _dirty = true;
        }

        #region Uniform<> Members

        public override Matrix4 Value
        {
            set
            {
                if (_value != value)
                {
                    Set(value);
                }
            }

            get { return _value; }
        }

        #endregion

        #region ICleanable Members

        public void Clean()
        {
            if (_dirty)
            {
                GL.UniformMatrix4(Location, false, ref _value);
                _dirty = false;
            }
        }

        #endregion

        private Matrix4 _value;
        private bool _dirty;
    }
}
