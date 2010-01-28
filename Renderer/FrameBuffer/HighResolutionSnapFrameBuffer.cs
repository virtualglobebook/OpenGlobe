#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;

namespace MiniGlobe.Renderer
{
    public sealed class HighResolutionSnapFrameBuffer : IDisposable
    {
        public HighResolutionSnapFrameBuffer(Context context, double widthInInches, int dotsPerInch, double aspectRatio)
        {
            _widthInInches = widthInInches;
            _dotsPerInch = dotsPerInch;
            _aspectRatio = aspectRatio;

            Texture2DDescription colorDescription = new Texture2DDescription(WidthInPixels, HeightInPixels, TextureFormat.RedGreenBlue8, false);
            _colorTexture = Device.CreateTexture2D(colorDescription);

            Texture2DDescription depthDescription = new Texture2DDescription(WidthInPixels, HeightInPixels, TextureFormat.Depth24, false);
            _depthTexture = Device.CreateTexture2D(depthDescription);

            _frameBuffer = context.CreateFrameBuffer();
            _frameBuffer.ColorAttachments[0] = _colorTexture;
            _frameBuffer.DepthAttachment = _depthTexture;
        }

        public double WidthInInches
        {
            get { return _widthInInches; }
        }

        public double HeightInInches
        {
            get { return _widthInInches * (1.0 / AspectRatio); }
        }

        public int WidthInPixels
        {
            get { return (int)(WidthInInches * DotsPerInch); }
        }

        public int HeightInPixels
        {
            get { return (int)(HeightInInches * DotsPerInch); }
        }

        public int DotsPerInch
        {
            get { return _dotsPerInch; }
        }

        public double AspectRatio
        {
            get { return _aspectRatio; }
        }

        public FrameBuffer FrameBuffer
        {
            get { return _frameBuffer; }
        }

        public void SaveColorBuffer(string filename)
        {
            _frameBuffer.ColorAttachments[0].Save(filename);
        }

        public void SaveDepthBuffer(string filename)
        {
            _frameBuffer.DepthAttachment.Save(filename);
        }

        #region IDisposable Members

        public void Dispose()
        {
            _colorTexture.Dispose();
            _depthTexture.Dispose();
            _frameBuffer.Dispose();
        }

        #endregion

        private readonly double _widthInInches;
        private readonly int _dotsPerInch;
        private readonly double _aspectRatio;

        Texture2D _colorTexture;
        Texture2D _depthTexture;
        FrameBuffer _frameBuffer;
    }
}