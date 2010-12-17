#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using OpenGlobe.Core;

namespace OpenGlobe.Renderer
{
    internal class ModelZToClipCoordinatesUniform : DrawAutomaticUniform
    {
        public ModelZToClipCoordinatesUniform(Uniform uniform)
        {
            _uniform = (Uniform<Matrix42<float>>)uniform;
        }

        #region DrawAutomaticUniform Members

        public override void Set(Context context, DrawState drawState, SceneState sceneState)
        {
            _uniform.Value = Matrix42<float>.DoubleToFloat(sceneState.ModelZToClipCoordinates);
        }

        #endregion

        private Uniform<Matrix42<float>> _uniform;
    }
}
