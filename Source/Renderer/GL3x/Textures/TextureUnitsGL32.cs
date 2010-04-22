#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System.Collections;
using OpenTK.Graphics.OpenGL;
using MiniGlobe.Renderer;

namespace MiniGlobe.Renderer.GL32
{
    internal class TextureUnitsGL32 : TextureUnits
    {
        public TextureUnitsGL32()
        {
            int textureUnits;
            GL.GetInteger(GetPName.MaxCombinedTextureImageUnits, out textureUnits);

            _textureUnits = new TextureUnit[textureUnits];
            _textureUnitGL32s = new TextureUnitGL32[textureUnits];
            for (int i = 0; i < textureUnits; ++i)
            {
                TextureUnitGL32 textureUnit = new TextureUnitGL32(i, (i == (textureUnits - 1)));
                _textureUnits[i] = textureUnit;
                _textureUnitGL32s[i] = textureUnit;
            }
        }

        #region TextureUnits Members

        public override TextureUnit this[int index]
        {
            get { return _textureUnits[index]; }
        }

        public override int Count
        {
            get { return _textureUnits.Length; }
        }

        public override IEnumerator GetEnumerator()
        {
            return _textureUnits.GetEnumerator();
        }

        #endregion

        internal void Clean()
        {
            for (int i = 0; i < _textureUnits.Length; ++i)
            {
                _textureUnitGL32s[i].Clean();
            }
        }

        private TextureUnit[] _textureUnits;
        private TextureUnitGL32[] _textureUnitGL32s;
    }
}
