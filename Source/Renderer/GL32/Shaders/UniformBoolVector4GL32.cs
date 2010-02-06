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
    internal class UniformBoolVector4GL32 : Uniform<Vector4b>, ICleanable
    {
        internal UniformBoolVector4GL32(int programHandle, string name, int location)
            : base(name, location, UniformType.BoolVector4)
        {
            int[] initialValue = new int[4];
            GL.GetUniform(programHandle, location, initialValue);
            _value = new Vector4b(
                (initialValue[0] != 0), 
                (initialValue[1] != 0), 
                (initialValue[2] != 0), 
                (initialValue[3] != 0));
        }

        private void Set(Vector4b value)
        {
            _value = value;
            _dirty = true;
        }

        #region Uniform<> Members

        public override Vector4b Value
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
                GL.Uniform4(Location, _value.X ? 1 : 0, _value.Y ? 1 : 0, _value.Z ? 1 : 0, _value.Y ? 1 : 0);
                _dirty = false;
            }
        }

        #endregion

        private Vector4b _value;
        private bool _dirty;
    }
}
