#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using MiniGlobe.Core;

namespace MiniGlobe.Renderer
{
    public class TextureAtlas
    {
        public TextureAtlas(IEnumerable<Bitmap> bitmaps)
            : this(bitmaps, 0)
        {
        }

        public TextureAtlas(IEnumerable<Bitmap> bitmaps, int borderWidthInPixels)
        {
            if (bitmaps == null)
            {
                throw new ArgumentNullException("bitmaps");
            }

            List<Bitmap> bitmapList = new List<Bitmap>(bitmaps);

            if (bitmapList.Count == 0)
            {
                throw new ArgumentException("bitmaps does not contain any items.");
            }

            if (bitmapList[0] == null)
            {
                throw new ArgumentNullException("An item in bitmaps is null.");
            }

            PixelFormat pixelFormat = bitmapList[0].PixelFormat;

            for (int i = 1; i < bitmapList.Count; ++i)
            {
                Bitmap b = bitmapList[i];

                if (b == null)
                {
                    throw new ArgumentNullException("An item in bitmaps is null.");
                }

                if (b.PixelFormat != pixelFormat)
                {
                    throw new ArgumentException("All bitmaps must have the same PixelFormat.");
                }
            }

            if (borderWidthInPixels < 0)
            {
                throw new ArgumentOutOfRangeException("borderWidthInPixels");
            }

            ///////////////////////////////////////////////////////////////////

            bitmapList.Sort(new BitmapMaximumToMinimumHeight());

            IList<Point> offsets = new List<Point>(bitmapList.Count);
            int width = ComputeAtlasWidth(bitmapList, borderWidthInPixels);
            int xOffset = 0;
            int yOffset = 0;
            int rowHeight = 0;

            //
            // TODO:  Pack more tightly based on algorithm in
            //
            //     http://www-ui.is.s.u-tokyo.ac.jp/~takeo/papers/i3dg2001.pdf
            //
            for (int i = 0; i < bitmapList.Count; ++i)
            {
                Bitmap b = bitmapList[i];

                int widthIncrement = b.Width + borderWidthInPixels;

                if (xOffset + widthIncrement > width)
                {
                    xOffset = 0;
                    yOffset += rowHeight + borderWidthInPixels;
                }

                if (xOffset == 0)
                {
                    //
                    // The first bitmap of the row determines the row height.
                    // This is worst case since bitmaps are sorted by height.
                    //
                    rowHeight = b.Height;
                }

                offsets.Add(new Point(xOffset, yOffset));
                xOffset += widthIncrement;
            }
            int height = yOffset + rowHeight;

            ///////////////////////////////////////////////////////////////////

            IList<RectangleH> textureCoordinates = new List<RectangleH>(bitmapList.Count);
            Bitmap bitmap = new Bitmap(width, height, pixelFormat);
            Graphics graphics = Graphics.FromImage(bitmap);

            double widthD = width;
            double heightD = height;

            for (int i = 0; i < bitmapList.Count; ++i)
            {
                Point upperLeft = offsets[i];
                Bitmap b = bitmapList[i];

                textureCoordinates.Add(new RectangleH(
                    new Vector2H(                                       // Lower Left
                        (double)upperLeft.X / widthD,
                        (heightD - (double)(upperLeft.Y + b.Height)) / heightD),
                    new Vector2H(                                       // Upper Right
                        (double)(upperLeft.X + b.Width) / widthD,
                        (heightD - (double)upperLeft.Y) / heightD)));

                graphics.DrawImageUnscaled(b, upperLeft);
            }
            graphics.Dispose();

            _bitmap = bitmap;
            _textureCoordinates = textureCoordinates;
        }

        public Bitmap Bitmap
        {
            get { return _bitmap; }
        }

        public IList<RectangleH> TextureCoordinates
        {
            get { return _textureCoordinates; }
        }

        private static int ComputeAtlasWidth(IList<Bitmap> bitmaps, int borderWidthInPixels)
        {
            int maxWidth = 0;
            int area = 0;
            for (int i = 0; i < bitmaps.Count; ++i)
            {
                area += (bitmaps[i].Width + borderWidthInPixels) * (bitmaps[i].Height + borderWidthInPixels);
                maxWidth = Math.Max(maxWidth, bitmaps[i].Width);
            }

            return Math.Max((int)Math.Sqrt((double)area), maxWidth + borderWidthInPixels);
        }

        private readonly Bitmap _bitmap;
        private readonly IList<RectangleH> _textureCoordinates;

        private class BitmapMaximumToMinimumHeight : IComparer<Bitmap>
        {
            public int Compare(Bitmap left, Bitmap right)
            {
                return right.Height - left.Height;
            }
        }
    }
}
