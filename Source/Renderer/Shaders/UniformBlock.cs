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
    public class UniformBlock
    {
        internal UniformBlock(
            string name,
            int sizeInBytes)
        {
            _name = name;
            _sizeInBytes = sizeInBytes;
            _members = new UniformBlockMemberCollection();
        }

        public string Name
        {
            get { return _name; }
        }

        public int SizeInBytes
        {
            get { return _sizeInBytes; }
        }

        public UniformBlockMemberCollection Members
        {
            get { return _members; }
        }

        public virtual void Bind(UniformBuffer uniformBuffer)
        {
        }

        private string _name;
        private int _sizeInBytes;
        private UniformBlockMemberCollection _members;
    }
}
