#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System.IO;

namespace OpenGlobe.Renderer
{
    public static class PersistentView
    {
        public static void Execute(string filename, GraphicsWindow window, Camera camera)
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