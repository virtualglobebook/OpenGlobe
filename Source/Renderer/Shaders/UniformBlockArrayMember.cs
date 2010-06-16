#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

namespace OpenGlobe.Renderer
{
    public class UniformBlockArrayMember : UniformBlockMember
    {
        internal UniformBlockArrayMember(
            string name,
            UniformType type,
            int offsetInBytes,
            int length,
            int elementStrideInBytes)
            : base(name, type, offsetInBytes)
        {
            _length = length;
            _elementStrideInBytes = elementStrideInBytes;
        }

        public int Length
        {
            get { return _length; }
        }

        public int ElementStrideInBytes
        {
            get { return _elementStrideInBytes; }
        }

        private int _length;
        private int _elementStrideInBytes;
    }
}
