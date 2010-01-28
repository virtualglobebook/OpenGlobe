#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Diagnostics;
using OpenTK;

namespace MiniGlobe.Renderer
{
    public class Camera
    {
        public Camera()
        {
            Eye = -Vector3d.UnitY;
            Target = Vector3d.Zero;
            Up = Vector3d.UnitZ;

            FieldOfViewY = Math.PI / 6.0;
            AspectRatio = 1;

            NearPlaneDistance = 0.01;
            FarPlaneDistance = 64;
        }

        public Vector3d Eye { get; set; }
        public Vector3d Target { get; set; }
        public Vector3d Up { get; set; }

        public double FieldOfViewX
        {
            get { return (2.0 * Math.Atan(AspectRatio * Math.Tan(FieldOfViewY * 0.5))); }
        }
        public double FieldOfViewY { get; set; }
        public double AspectRatio { get; set; }

        public double NearPlaneDistance { get; set; }
        public double FarPlaneDistance { get; set; }

        public void ZoomToTarget(double radius)
        {
            Vector3d toEye = Vector3d.Normalize(Eye - Target);

            double sin = Math.Sin(Math.Min(FieldOfViewX, FieldOfViewY) * 0.5);
            double distance = (radius / sin);
            Eye = Target + (distance * toEye);
        }
    }
}
