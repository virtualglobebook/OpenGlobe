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
    public class VertexAttributeEmulatedDoubleVector3 : VertexAttribute<Vector3D>
    {
        public VertexAttributeEmulatedDoubleVector3(string highName, string lowName)
            : base(highName + " " + lowName, VertexAttributeType.EmulatedDoubleVector3)
        {
            _highName = highName;
            _lowName = lowName;
        }

        public VertexAttributeEmulatedDoubleVector3(string highName, string lowName, int capacity)
            : base(highName + " " + lowName, VertexAttributeType.EmulatedDoubleVector3, capacity)
        {
            _highName = highName;
            _lowName = lowName;
        }

        public string HighName
        {
            get { return _highName; }
        }

        public string LowName
        {
            get { return _lowName; }
        }

        private readonly string _highName;
        private readonly string _lowName;
    }
}
