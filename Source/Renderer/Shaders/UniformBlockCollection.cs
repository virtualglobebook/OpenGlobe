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
    public class UniformBlockCollection : KeyedCollection<string, UniformBlock>
    {
        protected override string GetKeyForItem(UniformBlock item)
        {
            return item.Name;
        }
    }
}
