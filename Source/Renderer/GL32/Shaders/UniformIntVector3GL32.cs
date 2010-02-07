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
    internal class UniformIntVector3GL32 : Uniform<Vector3i>, ICleanable
    {
        internal UniformIntVector3GL32(string name, int location)
            : base(name, location, UniformType.IntVector3)
        {
            Set(new Vector3i());
        }

        private void Set(Vector3i value)
        {
            _value = value;
            _dirty = true;
        }

        #region Uniform<> Members

        public override Vector3i Value
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

        private Vector3i _value;
        private bool _dirty;
    }
}
