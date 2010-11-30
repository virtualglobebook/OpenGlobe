#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using OpenGlobe.Renderer;

namespace OpenGlobe.Scene
{
    internal abstract class ShapefileGraphics : IRenderable, IDisposable
    {
        public abstract void Render(Context context, SceneState sceneState);
        public abstract void Dispose();

        public abstract bool Wireframe { get; set; }
    }
}