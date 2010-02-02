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
using OpenTK;

namespace MiniGlobe.Core.Geometry
{
    public class Ellipsoid
    {
        public static readonly Ellipsoid Wgs84 = new Ellipsoid(6378137.0, 6378137.0, 6356752.314245);
        public static readonly Ellipsoid UnitSphere = new Ellipsoid(1, 1, 1);

        public Ellipsoid(double x, double y, double z)
            : this(new Vector3d(x, y, z))
        {
        }

        public Ellipsoid(Vector3d radii)
        {
            if ((radii.X <= 0) || (radii.Y <= 0) || (radii.Z <= 0))
            {
                throw new ArgumentOutOfRangeException("radii");
            }

            _radii = radii;
            _oneOverRadiiSquared = new Vector3d(
                1.0 / (radii.X * radii.X), 
                1.0 / (radii.Y * radii.Y), 
                1.0 / (radii.Z * radii.Z));
        }

        public Vector3d DeticSurfaceNormal(Vector3d positionOnEllipsoid)
        {
            return Vector3d.Normalize(Vector3d.Multiply(positionOnEllipsoid, _oneOverRadiiSquared));
        }

        public Vector3d Radii 
        {
            get { return _radii; }
        }

        public double MinimumRadius
        {
            get { return Math.Min(_radii.X, Math.Min(_radii.Y, _radii.Z)); }
        }

        public double MaximumRadius
        {
            get { return Math.Max(_radii.X, Math.Max(_radii.Y, _radii.Z)); }
        }

        public double[] Intersections(Vector3d origin, Vector3d direction)
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
                return new double[0]; // no intersections
            else if (discriminant == 0.0)
                return new double[1] { -0.5 * b / a }; // one intersection at a tangent point

            double t = -0.5 * (b + (b > 0 ? 1.0 : -1.0) * Math.Sqrt(discriminant));
            double root1 = t / a;
            double root2 = c / t;

            // Two intersections - return the smallest first.
            if (root1 < root2)
                return new double[2] { root1, root2 };
            else
                return new double[2] { root2, root1 };
        }

        private readonly Vector3d _radii;
        private readonly Vector3d _oneOverRadiiSquared;
    }
}
