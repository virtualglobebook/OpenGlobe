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
    public class UniformBlockMatrixArrayMember : UniformBlockMember
    {
        internal UniformBlockMatrixArrayMember(
            string name,
            UniformType type,
            int offsetInBytes,
            int length,
            int elementStrideInBytes,
            int matrixStrideInBytes,
            bool rowMajor)
            : base(name, type, offsetInBytes)
        {
            _length = length;
            _elementStrideInBytes = elementStrideInBytes;
            _matrixStrideInBytes = matrixStrideInBytes;
            _rowMajor = rowMajor;
        }

        public int Length
        {
            get { return _length; }
        }

        public int ElementStrideInBytes
        {
            get { return _elementStrideInBytes; }
        }

        public int MatrixStrideInBytes
        {
            get { return _matrixStrideInBytes; }
        }

        public bool RowMajor
        {
            get { return _rowMajor; }
        }

        private int _length;
        private int _elementStrideInBytes;
        private int _matrixStrideInBytes;
        private bool _rowMajor;
    }
}
