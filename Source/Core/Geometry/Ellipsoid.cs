#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;
using System.Globalization;
using System.Collections.Generic;

namespace OpenGlobe.Core
{
    public class Ellipsoid
    {
        public static readonly Ellipsoid Wgs84 = new Ellipsoid(6378137.0, 6378137.0, 6356752.314245);
        public static readonly Ellipsoid ScaledWgs84 = new Ellipsoid(1.0, 1.0, 6356752.314245 / 6378137.0);
        public static readonly Ellipsoid UnitSphere = new Ellipsoid(1.0, 1.0, 1.0);

        public Ellipsoid(double x, double y, double z)
            : this(new Vector3D(x, y, z))
        {
        }

        public Ellipsoid(Vector3D radii)
        {
            if ((radii.X <= 0.0) || (radii.Y <= 0.0) || (radii.Z <= 0.0))
            {
                throw new ArgumentOutOfRangeException("radii");
            }

            _radii = radii;
            _radiiSquared = new Vector3D(
                radii.X * radii.X,
                radii.Y * radii.Y,
                radii.Z * radii.Z);
            _radiiToTheFourth = new Vector3D(
                _radiiSquared.X * _radiiSquared.X,
                _radiiSquared.Y * _radiiSquared.Y,
                _radiiSquared.Z * _radiiSquared.Z);
            _oneOverRadiiSquared = new Vector3D(
                1.0 / (radii.X * radii.X), 
                1.0 / (radii.Y * radii.Y), 
                1.0 / (radii.Z * radii.Z));
        }

        public static Vector3D CentricSurfaceNormal(Vector3D positionOnEllipsoid)
        {
            return positionOnEllipsoid.Normalize();
        }

        public Vector3D GeodeticSurfaceNormal(Vector3D positionOnEllipsoid)
        {
            return (positionOnEllipsoid.MultiplyComponents(_oneOverRadiiSquared)).Normalize();
        }

        public Vector3D GeodeticSurfaceNormal(Geodetic3D geodetic)
        {
            double cosLatitude = Math.Cos(geodetic.Latitude);

            return new Vector3D(
                cosLatitude * Math.Cos(geodetic.Longitude),
                cosLatitude * Math.Sin(geodetic.Longitude),
                Math.Sin(geodetic.Latitude));
        }

        public Vector3D Radii 
        {
            get { return _radii; }
        }

        public Vector3D RadiiSquared
        {
            get { return _radiiSquared; }
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

            double t = -0.5 * (b + (b > 0.0 ? 1.0 : -1.0) * Math.Sqrt(discriminant));
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

        public Vector3D ToVector3D(Geodetic2D geodetic)
        {
            return ToVector3D(new Geodetic3D(geodetic.Longitude, geodetic.Latitude, 0.0));
        }

        public Vector3D ToVector3D(Geodetic3D geodetic)
        {
            Vector3D n = GeodeticSurfaceNormal(geodetic);
            Vector3D k = _radiiSquared.MultiplyComponents(n);
            double gamma = Math.Sqrt(
                (k.X * n.X) +
                (k.Y * n.Y) +
                (k.Z * n.Z));

            Vector3D rSurface = k / gamma;
            return rSurface + (geodetic.Height * n);
        }

        public ICollection<Geodetic3D> ToGeodetic3D(IEnumerable<Vector3D> positions)
        {
            if (positions == null)
            {
                throw new ArgumentNullException("positions");
            }

            IList<Geodetic3D> geodetics = new List<Geodetic3D>(CollectionAlgorithms.EnumerableCount(positions));

            foreach (Vector3D position in positions)
            {
                geodetics.Add(ToGeodetic3D(position));
            }

            return geodetics;
        }

        public ICollection<Geodetic2D> ToGeodetic2D(IEnumerable<Vector3D> positions)
        {
            if (positions == null)
            {
                throw new ArgumentNullException("positions");
            }

            IList<Geodetic2D> geodetics = new List<Geodetic2D>(CollectionAlgorithms.EnumerableCount(positions));

            foreach (Vector3D position in positions)
            {
                geodetics.Add(ToGeodetic2D(position));
            }

            return geodetics;
        }

        public Geodetic2D ToGeodetic2D(Vector3D positionOnEllipsoid)
        {
            Vector3D n = GeodeticSurfaceNormal(positionOnEllipsoid);
            return new Geodetic2D(
                Math.Atan2(n.Y, n.X),
                Math.Asin(n.Z / n.Magnitude));
        }

        public Geodetic3D ToGeodetic3D(Vector3D position)
        {
            Vector3D p = ScaleToGeodeticSurface(position);
            Vector3D h = position - p;
            double height = Math.Sign(h.Dot(position)) * h.Magnitude;
            return new Geodetic3D(ToGeodetic2D(p), height);
        }

        public Vector3D ScaleToGeodeticSurface(Vector3D position)
        {
            double beta = 1.0 / Math.Sqrt(
                (position.X * position.X) * _oneOverRadiiSquared.X +
                (position.Y * position.Y) * _oneOverRadiiSquared.Y +
                (position.Z * position.Z) * _oneOverRadiiSquared.Z);
            double n = new Vector3D(
                beta * position.X * _oneOverRadiiSquared.X,
                beta * position.Y * _oneOverRadiiSquared.Y,
                beta * position.Z * _oneOverRadiiSquared.Z).Magnitude;
            double alpha = (1.0 - beta) * (position.Magnitude / n);

            double x2 = position.X * position.X;
            double y2 = position.Y * position.Y;
            double z2 = position.Z * position.Z;

            double da = 0.0;
            double db = 0.0;
            double dc = 0.0;

            double s = 0.0;
            double dSdA = 1.0;

            do
            {
                alpha -= (s / dSdA);

                da = 1.0 + (alpha * _oneOverRadiiSquared.X);
                db = 1.0 + (alpha * _oneOverRadiiSquared.Y);
                dc = 1.0 + (alpha * _oneOverRadiiSquared.Z);

                double da2 = da * da;
                double db2 = db * db;
                double dc2 = dc * dc;

                double da3 = da * da2;
                double db3 = db * db2;
                double dc3 = dc * dc2;

                s = x2 / (_radiiSquared.X * da2) +
                    y2 / (_radiiSquared.Y * db2) +
                    z2 / (_radiiSquared.Z * dc2) - 1.0;

                dSdA = -2.0 *
                    (x2 / (_radiiToTheFourth.X * da3) +
                     y2 / (_radiiToTheFourth.Y * db3) +
                     z2 / (_radiiToTheFourth.Z * dc3));
            }
            while (Math.Abs(s) > 1e-10);

            return new Vector3D(
                position.X / da,
                position.Y / db,
                position.Z / dc);
        }

        public Vector3D ScaleToGeocentricSurface(Vector3D position)
        {
            double beta = 1.0 / Math.Sqrt(
                (position.X * position.X) * _oneOverRadiiSquared.X +
                (position.Y * position.Y) * _oneOverRadiiSquared.Y +
                (position.Z * position.Z) * _oneOverRadiiSquared.Z);

            return beta * position;
        }

        public IList<Vector3D> ComputeCurve(
            Vector3D start, 
            Vector3D stop, 
            double granularity)
        {
            if (granularity <= 0.0)
            {
                throw new ArgumentOutOfRangeException("granularity", "Granularity must be greater than zero.");
            }

            Vector3D normal = start.Cross(stop).Normalize();
            double theta = start.AngleBetween(stop);
            int n = Math.Max((int)(theta / granularity) - 1, 0);
            
            IList<Vector3D> positions = new List<Vector3D>(2 + n);

            positions.Add(start);

            for (int i = 1; i <= n; ++i)
            {
                double phi = (i * granularity);

                positions.Add(ScaleToGeocentricSurface(start.RotateAroundAxis(normal, phi)));
            }

            positions.Add(stop);

            return positions;
        }

        private readonly Vector3D _radii;
        private readonly Vector3D _radiiSquared;
        private readonly Vector3D _radiiToTheFourth;
        private readonly Vector3D _oneOverRadiiSquared;
    }
}
