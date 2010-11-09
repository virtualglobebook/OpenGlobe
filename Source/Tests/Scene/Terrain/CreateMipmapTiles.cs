using System;
using System.Collections.Generic;
using System.Linq;
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
                    throw new InvalidDataException("I don't know how to handle different numbers of rows and columns.")

                Directory.CreateDirectory(outputPath);

                const int tileXSize = 512;
                const int tileYSize = 512;

                const int tileXPosts = tileXSize + 1;
                const int tileYPosts = tileYSize + 1;

                int xTiles = (columns - 1) / tileXSize;
                int yTiles = (rows - 1) / tileYSize;

                int levels = 0;
                int shift = columns;
                while (shift > tileXSize)
                {
                    shift >>= 1;
                    ++levels;
                }

                for (int level = 0; level < levels; ++level)
                {
                    string levelPath = Path.Combine(outputPath, level.ToString());
                    int levelPower = 1 >> level;

                    for (int yTile = 0; yTile < yTiles; ++yTile)
                    {
                        for (int xTile = 0; xTile < xTiles; ++xTile)
                        {
                            string tilePath = Path.Combine(levelPath, yTile.ToString());
                            Directory.CreateDirectory(tilePath);
                            tilePath = Path.Combine(tilePath, xTile.ToString() + ".bil");
                            using (FileStream tileStream = new FileStream(tilePath, FileMode.Create))
                            {
                                byte[] tileData = new byte[tileXPosts * tileYPosts * sizeof(short)];
                                byte[] columnData = new byte[tileYPosts * levelPower * sizeof(short)];
                                for (int column = 0; column < tileXPosts; ++column)
                                {
                                    int offset = 256 + (xTile * tileXSize + column) * levelPower * rows * sizeof(short);
                                    inputStream.Seek(offset, SeekOrigin.Begin);
                                    ReallyRead(inputStream, columnData, 0, tileData.Length);

                                    for (int i = 0; i < columnData.Length; ++i)
                                    {
                                        tileData[column + i * tileXPosts] = columnData[i];
                                    }
                                }

                                tileStream.Write(tileData, 0, tileData.Length);
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
                read += stream.Read(buffer, read, length - read);
            }
        }
    }
}
