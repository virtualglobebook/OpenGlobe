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
    public enum TextureMinificationFilter
    {
        Nearest,
        Linear,
        NearestMipmapNearest,
        LinearMipmapNearest,
        NearestMipmapLinear,
        LinearMipmapLinear,
    }

    public enum TextureMagnificationFilter
    {
        Nearest,
        Linear
    }

    public enum TextureWrap
    {
        ClampToEdge,
        Repeat,
        MirroredRepeat
    }

    public struct Texture2DSampler : IEquatable<Texture2DSampler>
    {
        public static readonly Texture2DSampler NearestClampToEdge =
            new Texture2DSampler(
                TextureMinificationFilter.Nearest,
                TextureMagnificationFilter.Nearest,
                TextureWrap.ClampToEdge,
                TextureWrap.ClampToEdge);

        public static readonly Texture2DSampler LinearClampToEdge =
            new Texture2DSampler(
                TextureMinificationFilter.Linear,
                TextureMagnificationFilter.Linear,
                TextureWrap.ClampToEdge,
                TextureWrap.ClampToEdge);

        public static readonly Texture2DSampler NearestRepeat =
            new Texture2DSampler(
                TextureMinificationFilter.Nearest,
                TextureMagnificationFilter.Nearest,
                TextureWrap.Repeat,
                TextureWrap.Repeat);

        public static readonly Texture2DSampler LinearRepeat =
            new Texture2DSampler(
                TextureMinificationFilter.Linear,
                TextureMagnificationFilter.Linear,
                TextureWrap.Repeat,
                TextureWrap.Repeat);

        public Texture2DSampler(
            TextureMinificationFilter minificationFilter,
            TextureMagnificationFilter magnificationFilter,
            TextureWrap wrapS,
            TextureWrap wrapT)
        {
            _minificationFilter = minificationFilter;
            _magnificationFilter = magnificationFilter;
            _wrapS = wrapS;
            _wrapT = wrapT;
            _maximumAnisotropic = 1;
        }

        public Texture2DSampler(
            TextureMinificationFilter minificationFilter,
            TextureMagnificationFilter magnificationFilter,
            TextureWrap wrapS,
            TextureWrap wrapT,
            float maximumAnisotropic)
        {
            _minificationFilter = minificationFilter;
            _magnificationFilter = magnificationFilter;
            _wrapS = wrapS;
            _wrapT = wrapT;
            _maximumAnisotropic = maximumAnisotropic;
        }

        public TextureMinificationFilter MinificationFilter
        {
            get { return _minificationFilter; }
        }

        public TextureMagnificationFilter MagnificationFilter
        {
            get { return _magnificationFilter; }
        }

        public TextureWrap WrapS
        {
            get { return _wrapS; }
        }

        public TextureWrap WrapT
        {
            get { return _wrapT; }
        }

        public float MaximumAnisotropic
        {
            get { return _maximumAnisotropic; }
        }

        public static bool operator ==(Texture2DSampler left, Texture2DSampler right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Texture2DSampler left, Texture2DSampler right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "Minification Filter: {0} Magnification Filter: {1} WrapS: {2} WrapT: {3} Maximum Anisotropic: {4}",
                _minificationFilter, _magnificationFilter, _wrapS, _wrapT, _maximumAnisotropic);
        }

        public override int GetHashCode()
        {
            return
                _minificationFilter.GetHashCode() ^
                _magnificationFilter.GetHashCode() ^
                _wrapS.GetHashCode() ^
                _wrapT.GetHashCode() ^
                _maximumAnisotropic.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Texture2DSampler))
                return false;

            return this.Equals((Texture2DSampler)obj);
        }

        #region IEquatable<BlittableRGBA> Members

        public bool Equals(Texture2DSampler other)
        {
            return
                (_minificationFilter == other._minificationFilter) &&
                (_magnificationFilter == other._magnificationFilter) &&
                (_wrapS == other._wrapS) &&
                (_wrapT == other._wrapT) &&
                (_maximumAnisotropic == other._maximumAnisotropic);
        }

        #endregion

        private readonly TextureMinificationFilter _minificationFilter;
        private readonly TextureMagnificationFilter _magnificationFilter;
        private readonly TextureWrap _wrapS;
        private readonly TextureWrap _wrapT;
        private readonly float _maximumAnisotropic;
    }
}
