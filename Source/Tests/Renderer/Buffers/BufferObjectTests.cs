#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System.Drawing;
using System.Drawing.Imaging;
using NUnit.Framework;
using MiniGlobe.Core;

namespace MiniGlobe.Renderer
{
    [TestFixture]
    public class BufferObjectTests
    {
        [Test]
        public void VertexBuffer()
        {
            MiniGlobeWindow window = Device.CreateWindow(1, 1);

            Vector3S[] positions = new Vector3S[] 
            { 
                Vector3S.Zero,
                new Vector3S(1, 0, 0),
                new Vector3S(0, 1, 0)
            };

            //
            // Verify creating vertex buffer
            //
            int sizeInBytes = positions.Length * SizeInBytes<Vector3S>.Value;
            VertexBuffer vertexBuffer = Device.CreateVertexBuffer(BufferHint.StaticDraw, sizeInBytes);
            Assert.IsNotNull(vertexBuffer);
            Assert.AreEqual(BufferHint.StaticDraw, vertexBuffer.UsageHint);
            Assert.AreEqual(sizeInBytes, vertexBuffer.SizeInBytes);

            //
            // Verify copying entire buffer between system memory and vertex buffer
            //
            vertexBuffer.CopyFromSystemMemory(positions);

            Vector3S[] positions2 = vertexBuffer.CopyToSystemMemory<Vector3S>(0, vertexBuffer.SizeInBytes);
            Assert.AreEqual(positions[0], positions2[0]);
            Assert.AreEqual(positions[1], positions2[1]);
            Assert.AreEqual(positions[2], positions2[2]);

            //
            // Verify modiying a subset of the vertex buffer
            //
            Vector3S[] modifiedPositions = new Vector3S[] 
            { 
                new Vector3S(0, 1, 0),
                Vector3S.Zero
            };
            vertexBuffer.CopyFromSystemMemory(modifiedPositions, SizeInBytes<Vector3S>.Value, SizeInBytes<Vector3S>.Value);

            Vector3S[] positions3 = vertexBuffer.CopyToSystemMemory<Vector3S>(0, vertexBuffer.SizeInBytes);
            Assert.AreEqual(positions[0], positions3[0]);
            Assert.AreEqual(modifiedPositions[0], positions3[1]);
            Assert.AreEqual(positions[2], positions3[2]);

            vertexBuffer.Dispose();
            window.Dispose();
        }

        [Test]
        public void IndexBuffer()
        {
            MiniGlobeWindow window = Device.CreateWindow(1, 1);

            uint[] indicies = new uint[] { 0, 1, 2 };

            //
            // Verify creating index buffer
            //
            int sizeInBytes = indicies.Length * sizeof(uint);
            IndexBuffer indexBuffer = Device.CreateIndexBuffer(BufferHint.DynamicDraw, sizeInBytes);
            Assert.IsNotNull(indexBuffer);
            Assert.AreEqual(BufferHint.DynamicDraw, indexBuffer.UsageHint);
            Assert.AreEqual(sizeInBytes, indexBuffer.SizeInBytes);

            //
            // Verify copying entire buffer between system memory and index buffer
            //
            indexBuffer.CopyFromSystemMemory(indicies);
            Assert.AreEqual(IndexBufferDataType.UnsignedInt, indexBuffer.DataType);

            uint[] indicies2 = indexBuffer.CopyToSystemMemory<uint>(0, indexBuffer.SizeInBytes);
            Assert.AreEqual(indicies[0], indicies2[0]);
            Assert.AreEqual(indicies[1], indicies2[1]);
            Assert.AreEqual(indicies[2], indicies2[2]);

            //
            // Verify modiying a subset of the index buffer
            //
            uint modifiedIndex = 3;
            indexBuffer.CopyFromSystemMemory(new[] { modifiedIndex }, sizeof(uint));

            uint[] indicies3 = indexBuffer.CopyToSystemMemory<uint>(0, indexBuffer.SizeInBytes);
            Assert.AreEqual(indicies[0], indicies3[0]);
            Assert.AreEqual(modifiedIndex, indicies3[1]);
            Assert.AreEqual(indicies[2], indicies3[2]);

            indexBuffer.Dispose();
            window.Dispose();
        }

        [Test]
        public void UniformBuffer()
        {
            MiniGlobeWindow window = Device.CreateWindow(1, 1);

            ShaderProgram sp = Device.CreateShaderProgram(
                ShaderSources.PassThroughVertexShader(),
                ShaderSources.RedUniformBlockFragmentShader());
            Assert.IsEmpty(sp.LinkLog);

            UniformBlock redBlock = sp.UniformBlocks["RedBlock"];
            UniformBlockMember red = redBlock.Members["red"];

            //
            // Verify creating uniform buffer
            //
            UniformBuffer uniformBuffer = Device.CreateUniformBuffer(BufferHint.DynamicDraw, redBlock.SizeInBytes);
            Assert.IsNotNull(uniformBuffer);
            Assert.AreEqual(BufferHint.DynamicDraw, uniformBuffer.UsageHint);
            Assert.AreEqual(redBlock.SizeInBytes, uniformBuffer.SizeInBytes);

            redBlock.Bind(uniformBuffer);

            //
            // Verify copying into red member
            //
            float redIntensity = 0.5f;
            uniformBuffer.CopyFromSystemMemory(new[] { redIntensity }, red.OffsetInBytes);

            float[] redIntensity2 = uniformBuffer.CopyToSystemMemory<float>(red.OffsetInBytes, sizeof(float));
            Assert.AreEqual(redIntensity, redIntensity2[0]);

            sp.Dispose();
            uniformBuffer.Dispose();
            window.Dispose();
        }

        [Test]
        public void WritePixelBuffer()
        {
            MiniGlobeWindow window = Device.CreateWindow(1, 1);

            BlittableRGBA[] pixels = new BlittableRGBA[]
            {
                new BlittableRGBA(Color.Red), 
                new BlittableRGBA(Color.Green), 
                new BlittableRGBA(Color.Blue), 
                new BlittableRGBA(Color.White)
            };

            //
            // Verify creating pixel buffer
            //
            int sizeInBytes = pixels.Length * SizeInBytes<BlittableRGBA>.Value;
            WritePixelBuffer pixelBuffer = Device.CreateWritePixelBuffer(WritePixelBufferHint.StreamDraw, sizeInBytes);
            Assert.IsNotNull(pixelBuffer);
            Assert.AreEqual(WritePixelBufferHint.StreamDraw, pixelBuffer.UsageHint);
            Assert.AreEqual(sizeInBytes, pixelBuffer.SizeInBytes);

            //
            // Verify copying entire buffer between system memory and pixel buffer
            //
            pixelBuffer.CopyFromSystemMemory(pixels);

            BlittableRGBA[] pixels2 = pixelBuffer.CopyToSystemMemory<BlittableRGBA>(0, pixelBuffer.SizeInBytes);
            Assert.AreEqual(pixels[0], pixels2[0]);
            Assert.AreEqual(pixels[1], pixels2[1]);
            Assert.AreEqual(pixels[2], pixels2[2]);

            //
            // Verify modiying a subset of the vertex buffer
            //
            BlittableRGBA modifiedPixel = new BlittableRGBA(Color.Black);
            pixelBuffer.CopyFromSystemMemory(new[] { modifiedPixel }, SizeInBytes<BlittableRGBA>.Value);

            BlittableRGBA[] pixels3 = pixelBuffer.CopyToSystemMemory<BlittableRGBA>(0, pixelBuffer.SizeInBytes);
            Assert.AreEqual(pixels[0], pixels3[0]);
            Assert.AreEqual(modifiedPixel, pixels3[1]);
            Assert.AreEqual(pixels[2], pixels3[2]);

            pixelBuffer.Dispose();
            window.Dispose();
        }

        [Test]
        public void WritePixelBufferBitmap()
        {
            MiniGlobeWindow window = Device.CreateWindow(1, 1);

            Color color = Color.FromArgb(0, 1, 2, 3);
            Bitmap bitmap = new Bitmap(1, 1);
            bitmap.SetPixel(0, 0, color);

            WritePixelBuffer pixelBuffer = Device.CreateWritePixelBuffer(WritePixelBufferHint.StreamDraw, 4);
            pixelBuffer.CopyFromBitmap(bitmap);
            
            //
            // Verify read back - comes back BGRA
            //
            BlittableBGRA[] readBackPixel = pixelBuffer.CopyToSystemMemory<BlittableBGRA>();
            Assert.AreEqual(color.R, readBackPixel[0].R);
            Assert.AreEqual(color.G, readBackPixel[0].G);
            Assert.AreEqual(color.B, readBackPixel[0].B);
            Assert.AreEqual(color.A, readBackPixel[0].A);

            //
            // Verify read back into bitmap
            //
            Bitmap readBackBitmap = pixelBuffer.CopyToBitmap(1, 1, PixelFormat.Format32bppArgb);
            Assert.AreEqual(color, readBackBitmap.GetPixel(0, 0));

            readBackBitmap.Dispose();
            pixelBuffer.Dispose();
            bitmap.Dispose();
            window.Dispose();
        }
    }
}
