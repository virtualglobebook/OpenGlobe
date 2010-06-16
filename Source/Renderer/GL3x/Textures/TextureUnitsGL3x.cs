#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System.Collections;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using OpenGlobe.Renderer;

namespace OpenGlobe.Renderer.GL3x
{
    internal class TextureUnitsGL3x : TextureUnits, ICleanableObserver
    {
        public TextureUnitsGL3x()
        {
            int textureUnits;
            GL.GetInteger(GetPName.MaxCombinedTextureImageUnits, out textureUnits);

            _textureUnits = new TextureUnit[textureUnits];
            for (int i = 0; i < textureUnits; ++i)
            {
                TextureUnitGL3x textureUnit = new TextureUnitGL3x(i, this);
                _textureUnits[i] = textureUnit;
            }
            _dirtyTextureUnits = new List<ICleanable>();
            _lastTextureUnit = _textureUnits[textureUnits - 1] as TextureUnitGL3x;
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
            for (int i = 0; i < _dirtyTextureUnits.Count; ++i)
            {
                _dirtyTextureUnits[i].Clean();
            }
            _dirtyTextureUnits.Clear();
            _lastTextureUnit.CleanLastTextureUnit();
        }

        #region ICleanableObserver Members

        public void NotifyDirty(ICleanable value)
        {
            _dirtyTextureUnits.Add(value);
        }

        #endregion

        private TextureUnit[] _textureUnits;
        private IList<ICleanable> _dirtyTextureUnits;
        private TextureUnitGL3x _lastTextureUnit;
    }
}
