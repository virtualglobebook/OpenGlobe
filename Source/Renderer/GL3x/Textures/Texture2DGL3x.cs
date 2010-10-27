#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Diagnostics;
using OpenGlobe.Renderer;
using OpenTK.Graphics.OpenGL;

namespace OpenGlobe.Renderer.GL3x
{
    internal class Texture2DGL3x : Texture2D
    {
        public Texture2DGL3x(Texture2DDescription description, TextureTarget textureTarget)
        {
            Debug.Assert(description.Width > 0);
            Debug.Assert(description.Height > 0);

            if (description.GenerateMipmaps && (textureTarget == TextureTarget.TextureRectangle))
            {
                throw new ArgumentException("description.GenerateMipmaps cannot be true for texture rectangles.", "description");
            }

            int textureUnits;
            GL.GetInteger(GetPName.MaxCombinedTextureImageUnits, out textureUnits);

            _handle = new TextureHandleGL3x();
            _target = textureTarget;
            _description = description;
            _lastTextureUnit = OpenTK.Graphics.OpenGL.TextureUnit.Texture0 + (textureUnits - 1);

            //
            // TexImage2D is just used to allocate the texture so a PBO can't be bound.
            //
            WritePixelBufferGL3x.UnBind();
            BindToLastTextureUnit();
            GL.TexImage2D(_target, 0,
                TypeConverterGL3x.To(description.Format),
                description.Width,
                description.Height,
                0,
                TypeConverterGL3x.TextureToPixelFormat(description.Format),   
                TypeConverterGL3x.TextureToPixelType(description.Format),
                new IntPtr());

            //
            // Default filter, compatiable when attaching a non-mimapped 
            // texture to a frame buffer object.
            //
            _sampler = Texture2DSampler.LinearClampToEdge;

            ApplySampler();
        }

        internal TextureHandleGL3x Handle
        {
            get { return _handle; }
        }

        internal TextureTarget Target
        {
            get { return _target; }
        }

        internal void Bind()
        {
            GL.BindTexture(_target, _handle.Value);
        }

        private void BindToLastTextureUnit()
        {
            GL.ActiveTexture(_lastTextureUnit);
            Bind();
        }

        internal static void UnBind(TextureTarget textureTarget)
        {
            GL.BindTexture(textureTarget, 0);
        }

        #region Texture2D Members

        public override void CopyFromBuffer(
            WritePixelBuffer pixelBuffer,
            int xOffset,
            int yOffset,
            int width,
            int height,
            ImageFormat format,
            ImageDatatype dataType,
            int rowAlignment)
        {
            Debug.Assert(xOffset >= 0);
            Debug.Assert(yOffset >= 0);
            Debug.Assert(xOffset + width <= _description.Width);
            Debug.Assert(yOffset + height <= _description.Height);
            Debug.Assert(pixelBuffer.SizeInBytes >= TextureUtility.RequiredSizeInBytes(width, height, format, dataType, rowAlignment));
            Debug.Assert((rowAlignment == 1) || (rowAlignment == 2) || (rowAlignment == 4) || (rowAlignment == 8));

            WritePixelBufferGL3x bufferObjectGL = pixelBuffer as WritePixelBufferGL3x;

            bufferObjectGL.Bind();
            BindToLastTextureUnit();
            GL.PixelStore(PixelStoreParameter.UnpackAlignment, rowAlignment);
            GL.TexSubImage2D(_target, 0,
                xOffset,
                yOffset,
                width,
                height,
                TypeConverterGL3x.To(format),
                TypeConverterGL3x.To(dataType),
                new IntPtr());

            GenerateMipmaps();
        }

        public override ReadPixelBuffer CopyToBuffer(
            ImageFormat format, 
            ImageDatatype dataType,
            int rowAlignment)
        {
            Debug.Assert((rowAlignment == 1) || (rowAlignment == 2) || (rowAlignment == 4) || (rowAlignment == 8));

            ReadPixelBufferGL3x pixelBuffer = new ReadPixelBufferGL3x(ReadPixelBufferHint.StreamRead,
                TextureUtility.RequiredSizeInBytes(_description.Width, _description.Height, format, dataType, rowAlignment));

            pixelBuffer.Bind();
            BindToLastTextureUnit();
            GL.PixelStore(PixelStoreParameter.PackAlignment, rowAlignment);
            GL.GetTexImage(_target, 0,
                TypeConverterGL3x.To(format),
                TypeConverterGL3x.To(dataType),
                new IntPtr());

            return pixelBuffer;
        }

        public override Texture2DDescription Description
        {
            get { return _description; }
        }

        public override Texture2DSampler Sampler
        {
            get { return _sampler; }
            set 
            {
                if (_sampler != value)
                {
                    _sampler = value;
                    ApplySampler();
                }
            }
        }

        #endregion

        private void GenerateMipmaps()
        {
            if (_description.GenerateMipmaps)
            {
                Debug.Assert(TextureUtility.IsPowerOfTwo(Convert.ToUInt32(_description.Width)));
                Debug.Assert(TextureUtility.IsPowerOfTwo(Convert.ToUInt32(_description.Height)));
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            }
        }

        private void ApplySampler()
        {
            if (_target == TextureTarget.TextureRectangle)
            {
                if (_sampler.MinificationFilter != TextureMinificationFilter.Linear &&
                    _sampler.MinificationFilter != TextureMinificationFilter.Nearest)
                {
                    throw new ArgumentException("Rectangle textures only support linear and nearest minification filters.");
                }

                if (_sampler.WrapS == TextureWrap.Repeat ||
                    _sampler.WrapS == TextureWrap.MirroredRepeat ||
                    _sampler.WrapT == TextureWrap.Repeat ||
                    _sampler.WrapT == TextureWrap.MirroredRepeat)
                {
                    throw new ArgumentException("Rectangle textures do not support repeat and mirrored repeat wrap modes.");
                }
            }

            TextureMinFilter minFilter = TypeConverterGL3x.To(_sampler.MinificationFilter);
            TextureMagFilter magFilter = TypeConverterGL3x.To(_sampler.MagnificationFilter);
            TextureWrapMode wrapS = TypeConverterGL3x.To(_sampler.WrapS);
            TextureWrapMode wrapT = TypeConverterGL3x.To(_sampler.WrapT);

            BindToLastTextureUnit();
            GL.TexParameter(_target, TextureParameterName.TextureMinFilter, (int)minFilter);
            GL.TexParameter(_target, TextureParameterName.TextureMagFilter, (int)magFilter);
            GL.TexParameter(_target, TextureParameterName.TextureWrapS, (int)wrapS);
            GL.TexParameter(_target, TextureParameterName.TextureWrapT, (int)wrapT);
            ApplyAnisotropicFilter();
        }

        private void ApplyAnisotropicFilter()
        {
            if (Device.Extensions.AnisotropicFiltering)
            {
                GL.TexParameter(_target,
                    (TextureParameterName)ExtTextureFilterAnisotropic.TextureMaxAnisotropyExt,
                    _sampler.MaximumAnisotropic);
            }
            else
            {
                if (_sampler.MaximumAnisotropic != 1)
                {
                    throw new InsufficientVideoCardException("Anisotropic filtering is not supported.  The extension GL_EXT_texture_filter_anisotropic was not found.");
                }
            }
        }

        #region Disposable Members

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _handle.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion

        private readonly TextureHandleGL3x _handle;
        private readonly TextureTarget _target;
        private readonly Texture2DDescription _description;
        private readonly OpenTK.Graphics.OpenGL.TextureUnit _lastTextureUnit;
        private Texture2DSampler _sampler;
    }
}
