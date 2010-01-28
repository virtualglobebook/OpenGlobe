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

        public UniformType DataType
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
