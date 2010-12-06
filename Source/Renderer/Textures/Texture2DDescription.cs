#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Globalization;

namespace OpenGlobe.Renderer
{
    public struct Texture2DDescription : IEquatable<Texture2DDescription>
    {
        public Texture2DDescription(int width, int height, TextureFormat format)
            : this(width, height, format, false)
        {
        }

        public Texture2DDescription(
            int width, 
            int height, 
            TextureFormat format,
            bool generateMipmaps)
        {
            _width = width;
            _height = height;
            _format = format;
            _generateMipmaps = generateMipmaps;
        }

        public int Width
        {
            get { return _width; }
        }

        public int Height
        {
            get { return _height; }
        }

        public TextureFormat TextureFormat
        {
            get { return _format; }
        }

        public bool GenerateMipmaps
        {
            get { return _generateMipmaps; }
        }

        public bool ColorRenderable
        {
            get
            {
                return !DepthRenderable && !DepthStencilRenderable;
            }
        }

        public bool DepthRenderable
        {
            get
            {
                return
                    _format == TextureFormat.Depth16 || 
                    _format == TextureFormat.Depth24 || 
                    _format == TextureFormat.Depth32f || 
                    _format == TextureFormat.Depth24Stencil8 || 
                    _format == TextureFormat.Depth32fStencil8;
            }
        }

        public bool DepthStencilRenderable
        {
            get
            {
                return
                    _format == TextureFormat.Depth24Stencil8 || 
                    _format == TextureFormat.Depth32fStencil8;
            }
        }


        /// <summary>
        /// This is approximate because we don't know exactly how the driver stores the texture.
        /// </summary>
        public int ApproximateSizeInBytes
        {
            get
            {
                return _width * _height * SizeInBytes(_format);
            }
        }

        private static int SizeInBytes(TextureFormat format)
        {
            switch (format)
            {
                case TextureFormat.RedGreenBlue8:
                    return 3;
                case TextureFormat.RedGreenBlue16:
                    return 6;
                case TextureFormat.RedGreenBlueAlpha8:
                    return 4;
                case TextureFormat.RedGreenBlue10A2:
                    return 4;
                case TextureFormat.RedGreenBlueAlpha16:
                    return 8;
                case TextureFormat.Depth16:
                    return 2;
                case TextureFormat.Depth24:
                    return 3;
                case TextureFormat.Red8:
                    return 1;
                case TextureFormat.Red16:
                    return 2;
                case TextureFormat.RedGreen8:
                    return 2;
                case TextureFormat.RedGreen16:
                    return 4;
                case TextureFormat.Red16f:
                    return 2;
                case TextureFormat.Red32f:
                    return 4;
                case TextureFormat.RedGreen16f:
                    return 4;
                case TextureFormat.RedGreen32f:
                    return 8;
                case TextureFormat.Red8i:
                    return 1;
                case TextureFormat.Red8ui:
                    return 1;
                case TextureFormat.Red16i:
                    return 2;
                case TextureFormat.Red16ui:
                    return 2;
                case TextureFormat.Red32i:
                    return 4;
                case TextureFormat.Red32ui:
                    return 4;
                case TextureFormat.RedGreen8i:
                    return 2;
                case TextureFormat.RedGreen8ui:
                    return 2;
                case TextureFormat.RedGreen16i:
                    return 4;
                case TextureFormat.RedGreen16ui:
                    return 4;
                case TextureFormat.RedGreen32i:
                    return 8;
                case TextureFormat.RedGreen32ui:
                    return 8;
                case TextureFormat.RedGreenBlueAlpha32f:
                    return 16;
                case TextureFormat.RedGreenBlue32f:
                    return 12;
                case TextureFormat.RedGreenBlueAlpha16f:
                    return 8;
                case TextureFormat.RedGreenBlue16f:
                    return 6;
                case TextureFormat.Depth24Stencil8:
                    return 4;
                case TextureFormat.Red11fGreen11fBlue10f:
                    return 4;
                case TextureFormat.RedGreenBlue9E5:
                    return 4;
                case TextureFormat.SRedGreenBlue8:
                    return 3;
                case TextureFormat.SRedGreenBlue8Alpha8:
                    return 4;
                case TextureFormat.Depth32f:
                    return 4;
                case TextureFormat.Depth32fStencil8:
                    return 5;
                case TextureFormat.RedGreenBlueAlpha32ui:
                    return 16;
                case TextureFormat.RedGreenBlue32ui:
                    return 12;
                case TextureFormat.RedGreenBlueAlpha16ui:
                    return 8;
                case TextureFormat.RedGreenBlue16ui:
                    return 6;
                case TextureFormat.RedGreenBlueAlpha8ui:
                    return 4;
                case TextureFormat.RedGreenBlue8ui:
                    return 3;
                case TextureFormat.RedGreenBlueAlpha32i:
                    return 16;
                case TextureFormat.RedGreenBlue32i:
                    return 12;
                case TextureFormat.RedGreenBlueAlpha16i:
                    return 8;
                case TextureFormat.RedGreenBlue16i:
                    return 6;
                case TextureFormat.RedGreenBlueAlpha8i:
                    return 4;
                case TextureFormat.RedGreenBlue8i:
                    return 3;
            }

            throw new ArgumentException("format");
        }

        public static bool operator ==(Texture2DDescription left, Texture2DDescription right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Texture2DDescription left, Texture2DDescription right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "Width: {0} Height: {1} Format: {2} GenerateMipmaps: {3}",
                _width, _height, _format, _generateMipmaps);
        }

        public override int GetHashCode()
        {
            return
                _width.GetHashCode() ^
                _height.GetHashCode() ^
                _format.GetHashCode() ^
                _generateMipmaps.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Texture2DDescription))
                return false;

            return this.Equals((Texture2DDescription)obj);
        }

        #region IEquatable<Texture2DDescription> Members

        public bool Equals(Texture2DDescription other)
        {
            return
                (_width == other._width) &&
                (_height == other._height) &&
                (_format == other._format) &&
                (_generateMipmaps == other._generateMipmaps);
        }

        #endregion

        private readonly int _width;
        private readonly int _height;
        private readonly TextureFormat _format;
        private readonly bool _generateMipmaps;
    }
}
