#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Diagnostics;
using OpenGlobe.Core;

using ImagingPixelFormat = System.Drawing.Imaging.PixelFormat;

namespace OpenGlobe.Renderer
{
    internal static class TextureUtility
    {
        public static ImageDatatype ImagingPixelFormatToDatatype(ImagingPixelFormat pixelFormat)
        {
            // TODO:  Not tested exhaustively
            Debug.Assert(Supported(pixelFormat));

            if (pixelFormat == ImagingPixelFormat.Format16bppRgb555)
            {
                return ImageDatatype.UnsignedShort5551;
            }
            else if (pixelFormat == ImagingPixelFormat.Format16bppRgb565)
            {
                return ImageDatatype.UnsignedShort565;
            }
            else if ((pixelFormat == ImagingPixelFormat.Format24bppRgb) ||
                     (pixelFormat == ImagingPixelFormat.Format32bppRgb) ||
                     (pixelFormat == ImagingPixelFormat.Format32bppArgb))
            {
                return ImageDatatype.UnsignedByte;
            }
            else if ((pixelFormat == ImagingPixelFormat.Format48bppRgb) ||
                     (pixelFormat == ImagingPixelFormat.Format64bppArgb))
            {
                return ImageDatatype.UnsignedShort;
            }
            else if (pixelFormat == ImagingPixelFormat.Format16bppArgb1555)
            {
                return ImageDatatype.UnsignedShort1555Reversed;
            }

            Debug.Fail("pixelFormat");
            return ImageDatatype.UnsignedByte;
        }

        public static ImageFormat ImagingPixelFormatToImageFormat(ImagingPixelFormat pixelFormat)
        {
            Debug.Assert(Supported(pixelFormat));

            if ((pixelFormat == ImagingPixelFormat.Format16bppRgb555) ||
                (pixelFormat == ImagingPixelFormat.Format16bppRgb565) ||
                (pixelFormat == ImagingPixelFormat.Format24bppRgb) ||
                (pixelFormat == ImagingPixelFormat.Format32bppRgb) ||
                (pixelFormat == ImagingPixelFormat.Format48bppRgb))
            {
                return ImageFormat.BlueGreenRed;
            }
            else if ((pixelFormat == ImagingPixelFormat.Format16bppArgb1555) ||
                     (pixelFormat == ImagingPixelFormat.Format32bppArgb) ||
                     (pixelFormat == ImagingPixelFormat.Format64bppArgb))
            {
                return ImageFormat.BlueGreenRedAlpha;
            }

            Debug.Fail("pixelFormat");
            return ImageFormat.BlueGreenRed;
        }

        private static bool Supported(ImagingPixelFormat pixelFormat)
        {
            return
                (pixelFormat != ImagingPixelFormat.DontCare) &&
                (pixelFormat != ImagingPixelFormat.Undefined) &&
                (pixelFormat != ImagingPixelFormat.Indexed) &&
                (pixelFormat != ImagingPixelFormat.Gdi) &&
                (pixelFormat != ImagingPixelFormat.Extended) &&
                (pixelFormat != ImagingPixelFormat.Format1bppIndexed) &&
                (pixelFormat != ImagingPixelFormat.Format4bppIndexed) &&
                (pixelFormat != ImagingPixelFormat.Format8bppIndexed) &&
                (pixelFormat != ImagingPixelFormat.Alpha) &&
                (pixelFormat != ImagingPixelFormat.PAlpha) &&
                (pixelFormat != ImagingPixelFormat.Format32bppPArgb) &&
                (pixelFormat != ImagingPixelFormat.Format16bppGrayScale) &&
                (pixelFormat != ImagingPixelFormat.Format64bppPArgb) &&
                (pixelFormat != ImagingPixelFormat.Canonical);
        }

        public static bool IsPowerOfTwo(uint i)
        {
            return (i != 0) && ((i & (i - 1)) == 0);
        }

        public static int RequiredSizeInBytes(
            int width, 
            int height, 
            ImageFormat format, 
            ImageDatatype dataType, 
            int rowAlignment)
        {
            int rowSize = width * NumberOfChannels(format) * SizeInBytes(dataType);

            int remainder = (rowSize % rowAlignment);
            rowSize += (rowAlignment - remainder) % rowAlignment;

            return rowSize * height;
        }

        public static int NumberOfChannels(ImageFormat format)
        {
            switch (format)
            {
                case ImageFormat.StencilIndex:
                    return 1;
                case ImageFormat.DepthComponent:
                    return 1;
                case ImageFormat.Red:
                    return 1;
                case ImageFormat.Green:
                    return 1;
                case ImageFormat.Blue:
                    return 1;
                case ImageFormat.RedGreenBlue:
                    return 3;
                case ImageFormat.RedGreenBlueAlpha:
                    return 4;
                case ImageFormat.BlueGreenRed:
                    return 3;
                case ImageFormat.BlueGreenRedAlpha:
                    return 4;
                case ImageFormat.RedGreen:
                    return 2;
                case ImageFormat.RedGreenInteger:
                    return 2;
                case ImageFormat.DepthStencil:
                    return 2;
                case ImageFormat.RedInteger:
                    return 1;
                case ImageFormat.GreenInteger:
                    return 1;
                case ImageFormat.BlueInteger:
                    return 1;
                case ImageFormat.RedGreenBlueInteger:
                    return 3;
                case ImageFormat.RedGreenBlueAlphaInteger:
                    return 4;
                case ImageFormat.BlueGreenRedInteger:
                    return 3;
                case ImageFormat.BlueGreenRedAlphaInteger:
                    return 4;
            }

            throw new ArgumentException("format");
        }

        public static int SizeInBytes(ImageDatatype dataType)
        {
            Debug.Assert(
                (dataType == ImageDatatype.Byte) ||
                (dataType == ImageDatatype.UnsignedByte) ||
                (dataType == ImageDatatype.Short) ||
                (dataType == ImageDatatype.UnsignedShort) ||
                (dataType == ImageDatatype.Int) ||
                (dataType == ImageDatatype.UnsignedInt) ||
                (dataType == ImageDatatype.Float) ||
                (dataType == ImageDatatype.HalfFloat) ||
                (dataType == ImageDatatype.UnsignedByte332) ||
                (dataType == ImageDatatype.UnsignedShort4444) ||
                (dataType == ImageDatatype.UnsignedShort5551) ||
                (dataType == ImageDatatype.UnsignedInt8888) ||
                (dataType == ImageDatatype.UnsignedInt1010102) ||
                (dataType == ImageDatatype.UnsignedByte233Reversed) ||
                (dataType == ImageDatatype.UnsignedShort565) ||
                (dataType == ImageDatatype.UnsignedShort565Reversed) ||
                (dataType == ImageDatatype.UnsignedShort4444Reversed) ||
                (dataType == ImageDatatype.UnsignedShort1555Reversed) ||
                (dataType == ImageDatatype.UnsignedInt8888Reversed) ||
                (dataType == ImageDatatype.UnsignedInt2101010Reversed) ||
                (dataType == ImageDatatype.UnsignedInt248) ||
                (dataType == ImageDatatype.UnsignedInt10F11F11FReversed) ||
                (dataType == ImageDatatype.UnsignedInt5999Reversed) ||
                (dataType == ImageDatatype.Float32UnsignedInt248Reversed));

            return _sizeInBytes[(int)dataType];
        }

        private static readonly int[] _sizeInBytes = new int[]
        {
            1, // Byte
            1, // UnsignedByte
            2, // Short
            2, // UnsignedShort
            4, // Int
            4, // UnsignedInt
            4, // Float
            2, // HalfFloat
            1, // UnsignedByte332
            2, // UnsignedShort4444
            2, // UnsignedShort5551
            4, // UnsignedInt8888
            4, // UnsignedInt1010102
            1, // UnsignedByte233Reversed
            2, // UnsignedShort565
            2, // UnsignedShort565Reversed
            2, // UnsignedShort4444Reversed
            2, // UnsignedShort1555Reversed
            4, // UnsignedInt8888Reversed
            4, // UnsignedInt2101010Reversed
            4, // UnsignedInt248
            4, // UnsignedInt10F11F11FReversed
            4, // UnsignedInt5999Reversed
            4  // Float32UnsignedInt248Reversed
        };
    }
}
