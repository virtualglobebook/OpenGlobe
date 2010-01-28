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
    internal class UniformBoolVector3GL32 : Uniform<Vector3b>, ICleanable
    {
        internal UniformBoolVector3GL32(int programHandle, string name, int location)
            : base(name, location, UniformType.BoolVector3)
        {
            int[] initialValue = new int[3];
            GL.GetUniform(programHandle, location, initialValue);
            _value = new Vector3b((initialValue[0] != 0), (initialValue[1] != 0), (initialValue[2] != 0));
        }

        #region ICleanable Uniform<>

        public override Vector3b Value
        {
            set
            {
                if (_value != value)
                {
                    _value = value;
                    _dirty = true;
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
                GL.Uniform3(Location, _value.X ? 1 : 0, _value.Y ? 1 : 0, _value.Z ? 1 : 0);
                _dirty = false;
            }
        }

        #endregion

        private Vector3b _value;
        private bool _dirty;
    }
}
