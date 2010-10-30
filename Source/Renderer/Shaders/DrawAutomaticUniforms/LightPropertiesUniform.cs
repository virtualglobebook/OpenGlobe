#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using OpenGlobe.Core;

namespace OpenGlobe.Renderer
{
    internal class LightPropertiesUniform : DrawAutomaticUniform
    {
        public LightPropertiesUniform(Uniform uniform)
        {
            _uniform = (Uniform<Vector4S>)uniform;
        }

        #region DrawAutomaticUniform Members

        public override void Set(Context context, DrawState drawState, SceneState sceneState)
        {
            _uniform.Value = new Vector4S(
                sceneState.DiffuseIntensity,
                sceneState.SpecularIntensity,
                sceneState.AmbientIntensity,
                sceneState.Shininess);
        }

        #endregion

        private Uniform<Vector4S> _uniform;
    }
}
