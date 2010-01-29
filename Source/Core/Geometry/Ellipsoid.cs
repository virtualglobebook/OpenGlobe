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

        private readonly Vector3d _radii;
        private readonly Vector3d _oneOverRadiiSquared;
    }
}
