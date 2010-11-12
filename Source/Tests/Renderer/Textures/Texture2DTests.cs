#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System.Drawing;
using OpenGlobe.Core;
using NUnit.Framework;

namespace OpenGlobe.Renderer
{
    [TestFixture]
    public class Texture2DTests
    {
        [Test]
        public void Texture2DDescription()
        {
            Texture2DDescription description = new Texture2DDescription(512, 256, TextureFormat.RedGreenBlueAlpha8, true);
            Assert.AreEqual(512, description.Width);
            Assert.AreEqual(256, description.Height);
            Assert.AreEqual(TextureFormat.RedGreenBlueAlpha8, description.TextureFormat);
            Assert.IsTrue(description.GenerateMipmaps);

            Texture2DDescription description2 = new Texture2DDescription(512, 256, TextureFormat.RedGreenBlueAlpha8, true);
            Assert.AreEqual(description, description2);

            Texture2DDescription description3 = new Texture2DDescription(64, 32, TextureFormat.RedGreenBlueAlpha8, true);
            Assert.AreNotEqual(description, description3);
        }

        [Test]
        public void Texture2DSampler()
        {
            using (GraphicsWindow window = Device.CreateWindow(1, 1))
            {
                using (TextureSampler filter = Device.CreateTexture2DSampler(
                    TextureMinificationFilter.Linear,
                    TextureMagnificationFilter.Nearest,
                    TextureWrap.MirroredRepeat,
                    TextureWrap.Repeat,
                    2))
                {
                    Assert.AreEqual(TextureMinificationFilter.Linear, filter.MinificationFilter);
                    Assert.AreEqual(TextureMagnificationFilter.Nearest, filter.MagnificationFilter);
                    Assert.AreEqual(TextureWrap.MirroredRepeat, filter.WrapS);
                    Assert.AreEqual(TextureWrap.Repeat, filter.WrapT);
                    Assert.AreEqual(2, filter.MaximumAnisotropic);
                }
            }
        }

        [Test]
        public void Texture2D()
        {
            BlittableRGBA[] pixels = new BlittableRGBA[]
            {
                new BlittableRGBA(Color.Red), 
                new BlittableRGBA(Color.Green)
            };
            int sizeInBytes = pixels.Length * SizeInBytes<BlittableRGBA>.Value;
            Texture2DDescription description = new Texture2DDescription(2, 1, TextureFormat.RedGreenBlueAlpha8, false);

            using (GraphicsWindow window = Device.CreateWindow(1, 1))
            using (WritePixelBuffer writePixelBuffer = Device.CreateWritePixelBuffer(PixelBufferHint.Stream, sizeInBytes))
            using (Texture2D texture = Device.CreateTexture2D(description))
            {
                writePixelBuffer.CopyFromSystemMemory(pixels);

                //
                // Create texture with pixel buffer
                //
                texture.CopyFromBuffer(writePixelBuffer, BlittableRGBA.Format, BlittableRGBA.Datatype);

                //
                // Read back pixels
                //
                using (ReadPixelBuffer readPixelBuffer = texture.CopyToBuffer(BlittableRGBA.Format, BlittableRGBA.Datatype))
                {
                    BlittableRGBA[] readPixels = readPixelBuffer.CopyToSystemMemory<BlittableRGBA>();

                    Assert.AreEqual(sizeInBytes, readPixelBuffer.SizeInBytes);
                    Assert.AreEqual(pixels[0], readPixels[0]);
                    Assert.AreEqual(pixels[1], readPixels[1]);
                    Assert.AreEqual(description, texture.Description);
                }
            }
        }

        [Test]
        public void Texture2DSubImage()
        {
            float[] pixels = new float[]
            {
                1, 2,
                3, 4
            };
            int sizeInBytes = pixels.Length * sizeof(float);
            Texture2DDescription description = new Texture2DDescription(2, 2, TextureFormat.Red32f, true);

            using (GraphicsWindow window = Device.CreateWindow(1, 1))
            using (WritePixelBuffer writePixelBuffer = Device.CreateWritePixelBuffer(PixelBufferHint.Stream, sizeInBytes))
            using (Texture2D texture = Device.CreateTexture2D(description))
            {
                //
                // Copy from system memory to texture via pixel buffer
                //
                writePixelBuffer.CopyFromSystemMemory(pixels);
                texture.CopyFromBuffer(writePixelBuffer, ImageFormat.Red, ImageDatatype.Float, 1);

                //
                // Read back pixels
                //
                using (ReadPixelBuffer readPixelBuffer = texture.CopyToBuffer(ImageFormat.Red, ImageDatatype.Float, 1))
                {
                    float[] readPixels = readPixelBuffer.CopyToSystemMemory<float>();

                    //
                    // Verify
                    //
                    Assert.AreEqual(sizeInBytes, readPixelBuffer.SizeInBytes);
                    Assert.AreEqual(pixels[0], readPixels[0]);
                    Assert.AreEqual(pixels[1], readPixels[1]);
                    Assert.AreEqual(pixels[2], readPixels[2]);
                    Assert.AreEqual(pixels[3], readPixels[3]);
                    Assert.AreEqual(description, texture.Description);

                    //
                    // Update sub image
                    //
                    float modifiedPixel = 9;
                    writePixelBuffer.CopyFromSystemMemory(new[] { modifiedPixel });
                    texture.CopyFromBuffer(writePixelBuffer, 1, 1, 1, 1, ImageFormat.Red, ImageDatatype.Float, 1);
                    using (ReadPixelBuffer readPixelBuffer2 = texture.CopyToBuffer(ImageFormat.Red, ImageDatatype.Float, 1))
                    {
                        float[] readPixels2 = readPixelBuffer2.CopyToSystemMemory<float>();

                        Assert.AreEqual(sizeInBytes, readPixelBuffer2.SizeInBytes);
                        Assert.AreEqual(pixels[0], readPixels2[0]);
                        Assert.AreEqual(pixels[1], readPixels2[1]);
                        Assert.AreEqual(pixels[2], readPixels2[2]);
                        Assert.AreEqual(modifiedPixel, readPixels2[3]);
                    }
                }
            }
        }

        [Test]
        public void Texture2DAlignment()
        {
            byte[] pixels = new byte[]
            {
                1, 2, 3, 4, 4, 6,
                7, 8, 9, 10, 11, 12
            };
            int sizeInBytes = pixels.Length * sizeof(byte);
            Texture2DDescription description = new Texture2DDescription(2, 2, TextureFormat.RedGreenBlue8, false);

            using (GraphicsWindow window = Device.CreateWindow(1, 1))
            using (WritePixelBuffer writePixelBuffer = Device.CreateWritePixelBuffer(PixelBufferHint.Stream, sizeInBytes))
            using (Texture2D texture = Device.CreateTexture2D(description))
            {
                //
                // Copy from system memory to texture via pixel buffer
                //
                writePixelBuffer.CopyFromSystemMemory(pixels);
                texture.CopyFromBuffer(writePixelBuffer, ImageFormat.RedGreenBlue, ImageDatatype.UnsignedByte, 1);

                //
                // Read back pixels
                //
                using (ReadPixelBuffer readPixelBuffer = texture.CopyToBuffer(ImageFormat.RedGreenBlue, ImageDatatype.UnsignedByte, 1))
                {
                    byte[] readPixels = readPixelBuffer.CopyToSystemMemory<byte>();

                    //
                    // Verify
                    //
                    Assert.AreEqual(sizeInBytes, readPixelBuffer.SizeInBytes);
                    for (int i = 0; i < pixels.Length; ++i)
                    {
                        Assert.AreEqual(pixels[i], readPixels[i]);
                    }
                }
                Assert.AreEqual(description, texture.Description);
            }
        }

        [Test]
        [Explicit("NVIDIA Only:  The last channel of the last pixel is not written to the texture")]
        public void Texture2DRowAlignment()
        {
            //
            // Create pixel buffer - RGB, 4 byte aligned, 255 is padding
            //
            byte[] pixels = new byte[]
            {
                1, 2, 3, 255,
                4, 5, 6, 255
            };
            int sizeInBytes = pixels.Length * sizeof(byte);

            using (GraphicsWindow window = Device.CreateWindow(1, 1))
            using (WritePixelBuffer writePixelBuffer = Device.CreateWritePixelBuffer(PixelBufferHint.Stream, sizeInBytes))
            using (Texture2D texture = Device.CreateTexture2D(new Texture2DDescription(1, 2, TextureFormat.RedGreenBlue8, false)))
            {
                writePixelBuffer.CopyFromSystemMemory(pixels);
                texture.CopyFromBuffer(writePixelBuffer, ImageFormat.RedGreenBlue, ImageDatatype.UnsignedByte);

                //
                // Read back pixels
                //
                using (ReadPixelBuffer readPixelBuffer = texture.CopyToBuffer(ImageFormat.RedGreenBlue, ImageDatatype.UnsignedByte, 1))
                {
                    byte[] readPixels = readPixelBuffer.CopyToSystemMemory<byte>();
                    Assert.AreEqual(1, readPixels[0]);
                    Assert.AreEqual(2, readPixels[1]);
                    Assert.AreEqual(3, readPixels[2]);
                    Assert.AreEqual(4, readPixels[3]);
                    Assert.AreEqual(5, readPixels[4]);
                    Assert.AreEqual(6, readPixels[5]);
                }
            }
        }

        [Test]
        public void EnumerateTextureUnits()
        {
            using (GraphicsWindow window = Device.CreateWindow(1, 1))
            {
                int count = 0;
                foreach (TextureUnit unit in window.Context.TextureUnits)
                {
                    Assert.IsNull(unit.Texture);
                    Assert.IsNull(unit.TextureSampler);
                    ++count;
                }
                Assert.AreEqual(count, window.Context.TextureUnits.Count);
            }
        }

        [Test]
        public void TextureUnits()
        {
            using (GraphicsWindow window = Device.CreateWindow(1, 1))
            using (Texture2D texture = Device.CreateTexture2D(new Texture2DDescription(1, 1, TextureFormat.RedGreenBlueAlpha8, false)))
            using (TextureSampler sampler = Device.CreateTexture2DSampler(
                    TextureMinificationFilter.Nearest,
                    TextureMagnificationFilter.Nearest,
                    TextureWrap.Clamp,
                    TextureWrap.Clamp,
                    2))
            {
                window.Context.TextureUnits[0].Texture = texture;
                window.Context.TextureUnits[0].TextureSampler = Device.TextureSamplers.LinearRepeat;

                Assert.AreEqual(texture, window.Context.TextureUnits[0].Texture);
                Assert.AreEqual(Device.TextureSamplers.LinearRepeat, window.Context.TextureUnits[0].TextureSampler);

                //
                // Assign same texture with different filter
                //
                window.Context.TextureUnits[0].Texture = texture;
                window.Context.TextureUnits[0].TextureSampler = sampler;
                Assert.AreEqual(texture, window.Context.TextureUnits[0].Texture);
                Assert.AreEqual(sampler, window.Context.TextureUnits[0].TextureSampler);
            }
        }
    }
}
