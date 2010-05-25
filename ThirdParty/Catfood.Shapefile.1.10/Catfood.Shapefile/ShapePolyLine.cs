/* ------------------------------------------------------------------------
 * (c)copyright 2009 Catfood Software - http://www.catfood.net
 * Provided under the ms-PL license, see LICENSE.txt
 * ------------------------------------------------------------------------ */

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace Catfood.Shapefile
{
    /// <summary>
    /// A Shapefile PolyLine  Shape
    /// </summary>
    public class ShapePolyLine : Shape
    {
        private RectangleD _boundingBox;
        private List<PointD[]> _parts;

        /// <summary>
        /// A Shapefile PolyLine Shape
        /// </summary>
        /// <param name="recordNumber">The record number in the Shapefile</param>
        /// <param name="metadata">Metadata about the shape</param>
        /// <param name="shapeData">The shape record as a byte array</param>
        /// <exception cref="ArgumentNullException">Thrown if shapeData is null</exception>
        /// <exception cref="InvalidOperationException">Thrown if an error occurs parsing shapeData</exception>
        protected internal ShapePolyLine(int recordNumber, StringDictionary metadata, byte[] shapeData)
            : base(ShapeType.PolyLine, recordNumber, metadata)
        {
            ParsePolyLineOrPolygon(shapeData, out _boundingBox, out _parts);
        }

        /// <summary>
        /// Gets the bounding box
        /// </summary>
        public RectangleD BoundingBox
        {
            get { return _boundingBox; }
        }
        
        /// <summary>
        /// Gets a list of parts (segments) for the PolyLine. Each part
        /// is an array of double precision points
        /// </summary>
        public List<PointD[]> Parts
        {
            get { return _parts; }
        }
    }
}
