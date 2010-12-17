#region License
//
// (C) Copyright 2010 Patrick Cozzi, Deron Ohlarik, and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
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
