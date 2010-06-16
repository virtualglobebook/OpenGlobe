#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
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