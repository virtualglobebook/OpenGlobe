#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System.Drawing;
using System.Drawing.Imaging;
using NUnit.Framework;
using OpenGlobe.Core;

namespace OpenGlobe.Renderer
{
    [TestFixture]
    public class BufferObjectTests
    {
        [Test]
        public void VertexBuffer()
        {
            Vector3F[] positions = new Vector3F[] 
            { 
                Vector3F.Zero,
                new Vector3F(1, 0, 0),
                new Vector3F(0, 1, 0)
            };
            int sizeInBytes = ArraySizeInBytes.Size(positions);

            using (GraphicsWindow window = Device.CreateWindow(1, 1))
            using (VertexBuffer vertexBuffer = Device.CreateVertexBuffer(BufferHint.StaticDraw, sizeInBytes))
            {
                //
                // Verify creating vertex buffer
                //
                Assert.IsNotNull(vertexBuffer);
                Assert.AreEqual(BufferHint.StaticDraw, vertexBuffer.UsageHint);
                Assert.AreEqual(sizeInBytes, vertexBuffer.SizeInBytes);

                //
                // Verify copying entire buffer between system memory and vertex buffer
                //
                vertexBuffer.CopyFromSystemMemory(positions);

                Vector3F[] positions2 = vertexBuffer.CopyToSystemMemory<Vector3F>(0, vertexBuffer.SizeInBytes);
                Assert.AreEqual(positions[0], positions2[0]);
                Assert.AreEqual(positions[1], positions2[1]);
                Assert.AreEqual(positions[2], positions2[2]);

                //
                // Verify modiying a subset of the vertex buffer
                //
                Vector3F[] modifiedPositions = new Vector3F[] 
                { 
                    new Vector3F(0, 1, 0),
                    Vector3F.Zero
                };
                vertexBuffer.CopyFromSystemMemory(modifiedPositions, SizeInBytes<Vector3F>.Value, SizeInBytes<Vector3F>.Value);

                Vector3F[] positions3 = vertexBuffer.CopyToSystemMemory<Vector3F>(0, vertexBuffer.SizeInBytes);
                Assert.AreEqual(positions[0], positions3[0]);
                Assert.AreEqual(modifiedPositions[0], positions3[1]);
                Assert.AreEqual(positions[2], positions3[2]);
            }
        }

        [Test]
        public void IndexBuffer()
        {
            uint[] indices = new uint[] { 0, 1, 2 };
            int sizeInBytes = indices.Length * sizeof(uint);

            using (GraphicsWindow window = Device.CreateWindow(1, 1))
            using (IndexBuffer indexBuffer = Device.CreateIndexBuffer(BufferHint.DynamicDraw, sizeInBytes))
            {
                //
                // Verify creating index buffer
                //
                Assert.IsNotNull(indexBuffer);
                Assert.AreEqual(BufferHint.DynamicDraw, indexBuffer.UsageHint);
                Assert.AreEqual(sizeInBytes, indexBuffer.SizeInBytes);

                //
                // Verify copying entire buffer between system memory and index buffer
                //
                indexBuffer.CopyFromSystemMemory(indices);
                Assert.AreEqual(IndexBufferDatatype.UnsignedInt, indexBuffer.Datatype);

                uint[] indices2 = indexBuffer.CopyToSystemMemory<uint>(0, indexBuffer.SizeInBytes);
                Assert.AreEqual(indices[0], indices2[0]);
                Assert.AreEqual(indices[1], indices2[1]);
                Assert.AreEqual(indices[2], indices2[2]);

                //
                // Verify modiying a subset of the index buffer
                //
                uint modifiedIndex = 3;
                indexBuffer.CopyFromSystemMemory(new[] { modifiedIndex }, sizeof(uint));

                uint[] indices3 = indexBuffer.CopyToSystemMemory<uint>(0, indexBuffer.SizeInBytes);
                Assert.AreEqual(indices[0], indices3[0]);
                Assert.AreEqual(modifiedIndex, indices3[1]);
                Assert.AreEqual(indices[2], indices3[2]);
            }
        }

        [Test]
        public void UniformBuffer()
        {
            using (GraphicsWindow window = Device.CreateWindow(1, 1))
            using (ShaderProgram sp = Device.CreateShaderProgram(
                    ShaderSources.PassThroughVertexShader(),
                    ShaderSources.RedUniformBlockFragmentShader()))
            using (UniformBuffer uniformBuffer = Device.CreateUniformBuffer(BufferHint.DynamicDraw, sp.UniformBlocks["RedBlock"].SizeInBytes))
            {
                Assert.IsFalse(sp.Log.Contains("warning"));

                UniformBlock redBlock = sp.UniformBlocks["RedBlock"];
                UniformBlockMember red = redBlock.Members["red"];

                //
                // Verify creating uniform buffer
                //
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
            }
        }

        [Test]
        public void WritePixelBuffer()
        {
            BlittableRGBA[] pixels = new BlittableRGBA[]
            {
                new BlittableRGBA(Color.Red), 
                new BlittableRGBA(Color.Green), 
                new BlittableRGBA(Color.Blue), 
                new BlittableRGBA(Color.White)
            };
            int sizeInBytes = ArraySizeInBytes.Size(pixels);

            using (GraphicsWindow window = Device.CreateWindow(1, 1))
            using (WritePixelBuffer pixelBuffer = Device.CreateWritePixelBuffer(PixelBufferHint.Stream, sizeInBytes))
            {
                //
                // Verify creating pixel buffer
                //
                Assert.IsNotNull(pixelBuffer);
                Assert.AreEqual(PixelBufferHint.Stream, pixelBuffer.UsageHint);
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
            }
        }

        [Test]
        public void WritePixelBufferBitmap()
        {
            using (Bitmap bitmap = new Bitmap(1, 1))
            using (GraphicsWindow window = Device.CreateWindow(1, 1))
            using (WritePixelBuffer pixelBuffer = Device.CreateWritePixelBuffer(PixelBufferHint.Stream,
                BitmapAlgorithms.SizeOfPixelsInBytes(bitmap)))
            {
                Color color = Color.FromArgb(0, 1, 2, 3);
                bitmap.SetPixel(0, 0, color);

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
                using (Bitmap readBackBitmap = pixelBuffer.CopyToBitmap(1, 1, PixelFormat.Format32bppArgb))
                {
                    Assert.AreEqual(color, readBackBitmap.GetPixel(0, 0));
                }
            }
        }
    }
}
