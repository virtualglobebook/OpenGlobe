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
