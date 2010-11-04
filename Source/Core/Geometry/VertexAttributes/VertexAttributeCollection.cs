#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace OpenGlobe.Core
{
    public class VertexAttributeCollection : ICollection<VertexAttribute>
    {
        public void Add(VertexAttribute vertexAttribute)
        {
            m_collection.Add(vertexAttribute.Name, vertexAttribute);
        }

        public void Clear()
        {
            m_collection.Clear();
        }

        public bool Contains(VertexAttribute vertexAttribute)
        {
            return Contains(vertexAttribute.Name);
        }

        public bool Contains(string vertexAttributeName)
        {
            return m_collection.ContainsKey(vertexAttributeName);
        }

        public void CopyTo(VertexAttribute[] array, int arrayIndex)
        {
            ICollection<VertexAttribute> values = m_collection.Values;
            values.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return m_collection.Count; }
        }

        bool ICollection<VertexAttribute>.IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(VertexAttribute item)
        {
            return Remove(item.Name);
        }

        public bool Remove(string vertexAttributeName)
        {
            return m_collection.Remove(vertexAttributeName);
        }

        public IEnumerator<VertexAttribute> GetEnumerator()
        {
            ICollection<VertexAttribute> values = m_collection.Values;
            return values.GetEnumerator();
        }

#if !CSToJava
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
#endif

        public VertexAttribute this[string vertexAttributeName]
        {
            get { return m_collection[vertexAttributeName]; }
        }

        private Dictionary<string, VertexAttribute> m_collection = new Dictionary<string, VertexAttribute>();
    }
}
