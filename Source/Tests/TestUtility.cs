#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System.Drawing;
using NUnit.Framework;
using MiniGlobe.Renderer;

namespace MiniGlobe
{
    public static class TestUtility
    {
        /// <summary>
        /// Creates a frame buffer with a 1x1 RGB color attachment.
        /// </summary>
        public static FrameBuffer CreateFrameBuffer(Context context)
        {
            Texture2DDescription colorDescription = new Texture2DDescription(1, 1, TextureFormat.RedGreenBlue8, false);
            Texture2D colorTexture = Device.CreateTexture2D(colorDescription);
            FrameBuffer frameBuffer = context.CreateFrameBuffer();
            frameBuffer.ColorAttachments[0] = colorTexture;

            // TODO:  Why isn't this the default?
            // TODO:  Lower left or upper left?
            context.Viewport = new Rectangle(0, 0, 1, 1);

            return frameBuffer;
        }

        public static void ValidateColor(Texture2D colorTexture, byte red, byte green, byte blue)
        {
            using (ReadPixelBuffer readPixelBuffer = colorTexture.CopyToBuffer(ImageFormat.RedGreenBlue, ImageDataType.UnsignedByte, 1))
            {
                byte[] color = readPixelBuffer.CopyToSystemMemory<byte>();
                Assert.AreEqual(red, color[0]);
                Assert.AreEqual(green, color[1]);
                Assert.AreEqual(blue, color[2]);
            }
        }
    }
}
