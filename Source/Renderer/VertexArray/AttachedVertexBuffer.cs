#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System.Diagnostics;

namespace MiniGlobe.Renderer
{
    public class AttachedVertexBuffer
    {
        public AttachedVertexBuffer(
            VertexBuffer vertexBuffer,
            VertexAttributeComponentType componentType,
            int numberOfComponents)
            : this(vertexBuffer, componentType, numberOfComponents, false)
        {
        }

        public AttachedVertexBuffer(
            VertexBuffer vertexBuffer,
            VertexAttributeComponentType componentType,
            int numberOfComponents,
            bool normalize)
        {
            _vertexBuffer = vertexBuffer;
            _componentType = componentType;
            _numberOfComponents = numberOfComponents;
            _normalize = normalize;
        }

        public VertexBuffer VertexBuffer
        {
            get { return _vertexBuffer; }
        }

        public VertexAttributeComponentType ComponentType
        {
            get { return _componentType; }
        }

        public int NumberOfComponents
        {
            get { return _numberOfComponents; }
        }

        public bool Normalize
        {
            get { return _normalize; }
        }

        private VertexBuffer _vertexBuffer;
        private VertexAttributeComponentType _componentType;
        private int _numberOfComponents;
        private bool _normalize;
    }
}
