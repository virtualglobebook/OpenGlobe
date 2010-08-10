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
    public static class IntersectionTests
    {
        public static bool TryRayPlane(
            Vector3D rayOrigin, 
            Vector3D rayDirection, 
            Vector3D planeNormal, 
            double planeD, 
            out Vector3D intersectionPoint)
        {
            double denominator = planeNormal.Dot(rayDirection);

            if (Math.Abs(denominator) < 0.00000000000000000001)
            {
                //
                // Ray is parallel to plane.  The ray may be in the polygon's plane.
                //
                intersectionPoint = Vector3D.Zero;
                return false;
            }

            double t = (-planeD - planeNormal.Dot(rayOrigin)) / denominator;

            if (t < 0)
            {
                intersectionPoint = Vector3D.Zero;
                return false;
            }

            intersectionPoint = rayOrigin + (t * rayDirection);
            return true;
        }
    }
}