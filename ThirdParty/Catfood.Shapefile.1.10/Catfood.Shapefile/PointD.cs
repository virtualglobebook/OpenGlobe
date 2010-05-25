/* ------------------------------------------------------------------------
 * (c)copyright 2009 Catfood Software - http://www.catfood.net
 * Provided under the ms-PL license, see LICENSE.txt
 * ------------------------------------------------------------------------ */

using System;
using System.Collections.Generic;
using System.Text;


namespace Catfood.Shapefile
{
    /// <summary>
    /// A simple double precision point
    /// </summary>
    public struct PointD
    {
        /// <summary>
        /// Gets or sets the X value
        /// </summary>
        public double X;

        /// <summary>
        /// Gets or sets the Y value
        /// </summary>
        public double Y;

        /// <summary>
        /// A simple double precision point
        /// </summary>
        /// <param name="x">X value</param>
        /// <param name="y">Y value</param>
        public PointD(double x, double y)
        {
            X = x;
            Y = y;
        }
    }
}
