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
    public class LinkAutomaticUniformCollection : KeyedCollection<string, LinkAutomaticUniform>
    {
        protected override string GetKeyForItem(LinkAutomaticUniform item)
        {
            return item.Name;
        }
    }
}
