#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
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

                if (texture.Target != TextureTarget.Texture2D)
                {
                    throw new ArgumentException("Texture2D", "Incompatible texture.  Did you create the texture with Device.CreateTexture2D?");
                }

                if (_texture2D != texture)
                {
                    _texture2D = texture;
                    _dirtyFlags |= DirtyFlags.Texture2D;
                }
            }
        }

        public override Texture2D Texture2DRectangle
        {
            get { return _texture2DRectangle; }

            set
            {
                Texture2DGL32 texture = value as Texture2DGL32;

                if (texture.Target != TextureTarget.TextureRectangle)
                {
                    throw new ArgumentException("Texture2D", "Incompatible texture.  Did you create the texture with Device.CreateTexture2DRectangle?");
                }

                if (_texture2DRectangle != texture)
                {
                    _texture2DRectangle = texture;
                    _dirtyFlags |= DirtyFlags.Texture2DRectangle;
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
            if ((_lastTextureUnit) && ((_texture2D != null) || (_texture2DRectangle != null)))
            {
                _dirtyFlags = DirtyFlags.All;
            }

            if (_dirtyFlags != DirtyFlags.None)
            {
                GL.ActiveTexture(_textureUnit);

                if ((_dirtyFlags & DirtyFlags.Texture2D) == DirtyFlags.Texture2D)
                {
                    if (_texture2D != null)
                    {
                        _texture2D.Bind();
                    }
                    else
                    {
                        Texture2DGL32.UnBind(TextureTarget.Texture2D);
                    }
                }

                if ((_dirtyFlags & DirtyFlags.Texture2DRectangle) == DirtyFlags.Texture2DRectangle)
                {
                    if (_texture2DRectangle != null)
                    {
                        _texture2DRectangle.Bind();
                    }
                    else
                    {
                        Texture2DGL32.UnBind(TextureTarget.TextureRectangle);
                    }
                }

                _dirtyFlags = DirtyFlags.None;
            }
        }

        [Flags]
        private enum DirtyFlags
        {
            None = 0,
            Texture2D = 1,
            Texture2DRectangle = 2,
            All = Texture2D | Texture2DRectangle
        }

        private readonly OpenTK.Graphics.OpenGL.TextureUnit _textureUnit;
        private readonly bool _lastTextureUnit;
        private Texture2DGL32 _texture2D;
        private Texture2DGL32 _texture2DRectangle;
        private DirtyFlags _dirtyFlags;
    }
}
