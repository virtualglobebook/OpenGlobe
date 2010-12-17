#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

namespace OpenGlobe.Core
{
    public static class ContainmentTests
    {
        public static bool PointInsideTriangle(Vector2D point, Vector2D p0, Vector2D p1, Vector2D p2)
        {
            //
            // Implementation based on http://www.blackpawn.com/texts/pointinpoly/default.html.
            //
            Vector2D v0 = (p1 - p0);
            Vector2D v1 = (p2 - p0);
            Vector2D v2 = (point - p0);

            double dot00 = v0.Dot(v0);
            double dot01 = v0.Dot(v1);
            double dot02 = v0.Dot(v2);
            double dot11 = v1.Dot(v1);
            double dot12 = v1.Dot(v2);

            double q = 1.0 / (dot00 * dot11 - dot01 * dot01);
            double u = (dot11 * dot02 - dot01 * dot12) * q;
            double v = (dot00 * dot12 - dot01 * dot02) * q;

            return (u > 0) && (v > 0) && (u + v < 1);
        }

        /// <summary>
        /// The pyramid's base points should be in counterclockwise winding order.
        /// </summary>
        public static bool PointInsideThreeSidedInfinitePyramid(
            Vector3D point,
            Vector3D pyramidApex,
            Vector3D pyramidBase0,
            Vector3D pyramidBase1,
            Vector3D pyramidBase2)
        {
            Vector3D v0 = pyramidBase0 - pyramidApex;
            Vector3D v1 = pyramidBase1 - pyramidApex;
            Vector3D v2 = pyramidBase2 - pyramidApex;

            //
            // Face normals
            //
            Vector3D n0 = v1.Cross(v0);
            Vector3D n1 = v2.Cross(v1);
            Vector3D n2 = v0.Cross(v2);

            Vector3D planeToPoint = point - pyramidApex;

            return ((planeToPoint.Dot(n0) < 0) && (planeToPoint.Dot(n1) < 0) && (planeToPoint.Dot(n2) < 0));
        }
    }
}