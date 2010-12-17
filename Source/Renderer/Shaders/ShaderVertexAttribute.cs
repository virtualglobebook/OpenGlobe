#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

namespace OpenGlobe.Renderer
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
        Int,
        IntVector2,
        IntVector3,
        IntVector4
    }

    public class ShaderVertexAttribute
    {
        public ShaderVertexAttribute(
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

        public ShaderVertexAttributeType Datatype
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
