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

namespace MiniGlobe.Renderer.GL3x
{
    internal class UniformFloatVector3GL3x : Uniform<Vector3S>, ICleanable
    {
        internal UniformFloatVector3GL3x(string name, int location)
            : base(name, location, UniformType.FloatVector3)
        {
            Set(new Vector3S());
        }

        private void Set(Vector3S value)
        {
            _value = value;
            _dirty = true;
        }

        #region Uniform<> Members

        public override Vector3S Value
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
                GL.Uniform3(Location, _value.X, _value.Y, _value.Z);
                _dirty = false;
            }
        }

        #endregion

        private Vector3S _value;
        private bool _dirty;
    }
}
