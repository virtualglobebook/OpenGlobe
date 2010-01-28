#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System.Collections.ObjectModel;

namespace MiniGlobe.Renderer
{
    public class UniformBlockCollection : KeyedCollection<string, UniformBlock>
    {
        protected override string GetKeyForItem(UniformBlock item)
        {
            return item.Name;
        }
    }
}
