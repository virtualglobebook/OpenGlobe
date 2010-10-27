#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using OpenTK.Graphics.OpenGL;

namespace OpenGlobe.Renderer.GL3x
{
    internal class TextureSamplerGL3x : TextureSampler
    {
        public TextureSamplerGL3x(
            TextureMinificationFilter minificationFilter,
            TextureMagnificationFilter magnificationFilter,
            TextureWrap wrapS,
            TextureWrap wrapT,
            float maximumAnisotropic)
            : base(
                minificationFilter, 
                magnificationFilter, 
                wrapS, 
                wrapT, 
                maximumAnisotropic)
        {
            _handle = new SamplerHandleGL3x();

            int glMinificationFilter = (int)TypeConverterGL3x.To(minificationFilter);
            int glMagnificationFilter = (int)TypeConverterGL3x.To(magnificationFilter);
            int glWrapS = (int)TypeConverterGL3x.To(wrapS);
            int glWrapT = (int)TypeConverterGL3x.To(wrapT);

            GL.SamplerParameterI(_handle.Value, (ArbSamplerObjects)All.TextureMinFilter, ref glMinificationFilter);
            GL.SamplerParameterI(_handle.Value, (ArbSamplerObjects)All.TextureMagFilter, ref glMagnificationFilter);
            GL.SamplerParameterI(_handle.Value, (ArbSamplerObjects)All.TextureWrapS, ref glWrapS);
            GL.SamplerParameterI(_handle.Value, (ArbSamplerObjects)All.TextureWrapT, ref glWrapT);

            if (Device.Extensions.AnisotropicFiltering)
            {
                GL.SamplerParameter(_handle.Value, (ArbSamplerObjects)All.TextureMaxAnisotropyExt, maximumAnisotropic);
            }
            else
            {
                if (maximumAnisotropic != 1)
                {
                    throw new InsufficientVideoCardException("Anisotropic filtering is not supported.  The extension GL_EXT_texture_filter_anisotropic was not found.");
                }
            }
        }

        internal void Bind(int textureUnitIndex)
        {
            GL.BindSampler(textureUnitIndex, _handle.Value);
        }

        internal static void UnBind(int textureUnitIndex)
        {
            GL.BindSampler(textureUnitIndex, 0);
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

        private readonly SamplerHandleGL3x _handle;
    }
}