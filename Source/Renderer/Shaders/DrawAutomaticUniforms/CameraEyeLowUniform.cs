#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using OpenGlobe.Core;

namespace OpenGlobe.Renderer
{
    internal class CameraEyeLowUniform : DrawAutomaticUniform
    {
        public CameraEyeLowUniform(Uniform uniform)
        {
            _uniform = (Uniform<Vector3S>)uniform;
        }

        #region DrawAutomaticUniform Members

        public override void Set(Context context, DrawState drawState, SceneState sceneState)
        {
            _uniform.Value = sceneState.Camera.EyeLow;
        }

        #endregion

        private Uniform<Vector3S> _uniform;
    }
}
