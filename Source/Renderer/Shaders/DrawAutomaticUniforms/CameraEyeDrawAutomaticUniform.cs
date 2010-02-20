#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using OpenTK;
using MiniGlobe.Core;

namespace MiniGlobe.Renderer
{
    internal class CameraEyeUniform : DrawAutomaticUniform
    {
        public CameraEyeUniform(Uniform uniform)
        {
            _uniform = uniform as Uniform<Vector3>;
        }

        #region DrawAutomaticUniform Members

        public override void Set(Context context, SceneState sceneState)
        {
            _uniform.Value = Conversion.ToVector3(sceneState.Camera.Eye);
        }

        #endregion

        private Uniform<Vector3> _uniform;
    }
}
