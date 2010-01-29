#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using MiniGlobe.Renderer;
using OpenTK.Graphics.OpenGL;

namespace MiniGlobe.Renderer.GL32
{
    internal class TextureUnitGL32 : TextureUnit
    {
        public TextureUnitGL32(int index, bool lastTextureUnit)
        {
            _textureUnit = OpenTK.Graphics.OpenGL.TextureUnit.Texture0 + index;
            _lastTextureUnit = lastTextureUnit;
        }

        #region TextureUnit Members

        public override Texture2D Texture2D
        {
            get { return _texture2D; }

            set 
            {
                Texture2DGL32 texture = value as Texture2DGL32;

                if (_texture2D != texture)
                {
                    _texture2D = texture;
                    _dirty = true;
                }
            }
        }

        #endregion

        internal void Clean()
        {
            //
            // If the last texture unit has a texture attached, it
            // is cleaned even if it isn't dirty because the last
            // texture unit is used for texture uploads and downloads in
            // Texture2DGL32, the texture unit could be dirty without
            // knowing it.
            //
            if (_dirty || (_lastTextureUnit && (_texture2D != null)))
            {
                GL.ActiveTexture(_textureUnit);

                if (_texture2D != null)
                {
                    _texture2D.Bind();
                }
                else
                {
                    Texture2DGL32.UnBind();
                }

                _dirty = false;
            }
        }

        private readonly OpenTK.Graphics.OpenGL.TextureUnit _textureUnit;
        private readonly bool _lastTextureUnit;
        private Texture2DGL32 _texture2D;
        private bool _dirty;
    }
}
