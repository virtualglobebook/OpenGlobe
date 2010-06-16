#region License
//
// (C) Copyright 2010 Patrick Cozzi, Deron Ohlarik, and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;

namespace OpenGlobe.Renderer
{
    public class KeyboardKeyEventArgs : EventArgs
    {
        public KeyboardKeyEventArgs(KeyboardKeyEvent keyboardEvent, KeyboardKey key)
        {
            _keyboardEvent = keyboardEvent;
            _key = key;
        }

        public KeyboardKeyEvent KeyboardEvent
        {
            get { return _keyboardEvent; }
        }

        public KeyboardKey Key
        {
            get { return _key; }
        }

        private KeyboardKeyEvent _keyboardEvent;
        private KeyboardKey _key;
    }
}
