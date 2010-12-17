#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

namespace OpenGlobe.Core
{
    public class PointShape : Shape
    {
        internal PointShape(int recordNumber, Vector2D position)
            : base(recordNumber, ShapeType.Point)
        {
            _position = position;
        }

        public Vector2D Position
        {
            get { return _position; }
        }

        private readonly Vector2D _position;
    }
}
