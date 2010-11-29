#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

namespace OpenGlobe.Core
{
    public abstract class Shape
    {
        protected Shape(int recordNumber, ShapeType shapeType)
        {
            _recordNumber = recordNumber;
            _shapeType = shapeType;
        }

        public int RecordNumber
        { 
            get { return _recordNumber; }
        }

        public ShapeType ShapeType 
        { 
            get { return _shapeType; }
        }

        private readonly int _recordNumber;
        private readonly ShapeType _shapeType;
    }
}
