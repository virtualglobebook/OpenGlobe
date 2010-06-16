#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using OpenGlobe.Core.Geometry;

namespace OpenGlobe.Renderer
{
    internal class PerspectiveFarPlaneDistanceUniform : DrawAutomaticUniform
    {
        public PerspectiveFarPlaneDistanceUniform(Uniform uniform)
        {
            _uniform = uniform as Uniform<float>;
        }

        #region DrawAutomaticUniform Members

        public override void Set(Context context, SceneState sceneState)
        {
            _uniform.Value = (float)sceneState.Camera.PerspectiveFarPlaneDistance;
        }

        #endregion

        private Uniform<float> _uniform;
    }
}
