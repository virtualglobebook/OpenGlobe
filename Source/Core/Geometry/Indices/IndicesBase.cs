#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

namespace OpenGlobe.Core.Geometry
{
    public enum IndicesType
    {
        Byte,
        Int16,
        Int32
    }

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
