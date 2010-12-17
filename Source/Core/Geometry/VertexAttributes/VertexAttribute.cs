#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System.Collections.Generic;

namespace OpenGlobe.Core
{
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

        public VertexAttributeType Datatype
        {
            get { return _type; }
        }

        private readonly string _name;
        private readonly VertexAttributeType _type;
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

        private readonly List<T> _values;
    }
}
