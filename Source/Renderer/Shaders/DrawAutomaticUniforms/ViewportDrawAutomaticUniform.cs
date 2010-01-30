#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System.Drawing;
using OpenTK;
using MiniGlobe.Core;

namespace MiniGlobe.Renderer
{
    internal class ViewportDrawAutomaticUniform : DrawAutomaticUniform
    {
        public ViewportDrawAutomaticUniform(Uniform uniform)
        {
            _uniform = uniform as Uniform<Vector4>;
        }

        #region DrawAutomaticUniform Members

        public override void Set(Context context, SceneState sceneState)
        {
            //
            // viewport.Bottom should really be used but Rectangle goes top to botom, not bottom to top.
            //
            Rectangle viewport = context.Viewport;
            _uniform.Value = new Vector4(viewport.Left, viewport.Top, viewport.Width, viewport.Height);
        }

        #endregion

        private Uniform<Vector4> _uniform;
    }
}
