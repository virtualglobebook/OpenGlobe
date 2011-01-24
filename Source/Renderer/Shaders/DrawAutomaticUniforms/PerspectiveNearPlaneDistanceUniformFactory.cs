#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

namespace OpenGlobe.Renderer
{
    internal class PerspectiveNearPlaneDistanceUniformFactory : DrawAutomaticUniformFactory
    {
        #region PerspectiveNearPlaneDistanceUniformFactory Members

        public override string Name
        {
            get { return "og_perspectiveNearPlaneDistance"; }
        }

        public override UniformType Datatype
        {
            get { return UniformType.Float; }
        }

        public override DrawAutomaticUniform Create(Uniform uniform)
        {
            return new PerspectiveNearPlaneDistanceUniform(uniform);
        }

        #endregion
    }
}
