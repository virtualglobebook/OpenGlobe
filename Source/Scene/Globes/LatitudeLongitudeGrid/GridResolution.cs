#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using OpenGlobe.Core;

namespace OpenGlobe.Scene
{
    public class GridResolution
    {
        public GridResolution(Interval interval, Vector2D resolution)
        {
            _interval = interval;
            _resolution = resolution;
        }

        public Interval Interval { get { return _interval; } }
        public Vector2D Resolution { get { return _resolution; } }

        private readonly Interval _interval;
        private readonly Vector2D _resolution;
    }
}