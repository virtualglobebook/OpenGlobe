#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;
using OpenGlobe.Core;
using ImagingPixelFormat = System.Drawing.Imaging.PixelFormat;

namespace OpenGlobe.Renderer
{
    internal static class TextureUtility
    {
        public static ImageDatatype ImagingPixelFormatToDatatype(ImagingPixelFormat pixelFormat)
        {
            if (!Supported(pixelFormat))
            {
                throw new ArgumentException("Pixel format is not supported.", "pixelFormat");
            }

            // TODO:  Not tested exhaustively
            switch (pixelFormat)
            {
                case ImagingPixelFormat.Format16bppRgb555:
                    return ImageDatatype.UnsignedShort5551;
                case ImagingPixelFormat.Format16bppRgb565:
                    return ImageDatatype.UnsignedShort565;
                case ImagingPixelFormat.Format24bppRgb:
                case ImagingPixelFormat.Format32bppRgb:
                case ImagingPixelFormat.Format32bppArgb:
                    return ImageDatatype.UnsignedByte;
                case ImagingPixelFormat.Format48bppRgb:
                case ImagingPixelFormat.Format64bppArgb:
                    return ImageDatatype.UnsignedShort;
                case ImagingPixelFormat.Format16bppArgb1555:
                    return ImageDatatype.UnsignedShort1555Reversed;
            }

            throw new ArgumentException("pixelFormat");
        }

        public static ImageFormat ImagingPixelFormatToImageFormat(ImagingPixelFormat pixelFormat)
        {
            if (!Supported(pixelFormat))
            {
                throw new ArgumentException("Pixel format is not supported.", "pixelFormat");
            }

            switch (pixelFormat)
            {
                case ImagingPixelFormat.Format16bppRgb555:
                case ImagingPixelFormat.Format16bppRgb565:
                case ImagingPixelFormat.Format24bppRgb:
                case ImagingPixelFormat.Format32bppRgb:
                case ImagingPixelFormat.Format48bppRgb:
                    return ImageFormat.BlueGreenRed;
                case ImagingPixelFormat.Format16bppArgb1555:
                case ImagingPixelFormat.Format32bppArgb:
                case ImagingPixelFormat.Format64bppArgb:
                    return ImageFormat.BlueGreenRedAlpha;
            }

            throw new ArgumentException("pixelFormat");
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
            switch (dataType)
            {
                case ImageDatatype.Byte:
                    return 1;
                case ImageDatatype.UnsignedByte:
                    return 1;
                case ImageDatatype.Short:
                    return 2;
                case ImageDatatype.UnsignedShort:
                    return 2;
                case ImageDatatype.Int:
                    return 4;
                case ImageDatatype.UnsignedInt:
                    return 4;
                case ImageDatatype.Float:
                    return 4;
                case ImageDatatype.HalfFloat:
                    return 2;
                case ImageDatatype.UnsignedByte332:
                    return 1;
                case ImageDatatype.UnsignedShort4444:
                    return 2;
                case ImageDatatype.UnsignedShort5551:
                    return 2;
                case ImageDatatype.UnsignedInt8888:
                    return 4;
                case ImageDatatype.UnsignedInt1010102:
                    return 4;
                case ImageDatatype.UnsignedByte233Reversed:
                    return 1;
                case ImageDatatype.UnsignedShort565:
                    return 2;
                case ImageDatatype.UnsignedShort565Reversed:
                    return 2;
                case ImageDatatype.UnsignedShort4444Reversed:
                    return 2;
                case ImageDatatype.UnsignedShort1555Reversed:
                    return 2;
                case ImageDatatype.UnsignedInt8888Reversed:
                    return 4;
                case ImageDatatype.UnsignedInt2101010Reversed:
                    return 4;
                case ImageDatatype.UnsignedInt248:
                    return 4;
                case ImageDatatype.UnsignedInt10F11F11FReversed:
                    return 4;
                case ImageDatatype.UnsignedInt5999Reversed:
                    return 4;
                case ImageDatatype.Float32UnsignedInt248Reversed:
                    return 4;
            }

            throw new ArgumentException("dataType");
        }
    }
}
