#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using MiniGlobe.Core;
using OpenTK;

namespace MiniGlobe.Examples.Chapter3
{
    // TODO:  Add POI marker to this example
    // TODO:  Add equator, etc.
    class GridResolution
    {
        public GridResolution(Interval interval, Vector2d resolution)
        {
            _interval = interval;
            _resolution = resolution;
        }

        public Interval Interval { get { return _interval; } }
        public Vector2d Resolution { get { return _resolution; } }

        private readonly Interval _interval;
        private readonly Vector2d _resolution;
    }
}