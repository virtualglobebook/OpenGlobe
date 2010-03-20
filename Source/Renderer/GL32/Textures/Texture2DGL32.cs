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
using MiniGlobe.Core;
using MiniGlobe.Renderer;
using OpenTK.Graphics.OpenGL;

namespace MiniGlobe.Renderer.GL32
{
    internal class Texture2DGL32 : Texture2D
    {
        public Texture2DGL32(Texture2DDescription description)
        {
            Debug.Assert(description.Width > 0);
            Debug.Assert(description.Height > 0);

            int textureUnits;
            GL.GetInteger(GetPName.MaxCombinedTextureImageUnits, out textureUnits);

            _handle = GL.GenTexture();
            _description = description;
            _lastTextureUnit = OpenTK.Graphics.OpenGL.TextureUnit.Texture0 + (textureUnits - 1);

            //
            // TexImage2D is just used to allocate the texture so a PBO can't be bound.
            //
            WritePixelBufferGL32.UnBind();
            BindToLastTextureUnit();
            GL.TexImage2D(TextureTarget.Texture2D, 0,
                TypeConverterGL32.To(description.Format),
                description.Width,
                description.Height,
                0,
                TypeConverterGL32.TextureToPixelFormat(description.Format),   
                TypeConverterGL32.TextureToPixelType(description.Format),
                new IntPtr());

            //
            // Default filter, compatiable when attaching a non-mimapped 
            // texture to a frame buffer object.
            //
            _filter = Texture2DFilter.LinearClampToEdge;

            ApplyFilter();
        }

        internal int Handle
        {
            get { return _handle; }
        }

        internal void Bind()
        {
            // TODO: avoid duplicate binds
            GL.BindTexture(TextureTarget.Texture2D, _handle);
        }

        private void BindToLastTextureUnit()
        {
            GL.ActiveTexture(_lastTextureUnit);
            Bind();
        }

        internal static void UnBind()
        {
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        #region Texture2D Members

        public override void CopyFromBuffer(
            WritePixelBuffer pixelBuffer,
            int xOffset,
            int yOffset,
            int width,
            int height,
            ImageFormat format,
            ImageDataType dataType)
        {
            Debug.Assert(xOffset >= 0);
            Debug.Assert(yOffset >= 0);
            Debug.Assert(xOffset + width <= _description.Width);
            Debug.Assert(yOffset + height <= _description.Height);
            Debug.Assert(pixelBuffer.SizeInBytes >= TextureUtility.RequiredSizeInBytes(width, height, format, dataType));

            WritePixelBufferGL32 bufferObjectGL = pixelBuffer as WritePixelBufferGL32;

            bufferObjectGL.Bind();
            BindToLastTextureUnit();
            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
            GL.TexSubImage2D(TextureTarget.Texture2D, 0,
                xOffset,
                xOffset,
                width,
                height,
                TypeConverterGL32.To(format),
                TypeConverterGL32.To(dataType),
                new IntPtr());
            GenerateMipmaps();
        }

        public override ReadPixelBuffer CopyToBuffer(ImageFormat format, ImageDataType dataType)
        {
            ReadPixelBufferGL32 pixelBuffer = new ReadPixelBufferGL32(ReadPixelBufferHint.StreamRead,
                TextureUtility.RequiredSizeInBytes(_description.Width, _description.Height, format, dataType));

            pixelBuffer.Bind();
            BindToLastTextureUnit();
            GL.PixelStore(PixelStoreParameter.PackAlignment, 1);
            GL.GetTexImage(TextureTarget.Texture2D, 0,
                TypeConverterGL32.To(format),
                TypeConverterGL32.To(dataType),
                new IntPtr());

            return pixelBuffer;
        }

        public override Texture2DDescription Description
        {
            get { return _description; }
        }

        public override Texture2DFilter Filter
        {
            get { return _filter; }
            set 
            {
                if (_filter != value)
                {
                    _filter = value;
                    ApplyFilter();
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

        private void ApplyFilter()
        {
            TextureMinFilter minFilter = TypeConverterGL32.To(_filter.MinificationFilter);
            TextureMagFilter magFilter = TypeConverterGL32.To(_filter.MagnificationFilter);
            TextureWrapMode wrapS = TypeConverterGL32.To(_filter.WrapS);
            TextureWrapMode wrapT = TypeConverterGL32.To(_filter.WrapT);

            BindToLastTextureUnit();
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)minFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)magFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)wrapS);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)wrapT);
            ApplyAnisotropicFilter();
        }

        private void ApplyAnisotropicFilter()
        {
            if (Device.Extensions.AnisotropicFiltering)
            {
                GL.TexParameter(TextureTarget.Texture2D,
                    (TextureParameterName)ExtTextureFilterAnisotropic.TextureMaxAnisotropyExt,
                    _filter.MaximumAnisotropic);
            }
            else
            {
                if (_filter.MaximumAnisotropic != 1)
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
                GL.DeleteTexture(_handle);
            }
            base.Dispose(disposing);
        }

        #endregion

        private readonly int _handle;
        private readonly Texture2DDescription _description;
        private readonly OpenTK.Graphics.OpenGL.TextureUnit _lastTextureUnit;
        private Texture2DFilter _filter;
    }
}
