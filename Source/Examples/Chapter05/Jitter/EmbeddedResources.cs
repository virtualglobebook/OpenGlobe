#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System.IO;
using System.Reflection;

namespace OpenGlobe.Examples
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
