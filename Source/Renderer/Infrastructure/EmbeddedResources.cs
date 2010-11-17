#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System.IO;
using System.Reflection;

namespace OpenGlobe.Renderer
{
    internal static class EmbeddedResources
    {
        public static string GetText(string resourceName)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            //foreach (string name in assembly.GetManifestResourceNames())
            //{
            //    System.Console.WriteLine(name);
            //}
            
            Stream stream = assembly.GetManifestResourceStream(resourceName);
            StreamReader streamReader = new StreamReader(stream);
            return streamReader.ReadToEnd();
        }
    }
}
