#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

namespace MiniGlobe.Renderer
{
    public enum ShaderVertexAttributeType
    {
        Float,
        FloatVector2,
        FloatVector3,
        FloatVector4,
        FloatMatrix22,
        FloatMatrix33,
        FloatMatrix44,
    }

    public class ShaderVertexAttribute
    {
        internal ShaderVertexAttribute(
            string name, 
            int location,
            ShaderVertexAttributeType type,
            int length)
        {
            _name = name;
            _location = location;
            _type = type;
            _length = length;
        }

        public string Name
        {
            get { return _name; }
        }

        public int Location
        {
            get { return _location; }
        }

        public ShaderVertexAttributeType DataType
        {
            get { return _type; }
        }

        public int Length
        {
            get { return _length; }
        }

        private string _name;
        private int _location;
        private ShaderVertexAttributeType _type;
        private int _length;                            // TODO:  Array type
    }
}
