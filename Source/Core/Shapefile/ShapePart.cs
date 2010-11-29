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
    public class ShapePart
    {
        internal ShapePart(Vector2D[] positions, int offset, int count)
        {
            _positions = new Vector2D[count];
            
            for (int i = 0; i < count; ++i)
            {
                _positions[i] = positions[offset + i];
            }
        }

        public Vector2D this[int index]
        {
            get { return _positions[index]; }
        }

        public int Count
        {
            get { return _positions.Length; }
        }

        public IEnumerator GetEnumerator()
        {
            return _positions.GetEnumerator();
        }

        private readonly Vector2D[] _positions;
    }
}
