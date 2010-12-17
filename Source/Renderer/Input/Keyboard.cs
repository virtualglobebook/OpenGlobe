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
