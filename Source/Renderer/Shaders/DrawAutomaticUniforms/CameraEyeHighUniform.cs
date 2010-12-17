#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using OpenGlobe.Core;

namespace OpenGlobe.Renderer
{
    internal class CameraEyeHighUniform : DrawAutomaticUniform
    {
        public CameraEyeHighUniform(Uniform uniform)
        {
            _uniform = (Uniform<Vector3F>)uniform;
        }

        #region DrawAutomaticUniform Members

        public override void Set(Context context, DrawState drawState, SceneState sceneState)
        {
            _uniform.Value = sceneState.Camera.EyeHigh;
        }

        #endregion

        private Uniform<Vector3F> _uniform;
    }
}
