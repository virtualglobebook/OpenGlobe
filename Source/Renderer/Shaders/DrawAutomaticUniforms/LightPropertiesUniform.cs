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
    internal class LightPropertiesUniform : DrawAutomaticUniform
    {
        public LightPropertiesUniform(Uniform uniform)
        {
            _uniform = (Uniform<Vector4F>)uniform;
        }

        #region DrawAutomaticUniform Members

        public override void Set(Context context, DrawState drawState, SceneState sceneState)
        {
            _uniform.Value = new Vector4F(
                sceneState.DiffuseIntensity,
                sceneState.SpecularIntensity,
                sceneState.AmbientIntensity,
                sceneState.Shininess);
        }

        #endregion

        private Uniform<Vector4F> _uniform;
    }
}
