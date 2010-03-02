#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Globalization;

namespace MiniGlobe.Core.Geometry
{
    public class Ellipsoid
    {
        public static readonly Ellipsoid Wgs84 = new Ellipsoid(6378137.0, 6378137.0, 6356752.314245);
        public static readonly Ellipsoid UnitSphere = new Ellipsoid(1, 1, 1);

        public Ellipsoid(double x, double y, double z)
            : this(new Vector3D(x, y, z))
        {
        }

        public Ellipsoid(Vector3D radii)
        {
            if ((radii.X <= 0) || (radii.Y <= 0) || (radii.Z <= 0))
            {
                throw new ArgumentOutOfRangeException("radii");
            }

            _radii = radii;
            _oneOverRadii = new Vector3D(
                1.0 / radii.X,
                1.0 / radii.Y,
                1.0 / radii.Z);
            _oneOverRadiiSquared = new Vector3D(
                1.0 / (radii.X * radii.X), 
                1.0 / (radii.Y * radii.Y), 
                1.0 / (radii.Z * radii.Z));
        }

        public Vector3D DeticSurfaceNormal(Vector3D positionOnEllipsoid)
        {
            return (positionOnEllipsoid * _oneOverRadiiSquared).Normalize();
        }

        public Vector3D Radii 
        {
            get { return _radii; }
        }

        public Vector3D OneOverRadii
        {
            get { return _oneOverRadii; }
        }

        public Vector3D OneOverRadiiSquared
        {
            get { return _oneOverRadiiSquared; }
        }

        public double MinimumRadius
        {
            get { return Math.Min(_radii.X, Math.Min(_radii.Y, _radii.Z)); }
        }

        public double MaximumRadius
        {
            get { return Math.Max(_radii.X, Math.Max(_radii.Y, _radii.Z)); }
        }

        public double[] Intersections(Vector3D origin, Vector3D direction)
        {
            direction.Normalize();

            // By laborious algebraic manipulation....
            double a = direction.X * direction.X * _oneOverRadiiSquared.X +
                       direction.Y * direction.Y * _oneOverRadiiSquared.Y +
                       direction.Z * direction.Z * _oneOverRadiiSquared.Z;
            double b = 2.0 *
                       (origin.X * direction.X * _oneOverRadiiSquared.X +
                        origin.Y * direction.Y * _oneOverRadiiSquared.Y +
                        origin.Z * direction.Z * _oneOverRadiiSquared.Z);
            double c = origin.X * origin.X * _oneOverRadiiSquared.X +
                       origin.Y * origin.Y * _oneOverRadiiSquared.Y +
                       origin.Z * origin.Z * _oneOverRadiiSquared.Z - 1.0;

            // Solve the quadratic equation: ax^2 + bx + c = 0.
            // Algorithm is from Wikipedia's "Quadratic equation" topic, and Wikipedia credits
            // Numerical Recipes in C, section 5.6: "Quadratic and Cubic Equations"
            double discriminant = b * b - 4 * a * c;
            if (discriminant < 0.0)
            {
                // no intersections
                return new double[0]; 
            }
            else if (discriminant == 0.0)
            {
                // one intersection at a tangent point
                return new double[1] { -0.5 * b / a };
            }

            double t = -0.5 * (b + (b > 0 ? 1.0 : -1.0) * Math.Sqrt(discriminant));
            double root1 = t / a;
            double root2 = c / t;

            // Two intersections - return the smallest first.
            if (root1 < root2)
            {
                return new double[2] { root1, root2 };
            }
            else
            {
                return new double[2] { root2, root1 };
            }
        }

        public Vector3D DeticToVector3d(double longitude, double latitude, double height)
        {
            // Algorithm is from Wikipedia's "Geodetic System" topic.

            double cosLon = Math.Cos(longitude);
            double cosLat = Math.Cos(latitude);
            double sinLon = Math.Sin(longitude);
            double sinLat = Math.Sin(latitude);

            double major = MaximumRadius;
            double minor = MinimumRadius;
            double firstEccentricitySquared = 1.0 - (major * major) / (minor * minor);
            double chi = Math.Sqrt(1 - firstEccentricitySquared * cosLat * cosLat);
            double normal = major / chi;
            double normalPlusHeight = normal + height;

            double x = normalPlusHeight * cosLat * cosLon;
            double y = normalPlusHeight * cosLat * sinLon;
            double z = (normal * (1 - firstEccentricitySquared) + height) * sinLat;

            return new Vector3D(x, y, z);
        }

        private readonly Vector3D _radii;
        private readonly Vector3D _oneOverRadii;
        private readonly Vector3D _oneOverRadiiSquared;
    }
}
