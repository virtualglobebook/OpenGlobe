#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System.Collections.Generic;
using System.Collections.ObjectModel;
using OpenGlobe.Core;

namespace OpenGlobe.Renderer
{
    public class TextureCoordinateCollection : Collection<RectangleH>
    {
        public TextureCoordinateCollection(IList<RectangleH> list)
            : base(list)
        {
        }
    }
}