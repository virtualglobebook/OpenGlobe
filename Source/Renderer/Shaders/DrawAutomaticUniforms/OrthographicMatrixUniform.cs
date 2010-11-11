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
    internal class OrthographicMatrixUniform : DrawAutomaticUniform
    {
        public OrthographicMatrixUniform(Uniform uniform)
        {
            _uniform = (Uniform<Matrix4F>)uniform;
        }

        #region DrawAutomaticUniform Members

        public override void Set(Context context, DrawState drawState, SceneState sceneState)
        {
            _uniform.Value = sceneState.OrthographicMatrix.ToMatrix4F();
        }

        #endregion

        private Uniform<Matrix4F> _uniform;
    }
}
