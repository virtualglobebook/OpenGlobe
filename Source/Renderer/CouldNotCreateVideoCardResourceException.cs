#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Runtime.Serialization;

namespace OpenGlobe.Renderer
{
    [Serializable]
    public class CouldNotCreateVideoCardResourceException : Exception
    {
        public CouldNotCreateVideoCardResourceException()
        {
        }

        public CouldNotCreateVideoCardResourceException(string message)
            : base(message)
        {
        }

        public CouldNotCreateVideoCardResourceException(string message, Exception inner)
            : base(message, inner) 
        {
        }

        protected CouldNotCreateVideoCardResourceException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
