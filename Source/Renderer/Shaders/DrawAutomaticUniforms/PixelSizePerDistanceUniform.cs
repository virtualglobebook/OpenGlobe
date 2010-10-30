#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using OpenTK;

namespace OpenGlobe.Renderer
{
    internal class PixelSizePerDistanceUniform : DrawAutomaticUniform
    {
        public PixelSizePerDistanceUniform(Uniform uniform)
        {
            _uniform = (Uniform<float>)uniform;
        }

        #region DrawAutomaticUniform Members

        public override void Set(Context context, DrawState drawState, SceneState sceneState)
        {
            _uniform.Value = (float)(Math.Tan(0.5 * sceneState.Camera.FieldOfViewY) * 2.0 / context.Viewport.Height);
        }

        #endregion

        private Uniform<float> _uniform;
    }
}
