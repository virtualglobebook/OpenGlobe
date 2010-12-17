#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

namespace OpenGlobe.Core
{
    public class VertexAttributeFloat : VertexAttribute<float>
    {
        public VertexAttributeFloat(string name)
            : base(name, VertexAttributeType.Float)
        {
        }

        public VertexAttributeFloat(string name, int capacity)
            : base(name, VertexAttributeType.Float, capacity)
        {
        }
    }
}
