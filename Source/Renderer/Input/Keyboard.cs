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
    public abstract class Keyboard
    {
        public event EventHandler<KeyboardKeyEventArgs> KeyDown;
        public event EventHandler<KeyboardKeyEventArgs> KeyUp;

        protected virtual void OnKeyDown(KeyboardKey key)
        {
            EventHandler<KeyboardKeyEventArgs> handler = KeyDown;
            if (handler != null)
            {
                handler(this, new KeyboardKeyEventArgs(KeyboardKeyEvent.Down, key));
            }
        }

        protected virtual void OnKeyUp(KeyboardKey key)
        {
            EventHandler<KeyboardKeyEventArgs> handler = KeyUp;
            if (handler != null)
            {
                handler(this, new KeyboardKeyEventArgs(KeyboardKeyEvent.Up, key));
            }
        }
    }
}
