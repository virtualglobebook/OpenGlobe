#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using OpenGlobe.Renderer;
using OpenTK.Graphics.OpenGL;

namespace OpenGlobe.Renderer.GL3x
{
    internal class TextureUnitGL3x : TextureUnit, ICleanable
    {
        public TextureUnitGL3x(int index, ICleanableObserver observer)
        {
            _textureUnit = OpenTK.Graphics.OpenGL.TextureUnit.Texture0 + index;
            _observer = observer;
        }

        #region TextureUnit Members

        public override Texture2D Texture2D
        {
            get { return _texture2D; }

            set 
            {
                Texture2DGL3x texture = value as Texture2DGL3x;

                if (texture.Target != TextureTarget.Texture2D)
                {
                    throw new ArgumentException("Incompatible texture.  Did you create the texture with Device.CreateTexture2D?");
                }

                if ((_dirtyFlags == DirtyFlags.None) && (_texture2D != texture))
                {
                    _dirtyFlags |= DirtyFlags.Texture2D;
                    _observer.NotifyDirty(this);
                }

                _texture2D = texture;
            }
        }

        public override Texture2D Texture2DRectangle
        {
            get { return _texture2DRectangle; }

            set
            {
                Texture2DGL3x texture = value as Texture2DGL3x;

                if (texture.Target != TextureTarget.TextureRectangle)
                {
                    throw new ArgumentException("Incompatible texture.  Did you create the texture with Device.CreateTexture2DRectangle?");
                }

                if ((_dirtyFlags == DirtyFlags.None) && (_texture2DRectangle != texture))
                {
                    _dirtyFlags |= DirtyFlags.Texture2DRectangle;
                    _observer.NotifyDirty(this);
                }

                _texture2DRectangle = texture;
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
            if ((_texture2D != null) || (_texture2DRectangle != null))
            {
                _dirtyFlags = DirtyFlags.All;
            }

            Clean();
        }

        #region ICleanable Members

        public void Clean()
        {
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
	                    Texture2DGL3x.UnBind(TextureTarget.Texture2D);
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
	                    Texture2DGL3x.UnBind(TextureTarget.TextureRectangle);
	                }
	            }
	
	            _dirtyFlags = DirtyFlags.None;
            }
        }

        #endregion

        [Flags]
        private enum DirtyFlags
        {
            None = 0,
            Texture2D = 1,
            Texture2DRectangle = 2,
            All = Texture2D | Texture2DRectangle
        }

        private readonly OpenTK.Graphics.OpenGL.TextureUnit _textureUnit;
        private readonly ICleanableObserver _observer;
        private Texture2DGL3x _texture2D;
        private Texture2DGL3x _texture2DRectangle;
        private DirtyFlags _dirtyFlags;
    }
}
