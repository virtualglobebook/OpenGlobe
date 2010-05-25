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
    /// A simple double precision rectangle
    /// </summary>
    public struct RectangleD
    {
        /// <summary>
        /// Gets or sets the left value
        /// </summary>
        public double Left;

        /// <summary>
        /// Gets or sets the top value
        /// </summary>
        public double Top;

        /// <summary>
        /// Gets or sets the right value
        /// </summary>
        public double Right;

        /// <summary>
        /// Gets or sets the bottom value
        /// </summary>
        public double Bottom;

        /// <summary>
        /// A simple double precision rectangle
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="top">Top</param>
        /// <param name="right">Right</param>
        /// <param name="bottom">Bottom</param>
        public RectangleD(double left, double top, double right, double bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }
    }
}
