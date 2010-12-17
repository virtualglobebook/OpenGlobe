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
    public class UniformBlockMember
    {
        internal UniformBlockMember(
            string name,
            UniformType type,
            int offsetInBytes)
        {
            _name = name;
            _type = type;
            _offsetInBytes = offsetInBytes;
        }

        public string Name
        {
            get { return _name; }
        }

        public UniformType Datatype
        {
            get { return _type; }
        }

        public int OffsetInBytes
        {
            get { return _offsetInBytes; }
        }

        private string _name;
        private UniformType _type;
        private int _offsetInBytes;
    }
}
