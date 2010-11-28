#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System.Drawing;
using System.IO;
using NUnit.Framework;

namespace OpenGlobe.Renderer
{
    [TestFixture]
    public class FramebufferTests
    {
        [Test]
        public void ColorAttachment()
        {
            using (GraphicsWindow window = Device.CreateWindow(1, 1))
            using (Framebuffer framebuffer = window.Context.CreateFramebuffer())
            using (Texture2D colorTexture = Device.CreateTexture2D(new Texture2DDescription(1, 1, TextureFormat.RedGreenBlue8, false)))
            {
                Assert.IsNull(framebuffer.ColorAttachments[0]);
                framebuffer.ColorAttachments[0] = colorTexture;
                Assert.AreEqual(colorTexture, framebuffer.ColorAttachments[0]);

                //
                // Since frame buffer has a color attachment, it is complete, 
                // and should bind without error.
                //
                window.Context.Framebuffer = framebuffer;
                window.Context.Framebuffer = null;

                framebuffer.ColorAttachments[0] = null;
                Assert.IsNull(framebuffer.ColorAttachments[0]);
            }
        }

        [Test]
        public void DepthAttachment()
        {
            using (GraphicsWindow window = Device.CreateWindow(1, 1))
            using (Framebuffer framebuffer = window.Context.CreateFramebuffer())
            using (Texture2D depthTexture = Device.CreateTexture2D(new Texture2DDescription(1, 1, TextureFormat.Depth24, false)))
            {
                Assert.IsNull(framebuffer.DepthAttachment);
                framebuffer.DepthAttachment = depthTexture;
                Assert.AreEqual(depthTexture, framebuffer.DepthAttachment);
                framebuffer.DepthAttachment = null;
                Assert.IsNull(framebuffer.DepthAttachment);
            }
        }

        [Test]
        public void DepthStencilAttachment()
        {
            using (GraphicsWindow window = Device.CreateWindow(1, 1))
            using (Framebuffer framebuffer = window.Context.CreateFramebuffer())
            using (Texture2D depthStencilTexture = Device.CreateTexture2D(new Texture2DDescription(1, 1, TextureFormat.Depth32fStencil8, false)))
            {
                Assert.IsNull(framebuffer.DepthStencilAttachment);
                framebuffer.DepthStencilAttachment = depthStencilTexture;
                Assert.AreEqual(depthStencilTexture, framebuffer.DepthStencilAttachment);
                framebuffer.DepthStencilAttachment = null;
                Assert.IsNull(framebuffer.DepthStencilAttachment);
            }
        }

        [Test]
        public void EnumerateColorAttachments()
        {
            Texture2DDescription description = new Texture2DDescription(1, 1, TextureFormat.RedGreenBlue8, false);

            using (GraphicsWindow window = Device.CreateWindow(1, 1))
            using (Framebuffer framebuffer = window.Context.CreateFramebuffer())
            using (Texture2D color0 = Device.CreateTexture2D(description))
            using (Texture2D color1 = Device.CreateTexture2D(description))
            using (Texture2D color2 = Device.CreateTexture2D(description))
            {
                framebuffer.ColorAttachments[0] = color0;
                framebuffer.ColorAttachments[1] = color1;
                framebuffer.ColorAttachments[2] = color2;
                Assert.AreEqual(3, framebuffer.ColorAttachments.Count);

                framebuffer.ColorAttachments[1] = null;
                Assert.AreEqual(2, framebuffer.ColorAttachments.Count);

                framebuffer.ColorAttachments[1] = color1;
                Assert.AreEqual(3, framebuffer.ColorAttachments.Count);

                int count = 0;
                foreach (Texture2D texture in framebuffer.ColorAttachments)
                {
                    Assert.AreEqual(description, texture.Description);
                    ++count;
                }
                Assert.AreEqual(framebuffer.ColorAttachments.Count, count);
            }
        }

        [Test]
        public void HighResolutionSnapFramebuffer()
        {
            using (GraphicsWindow window = Device.CreateWindow(1, 1))
            using (HighResolutionSnapFramebuffer snap = new HighResolutionSnapFramebuffer(window.Context, 2, 10, 2))
            {

                Assert.AreEqual(2, snap.AspectRatio);
                Assert.AreEqual(10, snap.DotsPerInch);

                Assert.AreEqual(2, snap.WidthInInches);
                Assert.AreEqual(20, snap.WidthInPixels);

                Assert.AreEqual(1, snap.HeightInInches);
                Assert.AreEqual(10, snap.HeightInPixels);

                window.Context.Framebuffer = snap.Framebuffer;
                window.Context.Viewport = new Rectangle(0, 0, snap.WidthInPixels, snap.HeightInPixels);
                window.Context.Clear(new ClearState() { Buffers = ClearBuffers.ColorAndDepthBuffer, Color = Color.Red, Depth = 0.5f });

                string colorFile = "color.bmp";
                string depthFile = "depth.bmp";

                snap.SaveColorBuffer(colorFile);
                snap.SaveDepthBuffer(depthFile);

                try
                {
                    using (Bitmap colorBitmap = new Bitmap(colorFile))
                    {
                        Assert.AreEqual(snap.WidthInPixels, colorBitmap.Width);
                        Assert.AreEqual(snap.HeightInPixels, colorBitmap.Height);
                    }

                    using (Bitmap depthBitmap = new Bitmap(depthFile))
                    {
                        Assert.AreEqual(snap.WidthInPixels, depthBitmap.Width);
                        Assert.AreEqual(snap.HeightInPixels, depthBitmap.Height);
                    }
                }
                catch
                {
                    throw;
                }
                finally
                {
                    File.Delete(colorFile);
                    File.Delete(depthFile);
                }
            }
        }
    }
}
