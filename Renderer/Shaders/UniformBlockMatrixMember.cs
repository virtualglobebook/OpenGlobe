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
    public class UniformBlockMatrixMember : UniformBlockMember
    {
        internal UniformBlockMatrixMember(
            string name,
            UniformType type,
            int offsetInBytes,
            int strideInBytes,
            bool rowMajor)
            : base(name, type, offsetInBytes)
        {
            _strideInBytes = strideInBytes;
            _rowMajor = rowMajor;
        }

        public int StrideInBytes
        {
            get { return _strideInBytes; }
        }

        public bool RowMajor
        {
            get { return _rowMajor; }
        }

        private int _strideInBytes;
        private bool _rowMajor;
    }
}
