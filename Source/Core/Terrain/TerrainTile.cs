#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using MiniGlobe.Core;

namespace MiniGlobe.Terrain
{
    public class TerrainTile
    {
        public TerrainTile(
            GeodeticExtent extent,
            double minimumHeight, 
            double maximumHeight)
        {
            _extent = extent;
            _minimumHeight = minimumHeight;
            _maximumHeight = maximumHeight;
        }

        public GeodeticExtent Extent
        {
            get { return _extent; }
        }

        public double MinimumHeight
        {
            get { return _minimumHeight; }
        }

        public double MaximumHeight
        {
            get { return _maximumHeight; }
        }

        private GeodeticExtent _extent;
        private double _minimumHeight;
        private double _maximumHeight;
    }
}