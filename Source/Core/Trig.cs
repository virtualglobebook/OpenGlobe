#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;

namespace MiniGlobe.Core
{
    public static class Trig
    {
        public const double TwoPI = 2.0 * Math.PI;
        public const double RadiansPerDegree = Math.PI / 180.0;

        public static double DegreesToRadians(double degrees)
        {
            return degrees * RadiansPerDegree;
        }
    }
}