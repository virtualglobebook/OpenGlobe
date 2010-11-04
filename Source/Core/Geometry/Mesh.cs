#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System.Collections.Generic;

namespace OpenGlobe.Core
{
    public enum PrimitiveType
    {
        Points,
        Lines,
        LineLoop,
        LineStrip,
        Triangles,
        TriangleStrip,
        TriangleFan,
        LinesAdjacency,
        LineStripAdjacency,
        TrianglesAdjacency,
        TriangleStripAdjacency
    }

    public enum WindingOrder
    {
        Clockwise,
        Counterclockwise
    }

    public class Mesh
    {
        public Mesh()
        {
            _attributes = new VertexAttributeCollection();
        }

        public VertexAttributeCollection Attributes
        {
            get { return _attributes; }
        }

        public IndicesBase Indices { get; set; }

        public PrimitiveType PrimitiveType { get; set; }
        public WindingOrder FrontFaceWindingOrder { get; set; }

        private VertexAttributeCollection _attributes;
    }
}
