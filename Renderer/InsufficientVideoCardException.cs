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

namespace MiniGlobe.Renderer
{
    [Serializable]
    public class InsufficientVideoCardException : Exception
    {
        public InsufficientVideoCardException()
        {
        }

        public InsufficientVideoCardException(string message)
            : base(message)
        {
        }

        public InsufficientVideoCardException(string message, Exception inner)
            : base(message, inner) 
        {
        }

        protected InsufficientVideoCardException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
