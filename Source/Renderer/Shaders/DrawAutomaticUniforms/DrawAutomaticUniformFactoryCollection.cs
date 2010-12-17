#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System.Collections.ObjectModel;

namespace OpenGlobe.Renderer
{
    public class DrawAutomaticUniformFactoryCollection : KeyedCollection<string, DrawAutomaticUniformFactory>
    {
        protected override string GetKeyForItem(DrawAutomaticUniformFactory item)
        {
            return item.Name;
        }
    }
}
