using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.IO;

namespace OpenGlobe.Tests.Scene.Terrain
{
    [TestFixture]
    public class CreateMipmapTiles
    {
        [Test]
        [Explicit]
        public void Test()
        {
            string inputFilename = @"..\..\..\..\Data\Terrain\ps_height_16k.bt";
            string outputPath = Path.ChangeExtension(inputFilename, "");

            using (FileStream inputStream = new FileStream(inputFilename, FileMode.Open, FileAccess.Read))
            {
                byte[] header = new byte[256];
                ReallyRead(inputStream, header, 0, 256);

                int columns = BitConverter.ToInt32(header, 10);
                int rows = BitConverter.ToInt32(header, 14);
                short bytesPerElevation = BitConverter.ToInt16(header, 18);
                short isFloatingPoint = BitConverter.ToInt16(header, 20);

                if (bytesPerElevation != 2)
                    throw new InvalidDataException("I only read 2-byte elevations.");
                if (isFloatingPoint != 0)
                    throw new InvalidDataException("I only read integer elevations.");
                if (columns != rows)
                    throw new InvalidDataException("I don't know how to handle different numbers of rows and columns.");

                Directory.CreateDirectory(outputPath);

                const int tileXSize = 512;
                const int tileYSize = 512;

                const int tileXPosts = tileXSize + 1;
                const int tileYPosts = tileYSize + 1;

                const int lowestDetailMaxSize = 255;

                int xTiles = (columns - 1) / tileXSize;
                int yTiles = (rows - 1) / tileYSize;

                int levels = 0;
                int shift = columns;
                while (shift > lowestDetailMaxSize)
                {
                    shift >>= 1;
                    ++levels;
                }

                for (int level = 0; level <= levels; ++level)
                {
                    string levelPath = Path.Combine(outputPath, (levels - level).ToString());
                    int levelPower = 1 << level;

                    for (int yTile = 0; yTile < yTiles / levelPower + 1; ++yTile)
                    {
                        for (int xTile = 0; xTile < xTiles / levelPower + 1; ++xTile)
                        {
                            string tilePath = Path.Combine(levelPath, yTile.ToString());
                            Directory.CreateDirectory(tilePath);
                            tilePath = Path.Combine(tilePath, xTile.ToString() + ".bil");
                            using (FileStream tileStream = new FileStream(tilePath, FileMode.Create))
                            {
                                int postsX = tileXPosts;
                                if (postsX > columns)
                                    postsX = columns;
                                int postsY = tileYPosts;
                                if (postsY > rows)
                                    postsY = rows;
                                
                                byte[] tileData = new byte[postsX * postsY * sizeof(short)];
                                byte[] columnData = new byte[((postsY - 1) * levelPower + 1) * sizeof(short)];
                                for (int column = 0; column < tileXPosts; ++column)
                                {
                                    int offset = 256 + ((xTile * tileXSize + column) * levelPower * rows + yTile * tileYSize * levelPower) * sizeof(short);
                                    inputStream.Seek(offset, SeekOrigin.Begin);
                                    ReallyRead(inputStream, columnData, 0, columnData.Length);

                                    for (int i = 0; i < tileYPosts; ++i)
                                    {
                                        tileData[column * 2 + i * 2 * postsX] = columnData[i * 2 * levelPower];
                                        tileData[column * 2 + i * 2 * postsX + 1] = columnData[i * 2 * levelPower + 1];
                                    }
                                }

                                tileStream.Write(tileData, 0, tileData.Length);
                                
                                /*tilePath = Path.ChangeExtension(tilePath, ".png");

                                Bitmap bmp = new Bitmap(tileXPosts, tileYPosts, PixelFormat.Format24bppRgb);
                                for (int j = 0; j < tileYPosts; ++j)
                                {
                                    for (int i = 0; i < tileXPosts; ++i)
                                    {
                                        int postIndex = j * tileXPosts + i;
                                        short height = BitConverter.ToInt16(tileData, postIndex * 2);
                                        height /= 200;
                                        if (height > 0)
                                            bmp.SetPixel(i, j, Color.FromArgb(0, height, 0));
                                        else if (height >= -255)
                                            bmp.SetPixel(i, j, Color.FromArgb(0, 0, -height));
                                        else
                                            bmp.SetPixel(i, j, Color.FromArgb(255, 0, 0));
                                    }
                                }
                                bmp.Save(tilePath);*/
                            }
                        }
                    }
                }
            }
        }

        private void ReallyRead(Stream stream, byte[] buffer, int index, int length)
        {
            int read = 0;
            while (read < length)
            {
                int justRead = stream.Read(buffer, read, length - read);
                if (justRead == 0)
                    throw new InvalidDataException("Unexpected end of file found.");
                read += justRead;
            }
        }
    }
}
