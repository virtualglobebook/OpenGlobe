#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System.Diagnostics;
using OpenGlobe.Core;
using System;

namespace OpenGlobe.Renderer
{
    public class AttachedVertexBuffer : Disposable
    {
        public AttachedVertexBuffer(
            VertexBuffer vertexBuffer,
            VertexAttributeComponentType componentType,
            int numberOfComponents)
            : this(vertexBuffer, componentType, numberOfComponents, false, 0, 0)
        {
        }

        public AttachedVertexBuffer(
            VertexBuffer vertexBuffer,
            VertexAttributeComponentType componentType,
            int numberOfComponents,
            bool normalize,
            int offsetInBytes,
            int strideInBytes)
        {
            if (numberOfComponents <= 0)
            {
                throw new ArgumentOutOfRangeException("numberOfComponents", "numberOfComponents must be greater than zero.");
            }

            if (offsetInBytes < 0)
            {
                throw new ArgumentOutOfRangeException("offsetInBytes", "offsetInBytes must be greater than or equal to zero.");
            }

            if (strideInBytes < 0)
            {
                throw new ArgumentOutOfRangeException("stride", "stride must be greater than or equal to zero.");
            }

            _vertexBuffer = vertexBuffer;
            _componentType = componentType;
            _numberOfComponents = numberOfComponents;
            _normalize = normalize;
            _offsetInBytes = offsetInBytes;

            if (strideInBytes == 0)
            {
                //
                // Tightly packed
                //
                _strideInBytes = numberOfComponents * VertexArraySizes.SizeOf(componentType);
            }
            else
            {
                _strideInBytes = strideInBytes;
            }
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

        public int OffsetInBytes
        {
            get { return _offsetInBytes; }
        }

        public int StrideInBytes
        {
            get { return _strideInBytes; }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_vertexBuffer != null)
                {
                    _vertexBuffer.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        private VertexBuffer _vertexBuffer;
        private VertexAttributeComponentType _componentType;
        private int _numberOfComponents;
        private bool _normalize;
        private int _offsetInBytes;
        private int _strideInBytes;
    }
}
