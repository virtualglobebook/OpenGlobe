#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Globalization;

namespace OpenGlobe.Renderer
{
    public class TextureSamplers
    {
        internal TextureSamplers ()
	    {
            _nearestClampToEdge = Device.CreateTexture2DSampler(
                    TextureMinificationFilter.Nearest,
                    TextureMagnificationFilter.Nearest,
                    TextureWrap.ClampToEdge,
                    TextureWrap.ClampToEdge);

            _linearClampToEdge = Device.CreateTexture2DSampler(
                    TextureMinificationFilter.Linear,
                    TextureMagnificationFilter.Linear,
                    TextureWrap.ClampToEdge,
                    TextureWrap.ClampToEdge);

            _nearestRepeat = Device.CreateTexture2DSampler(
                    TextureMinificationFilter.Nearest,
                    TextureMagnificationFilter.Nearest,
                    TextureWrap.Repeat,
                    TextureWrap.Repeat);

            _linearRepeat = Device.CreateTexture2DSampler(
                    TextureMinificationFilter.Linear,
                    TextureMagnificationFilter.Linear,
                    TextureWrap.Repeat,
                    TextureWrap.Repeat);
	    }

        public TextureSampler NearestClampToEdge
        {
            get { return _nearestClampToEdge;  }
        }
        
        public TextureSampler LinearClampToEdge
        {
            get { return _linearClampToEdge;  }
        }

        public TextureSampler NearestRepeat
        {
            get { return _nearestRepeat;  }
        }

        public TextureSampler LinearRepeat
        {
            get { return _linearRepeat;  }
        }

        private readonly TextureSampler _nearestClampToEdge;
        private readonly TextureSampler _linearClampToEdge;
        private readonly TextureSampler _nearestRepeat;
        private readonly TextureSampler _linearRepeat;
    }
}
