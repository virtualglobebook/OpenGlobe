#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
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
            //
            // Device.NumberOfTextureUnits is not initialized yet.
            //
            int numberOfTextureUnits;
            GL.GetInteger(GetPName.MaxCombinedTextureImageUnits, out numberOfTextureUnits);

            _textureUnits = new TextureUnit[numberOfTextureUnits];
            for (int i = 0; i < numberOfTextureUnits; ++i)
            {
                TextureUnitGL3x textureUnit = new TextureUnitGL3x(i, this);
                _textureUnits[i] = textureUnit;
            }
            _dirtyTextureUnits = new List<ICleanable>();
            _lastTextureUnit = (TextureUnitGL3x)_textureUnits[numberOfTextureUnits - 1];
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
