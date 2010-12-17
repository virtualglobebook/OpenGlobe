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
