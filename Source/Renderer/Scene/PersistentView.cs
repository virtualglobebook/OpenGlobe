#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System.IO;

namespace MiniGlobe.Renderer
{
    public static class PersistentView
    {
        public static void Execute(string filename, MiniGlobeWindow window, Camera camera)
        {
            if (File.Exists(filename))
            {
                camera.LoadView(filename);
            }

            window.Keyboard.KeyDown += delegate(object sender, KeyboardKeyEventArgs e)
            {
                if (e.Key == KeyboardKey.Space)
                {
                    camera.SaveView(filename);
                }
            };
        }
    }
}