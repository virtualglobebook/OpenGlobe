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
    internal class UniformIntVector2GL3x : Uniform<Vector2i>, ICleanable
    {
        internal UniformIntVector2GL3x(string name, int location)
            : base(name, location, UniformType.IntVector2)
        {
            Set(new Vector2i());
        }

        private void Set(Vector2i value)
        {
            _value = value;
            _dirty = true;
        }

        #region Uniform<> Members

        public override Vector2i Value
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
                GL.Uniform2(Location, _value.X, _value.Y);
                _dirty = false;
            }
        }

        #endregion

        private Vector2i _value;
        private bool _dirty;
    }
}
