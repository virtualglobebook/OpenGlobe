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

namespace OpenGlobe.Renderer
{
    public class KeyboardGL3x : Keyboard
    {
        public KeyboardGL3x(KeyboardDevice keyboard)
        {
            _keyboard = keyboard;

            _keyboard.KeyDown += OpenTKKeyDown;
            _keyboard.KeyUp += OpenTKKeyUp;
        }

        private void OpenTKKeyUp(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            OnKeyUp(OpenTKToMiniGlobe(e.Key));
        }

        private void OpenTKKeyDown(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            OnKeyDown(OpenTKToMiniGlobe(e.Key));
        }

        private static KeyboardKey OpenTKToMiniGlobe(Key key)
        {
            switch (key)
            {
                case Key.ShiftLeft:
                    return KeyboardKey.ShiftLeft;
                case Key.ShiftRight:
                    return KeyboardKey.ShiftRight;
                case Key.ControlLeft:
                    return KeyboardKey.ControlLeft;
                case Key.ControlRight:
                    return KeyboardKey.ControlRight;
                case Key.AltLeft:
                    return KeyboardKey.AltLeft;
                case Key.AltRight:
                    return KeyboardKey.AltRight;
                case Key.WinLeft:
                    return KeyboardKey.WinLeft;
                case Key.WinRight:
                    return KeyboardKey.WinRight;
                case Key.Menu:
                    return KeyboardKey.Menu;
                case Key.F1:
                    return KeyboardKey.F1;
                case Key.F2:
                    return KeyboardKey.F2;
                case Key.F3:
                    return KeyboardKey.F3;
                case Key.F4:
                    return KeyboardKey.F4;
                case Key.F5:
                    return KeyboardKey.F5;
                case Key.F6:
                    return KeyboardKey.F6;
                case Key.F7:
                    return KeyboardKey.F7;
                case Key.F8:
                    return KeyboardKey.F8;
                case Key.F9:
                    return KeyboardKey.F9;
                case Key.F10:
                    return KeyboardKey.F10;
                case Key.F11:
                    return KeyboardKey.F11;
                case Key.F12:
                    return KeyboardKey.F12;
                case Key.F13:
                    return KeyboardKey.F13;
                case Key.F14:
                    return KeyboardKey.F14;
                case Key.F15:
                    return KeyboardKey.F15;
                case Key.F16:
                    return KeyboardKey.F16;
                case Key.F17:
                    return KeyboardKey.F17;
                case Key.F18:
                    return KeyboardKey.F18;
                case Key.F19:
                    return KeyboardKey.F19;
                case Key.F20:
                    return KeyboardKey.F20;
                case Key.F21:
                    return KeyboardKey.F21;
                case Key.F22:
                    return KeyboardKey.F22;
                case Key.F23:
                    return KeyboardKey.F23;
                case Key.F24:
                    return KeyboardKey.F24;
                case Key.F25:
                    return KeyboardKey.F25;
                case Key.F26:
                    return KeyboardKey.F26;
                case Key.F27:
                    return KeyboardKey.F27;
                case Key.F28:
                    return KeyboardKey.F28;
                case Key.F29:
                    return KeyboardKey.F29;
                case Key.F30:
                    return KeyboardKey.F30;
                case Key.F31:
                    return KeyboardKey.F31;
                case Key.F32:
                    return KeyboardKey.F32;
                case Key.F33:
                    return KeyboardKey.F33;
                case Key.F34:
                    return KeyboardKey.F34;
                case Key.F35:
                    return KeyboardKey.F35;
                case Key.Up:
                    return KeyboardKey.Up;
                case Key.Down:
                    return KeyboardKey.Down;
                case Key.Left:
                    return KeyboardKey.Left;
                case Key.Right:
                    return KeyboardKey.Right;
                case Key.Enter:
                    return KeyboardKey.Enter;
                case Key.Escape:
                    return KeyboardKey.Escape;
                case Key.Space:
                    return KeyboardKey.Space;
                case Key.Tab:
                    return KeyboardKey.Tab;
                case Key.BackSpace:
                    return KeyboardKey.Backspace;
                case Key.Insert:
                    return KeyboardKey.Insert;
                case Key.Delete:
                    return KeyboardKey.Delete;
                case Key.PageUp:
                    return KeyboardKey.PageUp;
                case Key.PageDown:
                    return KeyboardKey.PageDown;
                case Key.Home:
                    return KeyboardKey.Home;
                case Key.End:
                    return KeyboardKey.End;
                case Key.CapsLock:
                    return KeyboardKey.CapsLock;
                case Key.ScrollLock:
                    return KeyboardKey.ScrollLock;
                case Key.PrintScreen:
                    return KeyboardKey.PrintScreen;
                case Key.Pause:
                    return KeyboardKey.Pause;
                case Key.NumLock:
                    return KeyboardKey.NumLock;
                case Key.Clear:
                    return KeyboardKey.Clear;
                case Key.Sleep:
                    return KeyboardKey.Sleep;
                case Key.Keypad0:
                    return KeyboardKey.Keypad0;
                case Key.Keypad1:
                    return KeyboardKey.Keypad1;
                case Key.Keypad2:
                    return KeyboardKey.Keypad2;
                case Key.Keypad3:
                    return KeyboardKey.Keypad3;
                case Key.Keypad4:
                    return KeyboardKey.Keypad4;
                case Key.Keypad5:
                    return KeyboardKey.Keypad5;
                case Key.Keypad6:
                    return KeyboardKey.Keypad6;
                case Key.Keypad7:
                    return KeyboardKey.Keypad7;
                case Key.Keypad8:
                    return KeyboardKey.Keypad8;
                case Key.Keypad9:
                    return KeyboardKey.Keypad9;
                case Key.KeypadDivide:
                    return KeyboardKey.KeypadDivide;
                case Key.KeypadMultiply:
                    return KeyboardKey.KeypadMultiply;
                case Key.KeypadMinus:
                    return KeyboardKey.KeypadMinus;
                case Key.KeypadPlus:
                    return KeyboardKey.KeypadPlus;
                case Key.KeypadDecimal:
                    return KeyboardKey.KeypadDecimal;
                case Key.KeypadEnter:
                    return KeyboardKey.KeypadEnter;
                case Key.A:
                    return KeyboardKey.A;
                case Key.B:
                    return KeyboardKey.B;
                case Key.C:
                    return KeyboardKey.C;
                case Key.D:
                    return KeyboardKey.D;
                case Key.E:
                    return KeyboardKey.E;
                case Key.F:
                    return KeyboardKey.F;
                case Key.G:
                    return KeyboardKey.G;
                case Key.H:
                    return KeyboardKey.H;
                case Key.I:
                    return KeyboardKey.I;
                case Key.J:
                    return KeyboardKey.J;
                case Key.K:
                    return KeyboardKey.K;
                case Key.L:
                    return KeyboardKey.L;
                case Key.M:
                    return KeyboardKey.M;
                case Key.N:
                    return KeyboardKey.N;
                case Key.O:
                    return KeyboardKey.O;
                case Key.P:
                    return KeyboardKey.P;
                case Key.Q:
                    return KeyboardKey.Q;
                case Key.R:
                    return KeyboardKey.R;
                case Key.S:
                    return KeyboardKey.S;
                case Key.T:
                    return KeyboardKey.T;
                case Key.U:
                    return KeyboardKey.U;
                case Key.V:
                    return KeyboardKey.V;
                case Key.W:
                    return KeyboardKey.W;
                case Key.X:
                    return KeyboardKey.X;
                case Key.Y:
                    return KeyboardKey.Y;
                case Key.Z:
                    return KeyboardKey.Z;
                case Key.Number0:
                    return KeyboardKey.Number0;
                case Key.Number1:
                    return KeyboardKey.Number1;
                case Key.Number2:
                    return KeyboardKey.Number2;
                case Key.Number3:
                    return KeyboardKey.Number3;
                case Key.Number4:
                    return KeyboardKey.Number4;
                case Key.Number5:
                    return KeyboardKey.Number5;
                case Key.Number6:
                    return KeyboardKey.Number6;
                case Key.Number7:
                    return KeyboardKey.Number7;
                case Key.Number8:
                    return KeyboardKey.Number8;
                case Key.Number9:
                    return KeyboardKey.Number9;
                case Key.Tilde:
                    return KeyboardKey.Tilde;
                case Key.Minus:
                    return KeyboardKey.Minus;
                case Key.Plus:
                    return KeyboardKey.Plus;
                case Key.BracketLeft:
                    return KeyboardKey.BracketLeft;
                case Key.BracketRight:
                    return KeyboardKey.BracketRight;
                case Key.Semicolon:
                    return KeyboardKey.Semicolon;
                case Key.Quote:
                    return KeyboardKey.Quote;
                case Key.Comma:
                    return KeyboardKey.Comma;
                case Key.Period:
                    return KeyboardKey.Period;
                case Key.Slash:
                    return KeyboardKey.Slash;
                case Key.BackSlash:
                    return KeyboardKey.Backslash;
                default:
                    return KeyboardKey.Unknown;
            }
        }

        private KeyboardDevice _keyboard;
    }
}
