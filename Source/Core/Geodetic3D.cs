#region License
//
// (C) Copyright 2010 Patrick Cozzi, Deron Ohlarik, and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;

namespace MiniGlobe.Core
{
    public struct Geodetic3D : IEquatable<Geodetic3D>
    {
        public Geodetic3D(double longitude, double latitude, double height)
        {
            _longitude = longitude;
            _latitude = latitude;
            _height = height;
        }

        public Geodetic3D(double longitude, double latitude)
        {
            _longitude = longitude;
            _latitude = latitude;
            _height = 0;
        }

        public Geodetic3D(Geodetic2D geodetic2D)
        {
            _longitude = geodetic2D.Longitude;
            _latitude = geodetic2D.Latitude;
            _height = 0;
        }

        public double Longitude
        {
            get { return _longitude; }
        }

        public double Latitude
        {
            get { return _latitude; }
        }

        public double Height
        {
            get { return _height; }
        }

        public bool Equals(Geodetic3D other)
        {
            return _longitude == other._longitude && _latitude == other._latitude && _height == other._height;
        }

        public static bool operator ==(Geodetic3D left, Geodetic3D right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Geodetic3D left, Geodetic3D right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            if (obj is Geodetic3D)
            {
                return Equals((Geodetic3D)obj);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return _longitude.GetHashCode() ^ _latitude.GetHashCode() ^ _height.GetHashCode();
        }

        private readonly double _longitude;
        private readonly double _latitude;
        private readonly double _height;
    }
}
