#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using OpenGlobe.Core;

namespace OpenGlobe.Renderer
{
    public abstract class TextureSampler : Disposable
    {
        protected TextureSampler(
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

        private readonly TextureMinificationFilter _minificationFilter;
        private readonly TextureMagnificationFilter _magnificationFilter;
        private readonly TextureWrap _wrapS;
        private readonly TextureWrap _wrapT;
        private readonly float _maximumAnisotropic;
    }
}
