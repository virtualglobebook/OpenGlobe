#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;

namespace OpenGlobe.Core
{
    public struct GeodeticExtent : IEquatable<GeodeticExtent>
    {
        public GeodeticExtent(double west, double south, double east, double north)
        {
            _west = west;
            _south = south;
            _east = east;
            _north = north;
        }

        public GeodeticExtent(Geodetic2D bottomLeft, Geodetic2D topRight)
        {
            _west = bottomLeft.Longitude;
            _south = bottomLeft.Latitude;
            _east = topRight.Longitude;
            _north = topRight.Latitude;
        }

        public double West
        {
            get { return _west; }
        }

        public double South
        {
            get { return _south; }
        }

        public double East
        {
            get { return _east; }
        }

        public double North
        {
            get { return _north; }
        }

        public bool Equals(GeodeticExtent other)
        {
            return _west == other._west &&
                   _south == other._south &&
                   _east == other._east &&
                   _north == other._north;
        }

        public static bool operator ==(GeodeticExtent left, GeodeticExtent right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(GeodeticExtent left, GeodeticExtent right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            if (obj is GeodeticExtent)
            {
                return Equals((GeodeticExtent)obj);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return _west.GetHashCode() ^
                   _south.GetHashCode() ^
                   _east.GetHashCode() ^
                   _north.GetHashCode();
        }

        private readonly double _west;
        private readonly double _south;
        private readonly double _east;
        private readonly double _north;
    }
}
