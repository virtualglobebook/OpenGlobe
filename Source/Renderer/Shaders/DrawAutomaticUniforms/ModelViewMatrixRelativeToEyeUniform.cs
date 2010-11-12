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
    internal class ModelViewMatrixRelativeToEyeUniform : DrawAutomaticUniform
    {
        public ModelViewMatrixRelativeToEyeUniform(Uniform uniform)
        {
            _uniform = (Uniform<Matrix4S>)uniform;
        }

        #region DrawAutomaticUniform Members

        public override void Set(Context context, DrawState drawState, SceneState sceneState)
        {
            _uniform.Value = sceneState.ModelViewMatrixRelativeToEye.ToMatrix4S();
        }

        #endregion

        private Uniform<Matrix4S> _uniform;
    }
}
