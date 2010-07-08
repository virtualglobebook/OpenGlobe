#region License
//
// (C) Copyright 2010 Patrick Cozzi, Deron Ohlarik, and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;

namespace OpenGlobe.Core
{
    public class MessageQueueEventArgs : EventArgs
    {
        public MessageQueueEventArgs(object message)
        {
            _message = message;
        }

        public object Message
        {
            get { return _message; }
        }

        private object _message;
    }
}
