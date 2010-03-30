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
        public TerrainTile(
            Size size,
            float[] heights,
            float minimumHeight,
            float maximumHeight)
        {
            if (size.Width < 0 || size.Height < 0)
            {
                throw new ArgumentOutOfRangeException("size");
            }

            if (heights == null)
            {
                throw new ArgumentNullException("heights");
            }

            if (heights.Length != size.Width * size.Height)
            {
                throw new ArgumentException("heights", "heights.Length != size.Width * size.Height");
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

        public Size Size        // TODO Size is mutable, should be Vector2I
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

        private readonly Size _size;
        private readonly float[] _heights;
        private readonly float _minimumHeight;
        private readonly float _maximumHeight;
    }
}