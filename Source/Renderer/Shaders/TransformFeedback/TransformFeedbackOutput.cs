#region License
//
// (C) Copyright 2011 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

namespace OpenGlobe.Renderer
{
    public class TransformFeedbackOutput
    {
        internal TransformFeedbackOutput(
            string name,
            ShaderVertexAttributeType datatype,
            int numberOfComponents)
        {
            _name = name;
            _datatype = datatype;
            _numberOfComponents = numberOfComponents;
        }

        public string Name
        {
            get { return _name; }
        }

        public ShaderVertexAttributeType Datatype
        {
            get { return _datatype; }
        }

        public int NumberOfComponents
        {
            get { return _numberOfComponents; }
        }

        private readonly string _name;
        private readonly ShaderVertexAttributeType _datatype;
        private readonly int _numberOfComponents;
    }
}
