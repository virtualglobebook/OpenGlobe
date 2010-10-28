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
