#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Drawing;
using MiniGlobe.Core;

namespace MiniGlobe.Terrain
{
    public class TerrainTile
    {
        public static TerrainTile FromBitmap(Bitmap bitmap)
        {
            if (bitmap == null)
            {
                throw new ArgumentNullException("bitmap");
            }

            float[] heights = new float[bitmap.Width * bitmap.Height];
            float minHeight = float.MaxValue;
            float maxHeight = float.MinValue;

            int k = 0;
            for (int j = bitmap.Height - 1; j >= 0; --j)
            {
                for (int i = 0; i < bitmap.Width; ++i)
                {
                    float height = (float)(bitmap.GetPixel(i, j).R / 255.0);
                    heights[k++] = height;
                    minHeight = Math.Min(height, minHeight);
                    maxHeight = Math.Max(height, maxHeight);
                }
            }

            return new TerrainTile(
                new Vector2I(bitmap.Width, bitmap.Height),
                heights, minHeight, maxHeight);
        }

        public TerrainTile(
            Vector2I size,
            float[] heights,
            float minimumHeight,
            float maximumHeight)
        {
            if (size.X < 0 || size.Y < 0)
            {
                throw new ArgumentOutOfRangeException("size");
            }

            if (heights == null)
            {
                throw new ArgumentNullException("heights");
            }

            if (heights.Length != size.X * size.Y)
            {
                throw new ArgumentException("heights.Length != size.Width * size.Height");
            }

            if (minimumHeight > maximumHeight)
            {
                throw new ArgumentOutOfRangeException("minimumHeight", "minimumHeight > maximumHeight");
            }

            _size = size;
            _heights = heights;
            _minimumHeight = minimumHeight;
            _maximumHeight = maximumHeight;
        }

        public Vector2I Size
        {
            get { return _size; }
        }

        public float[] Heights
        {
            get { return _heights; }
        }

        public float MinimumHeight
        {
            get { return _minimumHeight; }
        }

        public float MaximumHeight
        {
            get { return _maximumHeight; }
        }

        private readonly Vector2I _size;
        private readonly float[] _heights;
        private readonly float _minimumHeight;
        private readonly float _maximumHeight;
    }
}