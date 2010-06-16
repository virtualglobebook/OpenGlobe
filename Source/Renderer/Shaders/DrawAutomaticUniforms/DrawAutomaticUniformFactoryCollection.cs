#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System.Collections.ObjectModel;

namespace OpenGlobe.Renderer
{
    internal class DrawAutomaticUniformFactoryCollection : KeyedCollection<string, DrawAutomaticUniformFactory>
    {
        protected override string GetKeyForItem(DrawAutomaticUniformFactory item)
        {
            return item.Name;
        }
    }
}
