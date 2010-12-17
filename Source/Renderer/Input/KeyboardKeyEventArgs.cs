#region License
//
// (C) Copyright 2010 Patrick Cozzi, Deron Ohlarik, and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
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
