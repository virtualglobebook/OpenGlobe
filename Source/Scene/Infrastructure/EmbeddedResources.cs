#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;
using System.IO;
using System.Reflection;

namespace OpenGlobe.Scene
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

        public static string ResourceNameToFilePath(string resourceName)
        {
            //
            // Convert a resource name like:
            //
            //   "OpenGlobe.Scene.Globes.RayCasted.Shaders.GlobeVS.glsl"
            //
            // to a relative file path like:
            //
            //   "Scene\\Globes\\RayCasted\\Shaders\\GlobeVS.glsl"
            //
            // making all sorts of assumptions in the process.
            //
            string relativeFilePath = resourceName.Substring(
                resourceName.IndexOf('.') + 1,
                resourceName.LastIndexOf('.') - resourceName.IndexOf('.') - 1);
            relativeFilePath = relativeFilePath.Replace(".", new string(Path.DirectorySeparatorChar, 1));
            relativeFilePath += resourceName.Substring(resourceName.LastIndexOf('.'));

            //
            // Search up folders from the current path for the resource
            // file.  To decision to search upto 16 paths is arbitrary
            // but is fine for sane source trees.
            //
            string currentPath = Directory.GetCurrentDirectory();
            string absoluteFilePath = null;

            const int numberOfParentsToCheck = 16;
            for (int i = 0; i < numberOfParentsToCheck; ++i)
            {
                string searchFilePath = currentPath;
                for (int j = 0; j < i; ++j)
                {
                    searchFilePath = Path.Combine(searchFilePath, ".." + Path.DirectorySeparatorChar);
                }
                searchFilePath = Path.Combine(searchFilePath, relativeFilePath);

                if (File.Exists(searchFilePath))
                {
                    absoluteFilePath = searchFilePath;
                    break;
                }
            }

            if (absoluteFilePath == null)
            {
                throw new ArgumentException("Could not find file for resources: " + resourceName);
            }

            return absoluteFilePath;
        }
    }
}
