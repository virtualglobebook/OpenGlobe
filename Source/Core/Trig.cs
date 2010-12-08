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
    public static class Trig
    {
        public const double OneOverPi = 1.0 / Math.PI;
        public const double PiOverTwo = Math.PI * 0.5;
        public const double PiOverThree = Math.PI / 3.0;
        public const double PiOverFour = Math.PI / 4.0;
        public const double PiOverSix = Math.PI / 6.0;
        public const double ThreePiOver2 = (3.0 * Math.PI) * 0.5;
        public const double TwoPi = 2.0 * Math.PI;
        public const double OneOverTwoPi = 1.0 / (2.0 * Math.PI);
        public const double RadiansPerDegree = Math.PI / 180.0;

        public static double ToRadians(double degrees)
        {
            return degrees * RadiansPerDegree;
        }

        public static Geodetic3D ToRadians(Geodetic3D geodetic)
        {
            return new Geodetic3D(ToRadians(geodetic.Longitude), ToRadians(geodetic.Latitude), geodetic.Height);
        }

        public static Geodetic2D ToRadians(Geodetic2D geodetic)
        {
            return new Geodetic2D(ToRadians(geodetic.Longitude), ToRadians(geodetic.Latitude));
        }

        public static double ToDegrees(double radians)
        {
            return radians / RadiansPerDegree;
        }

        public static Geodetic3D ToDegrees(Geodetic3D geodetic)
        {
            return new Geodetic3D(ToDegrees(geodetic.Longitude), ToDegrees(geodetic.Latitude), geodetic.Height);
        }

        public static Geodetic2D ToDegrees(Geodetic2D geodetic)
        {
            return new Geodetic2D(ToDegrees(geodetic.Longitude), ToDegrees(geodetic.Latitude));
        }

    }
}