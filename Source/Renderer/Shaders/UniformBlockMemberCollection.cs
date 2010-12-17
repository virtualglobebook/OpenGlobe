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
    public class UniformBlockMemberCollection : KeyedCollection<string, UniformBlockMember>
    {
        protected override string GetKeyForItem(UniformBlockMember item)
        {
            return item.Name;
        }
    }
}
