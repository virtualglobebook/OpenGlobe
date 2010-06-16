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
    internal class Wgs84AltitudeUniform : DrawAutomaticUniform
    {
        public Wgs84AltitudeUniform(Uniform uniform)
        {
            _uniform = uniform as Uniform<float>;
        }

        #region DrawAutomaticUniform Members

        public override void Set(Context context, SceneState sceneState)
        {
            _uniform.Value = (float)sceneState.Camera.Altitude(Ellipsoid.Wgs84);
        }

        #endregion

        private Uniform<float> _uniform;
    }
}
