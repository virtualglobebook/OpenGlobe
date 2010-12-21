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
    internal class Texture2DGL3x : Texture2D
    {
        public Texture2DGL3x(Texture2DDescription description, TextureTarget textureTarget)
        {
            if (description.Width <= 0)
            {
                throw new ArgumentOutOfRangeException("description.Width", "description.Width must be greater than zero.");
            }

            if (description.Height <= 0)
            {
                throw new ArgumentOutOfRangeException("description.Height", "description.Height must be greater than zero.");
            }

            if (description.GenerateMipmaps)
            {
                if (textureTarget == TextureTarget.TextureRectangle)
                {
                    throw new ArgumentException("description.GenerateMipmaps cannot be true for texture rectangles.", "description");
                }
                
                if (!TextureUtility.IsPowerOfTwo(Convert.ToUInt32(description.Width)))
                {
                    throw new ArgumentException("When description.GenerateMipmaps is true, the width must be a power of two.", "description");
                }

                if (!TextureUtility.IsPowerOfTwo(Convert.ToUInt32(description.Height)))
                {
                    throw new ArgumentException("When description.GenerateMipmaps is true, the height must be a power of two.", "description");
                }
            }
            
            _name = new TextureNameGL3x();
            _target = textureTarget;
            _description = description;
            _lastTextureUnit = OpenTKTextureUnit.Texture0 + (Device.NumberOfTextureUnits - 1);

            //
            // TexImage2D is just used to allocate the texture so a PBO can't be bound.
            //
            WritePixelBufferGL3x.UnBind();
            BindToLastTextureUnit();
            GL.TexImage2D(_target, 0,
                TypeConverterGL3x.To(description.TextureFormat),
                description.Width,
                description.Height,
                0,
                TypeConverterGL3x.TextureToPixelFormat(description.TextureFormat),   
                TypeConverterGL3x.TextureToPixelType(description.TextureFormat),
                new IntPtr());

            //
            // Default sampler, compatiable when attaching a non-mimapped 
            // texture to a frame buffer object.
            //
            ApplySampler(Device.TextureSamplers.LinearClamp);

            GC.AddMemoryPressure(description.ApproximateSizeInBytes);
        }

        internal TextureNameGL3x Handle
        {
            get { return _name; }
        }

        internal TextureTarget Target
        {
            get { return _target; }
        }

        internal void Bind()
        {
            GL.BindTexture(_target, _name.Value);
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
            if (pixelBuffer.SizeInBytes < TextureUtility.RequiredSizeInBytes(
                width, height, format, dataType, rowAlignment))
            {
                throw new ArgumentException("Pixel buffer is not big enough for provided width, height, format, and datatype.");
            }

            VerifyOffsets(xOffset, yOffset, width, height);
            VerifyRowAlignment(rowAlignment);

            WritePixelBufferGL3x bufferObjectGL = (WritePixelBufferGL3x)pixelBuffer;

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

        public override void CopyFromFramebuffer(
            int xOffset,
            int yOffset,
            int framebufferXOffset,
            int framebufferYOffset,
            int width,
            int height)
        {
            VerifyOffsets(xOffset, yOffset, width, height);

            BindToLastTextureUnit();
            GL.ReadBuffer(ReadBufferMode.Back);                     // Default.  Not really needed here.
            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 4);  // Don't think this is needed either.
            GL.CopyTexSubImage2D(_target, 0,
                xOffset,
                yOffset,
                framebufferXOffset,
                framebufferYOffset,
                width,
                height);

            GenerateMipmaps();
        }

        public override ReadPixelBuffer CopyToBuffer(
            ImageFormat format, 
            ImageDatatype dataType,
            int rowAlignment)
        {
            if (format == ImageFormat.StencilIndex)
            {
                throw new ArgumentException("StencilIndex is not supported by CopyToBuffer.  Try DepthStencil instead.", "format");
            }

            VerifyRowAlignment(rowAlignment);

            ReadPixelBufferGL3x pixelBuffer = new ReadPixelBufferGL3x(PixelBufferHint.Stream,
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

        private void VerifyOffsets(
            int xOffset,
            int yOffset,
            int width,
            int height)
        {
            if (xOffset < 0)
            {
                throw new ArgumentOutOfRangeException("xOffset", "xOffset must be greater than or equal to zero.");
            }

            if (yOffset < 0)
            {
                throw new ArgumentOutOfRangeException("yOffset", "yOffset must be greater than or equal to zero.");
            }

            if (xOffset + width > _description.Width)
            {
                throw new ArgumentOutOfRangeException("xOffset + width must be less than or equal to Description.Width");
            }

            if (yOffset + height > _description.Height)
            {
                throw new ArgumentOutOfRangeException("yOffset + height must be less than or equal to Description.Height");
            }
        }

        private void VerifyRowAlignment(int rowAlignment)
        {
            if ((rowAlignment != 1) && 
                (rowAlignment != 2) && 
                (rowAlignment != 4) && 
                (rowAlignment != 8))
            {
                throw new ArgumentException("rowAlignment");
            }
        }

        public override Texture2DDescription Description
        {
            get { return _description; }
        }

        #endregion

        private void GenerateMipmaps()
        {
            if (_description.GenerateMipmaps)
            {
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            }
        }

        private void ApplySampler(TextureSampler sampler)
        {
            TextureMinFilter minFilter = TypeConverterGL3x.To(sampler.MinificationFilter);
            TextureMagFilter magFilter = TypeConverterGL3x.To(sampler.MagnificationFilter);
            TextureWrapMode wrapS = TypeConverterGL3x.To(sampler.WrapS);
            TextureWrapMode wrapT = TypeConverterGL3x.To(sampler.WrapT);

            GL.TexParameter(_target, TextureParameterName.TextureMinFilter, (int)minFilter);
            GL.TexParameter(_target, TextureParameterName.TextureMagFilter, (int)magFilter);
            GL.TexParameter(_target, TextureParameterName.TextureWrapS, (int)wrapS);
            GL.TexParameter(_target, TextureParameterName.TextureWrapT, (int)wrapT);
        }

        #region Disposable Members

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _name.Dispose();
                GC.RemoveMemoryPressure(_description.ApproximateSizeInBytes);
            }
            base.Dispose(disposing);
        }

        #endregion

        private readonly TextureNameGL3x _name;
        private readonly TextureTarget _target;
        private readonly Texture2DDescription _description;
        private readonly OpenTKTextureUnit _lastTextureUnit;
    }
}
