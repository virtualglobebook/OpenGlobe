#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using OpenTK;

namespace OpenGlobe.Renderer
{
    internal class ModelViewPerspectiveMatrixUniform : DrawAutomaticUniform
    {
        public ModelViewPerspectiveMatrixUniform(Uniform uniform)
        {
            _uniform = uniform as Uniform<Matrix4>;
        }

        #region DrawAutomaticUniform Members

        public override void Set(Context context, DrawState drawState, SceneState sceneState)
        {
            _uniform.Value = Conversion.ToMatrix4(sceneState.ModelViewPerspectiveMatrix);
        }

        #endregion

        private Uniform<Matrix4> _uniform;
    }
}
