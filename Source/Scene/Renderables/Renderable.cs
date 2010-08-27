#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using OpenGlobe.Renderer;

namespace OpenGlobe.Scene
{
    public interface IRenderable
    {
        void Render(Context context, SceneState sceneState);
    }
}
