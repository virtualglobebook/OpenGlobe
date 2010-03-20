#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

namespace MiniGlobe.Renderer
{
    internal class ModelZToClipCoordinatesUniform : DrawAutomaticUniform
    {
        public ModelZToClipCoordinatesUniform(Uniform uniform)
        {
            _uniform = uniform as Uniform<Matrix42>;
        }

        #region DrawAutomaticUniform Members

        public override void Set(Context context, SceneState sceneState)
        {
            _uniform.Value = sceneState.ModelZToClipCoordinates;
        }

        #endregion

        private Uniform<Matrix42> _uniform;
    }
}
