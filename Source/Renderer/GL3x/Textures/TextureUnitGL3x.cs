#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;
using OpenGlobe.Renderer;
using OpenTK.Graphics.OpenGL;
using OpenTKTextureUnit = OpenTK.Graphics.OpenGL.TextureUnit;

namespace OpenGlobe.Renderer.GL3x
{
    internal class TextureUnitGL3x : TextureUnit, ICleanable
    {
        public TextureUnitGL3x(int index, ICleanableObserver observer)
        {
            _textureUnitIndex = index;
            _textureUnit = OpenTKTextureUnit.Texture0 + index;
            _observer = observer;
        }

        #region TextureUnit Members

        public override Texture2D Texture
        {
            get { return _texture; }

            set 
            {
                Texture2DGL3x texture = (Texture2DGL3x)value;

                if (_texture != texture)
                {
                    if (_dirtyFlags == DirtyFlags.None)
                    {
                        _observer.NotifyDirty(this);
                    }

                    _dirtyFlags |= DirtyFlags.Texture;
                    _texture = texture;
                }
            }
        }

        public override TextureSampler TextureSampler
        {
            get { return _textureSampler; }

            set
            {
                TextureSamplerGL3x sampler = (TextureSamplerGL3x)value;

                if (_textureSampler != sampler)
                {
                    if (_dirtyFlags == DirtyFlags.None)
                    {
                        _observer.NotifyDirty(this);
                    }

                    _dirtyFlags |= DirtyFlags.TextureSampler;
                    _textureSampler = sampler;
                }
            }
        }

        #endregion

        internal void CleanLastTextureUnit()
        {
            //
            // If the last texture unit has a texture attached, it
            // is cleaned even if it isn't dirty because the last
            // texture unit is used for texture uploads and downloads in
            // Texture2DGL3x, the texture unit could be dirty without
            // knowing it.
            //
            if (_texture != null)
            {
                _dirtyFlags |= DirtyFlags.Texture;
            }

            Clean();
        }

        #region ICleanable Members

        public void Clean()
        {
            if (_dirtyFlags != DirtyFlags.None)
            {
                Validate();

	            GL.ActiveTexture(_textureUnit);
	
	            if ((_dirtyFlags & DirtyFlags.Texture) == DirtyFlags.Texture)
	            {
	                if (_texture != null)
	                {
	                    _texture.Bind();
	                }
	                else
	                {
	                    Texture2DGL3x.UnBind(TextureTarget.Texture2D);
                        Texture2DGL3x.UnBind(TextureTarget.TextureRectangle);
	                }
	            }

                if ((_dirtyFlags & DirtyFlags.TextureSampler) == DirtyFlags.TextureSampler)
                {
                    if (_textureSampler != null)
                    {
                        _textureSampler.Bind(_textureUnitIndex);
                    }
                    else
                    {
                        TextureSamplerGL3x.UnBind(_textureUnitIndex);
                    }
                }

	            _dirtyFlags = DirtyFlags.None;
            }
        }

        #endregion

        private void Validate()
        {
            if (_texture != null)
            {
                if (_textureSampler == null)
                {
                    throw new InvalidOperationException("A texture sampler must be assigned to a texture unit with one or more bound textures.");
                }

                if (_texture.Target == TextureTarget.TextureRectangle)
                {
                    if (_textureSampler.MinificationFilter != TextureMinificationFilter.Linear &&
                        _textureSampler.MinificationFilter != TextureMinificationFilter.Nearest)
                    {
                        throw new InvalidOperationException("The texture sampler is incompatible with the rectangle texture bound to the same texture unit.  Rectangle textures only support linear and nearest minification filters.");
                    }

                    if (_textureSampler.WrapS == TextureWrap.Repeat ||
                        _textureSampler.WrapS == TextureWrap.MirroredRepeat ||
                        _textureSampler.WrapT == TextureWrap.Repeat ||
                        _textureSampler.WrapT == TextureWrap.MirroredRepeat)
                    {
                        throw new InvalidOperationException("The texture sampler is incompatible with the rectangle texture bound to the same texture unit.  Rectangle textures do not support repeat or mirrored repeat wrap modes.");
                    }
                }
            }
        }

        [Flags]
        private enum DirtyFlags
        {
            None = 0,
            Texture = 1,
            TextureSampler = 2,
            All = Texture | TextureSampler
        }

        private readonly int _textureUnitIndex;
        private readonly OpenTKTextureUnit _textureUnit;
        private readonly ICleanableObserver _observer;
        private Texture2DGL3x _texture;
        private TextureSamplerGL3x _textureSampler;
        private DirtyFlags _dirtyFlags;
    }
}
