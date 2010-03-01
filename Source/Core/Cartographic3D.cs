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
    public struct Cartographic3D : IEquatable<Cartographic3D>
    {
        public Cartographic3D(double longitude, double latitude, double height)
        {
            _longitude = longitude;
            _latitude = latitude;
            _height = height;
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

        public bool Equals(Cartographic3D other)
        {
            return _longitude == other._longitude && _latitude == other._latitude && _height == other._height;
        }

        public static bool operator ==(Cartographic3D left, Cartographic3D right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Cartographic3D left, Cartographic3D right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            if (obj is Cartographic3D)
            {
                return Equals((Cartographic3D)obj);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return _longitude.GetHashCode() ^ _latitude.GetHashCode() ^ _height.GetHashCode();
        }

        private double _longitude;
        private double _latitude;
        private double _height;
    }
}
