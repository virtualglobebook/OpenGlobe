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
    internal class ModelViewPerspectiveMatrixRelativeToEyeUniform : DrawAutomaticUniform
    {
        public ModelViewPerspectiveMatrixRelativeToEyeUniform(Uniform uniform)
        {
            _uniform = (Uniform<Matrix4F>)uniform;
        }

        #region DrawAutomaticUniform Members

        public override void Set(Context context, DrawState drawState, SceneState sceneState)
        {
            _uniform.Value = sceneState.ModelViewPerspectiveMatrixRelativeToEye.ToMatrix4F();
        }

        #endregion

        private Uniform<Matrix4F> _uniform;
    }
}
