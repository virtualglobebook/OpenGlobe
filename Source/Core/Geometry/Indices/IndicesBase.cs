#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

namespace OpenGlobe.Core
{
    public abstract class IndicesBase
    {
        protected IndicesBase(IndicesType type)
        {
            _type = type;
        }

        public IndicesType Datatype
        {
            get { return _type; }
        }

        private IndicesType _type;
    }
}
