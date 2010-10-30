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
using OpenGlobe.Core;
using OpenGlobe.Renderer;

namespace OpenGlobe
{
    public static class TestUtility
    {
        /// <summary>
        /// Creates a frame buffer with a 1x1 RGB color attachment.
        /// </summary>
        public static FrameBuffer CreateFrameBuffer(Context context)
        {
            FrameBuffer frameBuffer = context.CreateFrameBuffer();
            frameBuffer.ColorAttachments[0] = Device.CreateTexture2D(
                new Texture2DDescription(1, 1, TextureFormat.RedGreenBlue8, false));
            frameBuffer.DepthAttachment = Device.CreateTexture2D(
                new Texture2DDescription(1, 1, TextureFormat.Depth24, false));

            return frameBuffer;
        }

        /// <summary>
        /// Creates a 1x1 RGBA8 texture
        /// </summary>
        public static Texture2D CreateTexture(BlittableRGBA rgba)
        {
            Texture2DDescription description = new Texture2DDescription(1, 1, TextureFormat.RedGreenBlueAlpha8, false);
            Texture2D texture = Device.CreateTexture2D(description);

            BlittableRGBA[] pixels = new BlittableRGBA[] { rgba };

            int sizeInBytes = pixels.Length * SizeInBytes<BlittableRGBA>.Value;
            using (WritePixelBuffer writePixelBuffer = Device.CreateWritePixelBuffer(PixelBufferHint.Stream, sizeInBytes))
            {
                writePixelBuffer.CopyFromSystemMemory(pixels);
                texture.CopyFromBuffer(writePixelBuffer, ImageFormat.RedGreenBlueAlpha, ImageDatatype.UnsignedByte);
            }

            return texture;
        }

        public static VertexArray CreateVertexArray(Context context, int positionLocation)
        {
            Vector4S[] positions = new[] { new Vector4S(0, 0, 0, 1) };
            VertexBuffer positionsBuffer = Device.CreateVertexBuffer(BufferHint.StaticDraw, positions.Length * SizeInBytes<Vector4S>.Value);
            positionsBuffer.CopyFromSystemMemory(positions);

            VertexArray va = context.CreateVertexArray();
            va.Attributes[positionLocation] =
                new VertexBufferAttribute(positionsBuffer, ComponentDatatype.Float, 4);

            return va;
        }

        public static void ValidateColor(Texture2D colorTexture, byte red, byte green, byte blue)
        {
            using (ReadPixelBuffer readPixelBuffer = colorTexture.CopyToBuffer(ImageFormat.RedGreenBlue, ImageDatatype.UnsignedByte, 1))
            {
                byte[] color = readPixelBuffer.CopyToSystemMemory<byte>();
                Assert.AreEqual(red, color[0], "Red does not match");
                Assert.AreEqual(green, color[1], "Green does not match");
                Assert.AreEqual(blue, color[2], "Blue does not match");
            }
        }
    }
}
