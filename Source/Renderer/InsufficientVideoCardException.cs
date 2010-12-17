#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;
using System.Runtime.Serialization;

namespace OpenGlobe.Renderer
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
