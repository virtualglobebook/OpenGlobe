#region License
//
// (C) Copyright 2010 Patrick Cozzi, Deron Ohlarik, and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;

namespace OpenGlobe.Core
{
    public struct Geodetic2D : IEquatable<Geodetic2D>
    {
        public Geodetic2D(double longitude, double latitude)
        {
            _longitude = longitude;
            _latitude = latitude;
        }

        public Geodetic2D(Geodetic3D geodetic3D)
        {
            _longitude = geodetic3D.Longitude;
            _latitude = geodetic3D.Longitude;
        }

        public double Longitude
        {
            get { return _longitude; }
        }

        public double Latitude
        {
            get { return _latitude; }
        }

        public bool Equals(Geodetic2D other)
        {
            return _longitude == other._longitude && _latitude == other._latitude;
        }

        public static bool operator ==(Geodetic2D left, Geodetic2D right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Geodetic2D left, Geodetic2D right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            if (obj is Geodetic2D)
            {
                return Equals((Geodetic2D)obj);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return _longitude.GetHashCode() ^ _latitude.GetHashCode();
        }

        private readonly double _longitude;
        private readonly double _latitude;
    }
}
