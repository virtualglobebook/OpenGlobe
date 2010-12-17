#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using OpenGlobe.Core;

namespace OpenGlobe.Renderer
{
    internal class ViewportOrthographicMatrixUniform : DrawAutomaticUniform
    {
        public ViewportOrthographicMatrixUniform(Uniform uniform)
        {
            _uniform = (Uniform<Matrix4F>)uniform;
        }

        #region DrawAutomaticUniform Members

        public override void Set(Context context, DrawState drawState, SceneState sceneState)
        {
            _uniform.Value = SceneState.ComputeViewportOrthographicMatrix(context.Viewport).ToMatrix4F();
        }

        #endregion

        private Uniform<Matrix4F> _uniform;
    }
}
