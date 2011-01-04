#region License
//
// (C) Copyright 2011 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System.Collections.ObjectModel;

namespace OpenGlobe.Renderer
{
    public class TransformFeedbackOutputCollection : KeyedCollection<string, TransformFeedbackOutput>
    {
        protected override string GetKeyForItem(TransformFeedbackOutput item)
        {
            return item.Name;
        }
    }
}
