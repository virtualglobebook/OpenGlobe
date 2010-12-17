#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;

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
