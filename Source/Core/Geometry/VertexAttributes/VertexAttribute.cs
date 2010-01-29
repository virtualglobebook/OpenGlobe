#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using OpenTK;
using System.Collections.Generic;

namespace MiniGlobe.Core.Geometry
{
    // TODO:  These are the common types but we may need to add more.
    public enum VertexAttributeType
    {
        HalfFloat,
        HalfFloatVector2,
        HalfFloatVector3,
        HalfFloatVector4,
        Float,
        FloatVector2,
        FloatVector3,
        FloatVector4,
        Double,
        DoubleVector2,
        DoubleVector3,
        DoubleVector4
    }

    public abstract class VertexAttribute
    {
        protected VertexAttribute(string name, VertexAttributeType type)
        {
            _name = name;
            _type = type;
        }

        public string Name
        {
            get { return _name; }
        }

        public VertexAttributeType DataType
        {
            get { return _type; }
        }

        private string _name;
        private VertexAttributeType _type;
    }

    public class VertexAttribute<T> : VertexAttribute
    {
        protected VertexAttribute(string name, VertexAttributeType type)
            : base(name, type)
        {
            _values = new List<T>();
        }

        protected VertexAttribute(string name, VertexAttributeType type, int capacity)
            : base(name, type)
        {
            _values = new List<T>(capacity);
        }

        public IList<T> Values
        {
            get { return _values; }
        }

        private List<T> _values;
    }
}
