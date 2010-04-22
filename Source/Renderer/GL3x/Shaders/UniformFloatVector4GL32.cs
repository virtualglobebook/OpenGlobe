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
using MiniGlobe.Core;
using MiniGlobe.Renderer;

namespace MiniGlobe.Renderer.GL32
{
    internal class UniformFloatVector4GL32 : Uniform<Vector4S>, ICleanable
    {
        internal UniformFloatVector4GL32(string name, int location)
            : base(name, location, UniformType.FloatVector4)
        {
            Set(new Vector4S());
        }

        private void Set(Vector4S value)
        {
            _value = value;
            _dirty = true;
        }

        #region Uniform<> Members

        public override Vector4S Value
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
                GL.Uniform4(Location, _value.X, _value.Y, _value.Z, _value.W);
                _dirty = false;
            }
        }

        #endregion

        private Vector4S _value;
        private bool _dirty;
    }
}
