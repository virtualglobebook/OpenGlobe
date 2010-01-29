#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

namespace MiniGlobe.Core.Geometry
{
    public enum IndicesType
    {
        Byte,
        Short,
        Int
    }

    public abstract class IndicesBase
    {
        protected IndicesBase(IndicesType type)
        {
            _type = type;
        }

        public IndicesType DataType
        {
            get { return _type; }
        }

        private IndicesType _type;
    }
}
