#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System.Collections;

namespace OpenGlobe.Core
{
    public class PolygonShape : Shape
    {
        internal PolygonShape(
            int recordNumber,
            RectangleD extent,
            int[] parts,
            Vector2D[] positions)
            : base(recordNumber, ShapeType.Polygon)
        {
            _extent = extent;

            _parts = new ShapePart[parts.Length];
            for (int i = 0; i < parts.Length; ++i)
            {
                int count = ((i == parts.Length - 1) ?
                    positions.Length : parts[i + 1]) - parts[i];

                _parts[i] = new ShapePart(positions, parts[i], count);
            }
        }

        public RectangleD Extent
        {
            get { return _extent; }
        }

        public ShapePart this[int index]
        {
            get { return _parts[index]; }
        }

        public int Count
        {
            get { return _parts.Length; }
        }

        public IEnumerator GetEnumerator()
        {
            return _parts.GetEnumerator();
        }

        private readonly RectangleD _extent;
        private readonly ShapePart[] _parts;
    }
}
