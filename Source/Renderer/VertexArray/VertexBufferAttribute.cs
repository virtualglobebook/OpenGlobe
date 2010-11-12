#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
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
    public class VertexBufferAttribute
    {
        public VertexBufferAttribute(
            VertexBuffer vertexBuffer,
            ComponentDatatype componentDatatype,
            int numberOfComponents)
            : this(vertexBuffer, componentDatatype, numberOfComponents, false, 0, 0)
        {
        }

        public VertexBufferAttribute(
            VertexBuffer vertexBuffer,
            ComponentDatatype componentDatatype,
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
            _componentDatatype = componentDatatype;
            _numberOfComponents = numberOfComponents;
            _normalize = normalize;
            _offsetInBytes = offsetInBytes;

            if (strideInBytes == 0)
            {
                //
                // Tightly packed
                //
                _strideInBytes = numberOfComponents * VertexArraySizes.SizeOf(componentDatatype);
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

        public ComponentDatatype ComponentDatatype
        {
            get { return _componentDatatype; }
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

        private VertexBuffer _vertexBuffer;
        private ComponentDatatype _componentDatatype;
        private int _numberOfComponents;
        private bool _normalize;
        private int _offsetInBytes;
        private int _strideInBytes;
    }
}
