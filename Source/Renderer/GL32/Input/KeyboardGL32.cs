#region License
//
// (C) Copyright 2010 Patrick Cozzi, Deron Ohlarik, and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using OpenTK.Input;

namespace MiniGlobe.Renderer
{
    public class KeyboardGL32 : Keyboard
    {
        public KeyboardGL32(KeyboardDevice keyboard)
        {
            _keyboard = keyboard;
        }

        private KeyboardDevice _keyboard;
    }
}
