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
    internal class PerspectiveFarPlaneDistanceUniformFactory : DrawAutomaticUniformFactory
    {
        #region PerspectiveFarPlaneDistanceUniformFactory Members

        public override string Name
        {
            get { return "og_perspectiveFarPlaneDistance"; }
        }

        public override UniformType Datatype
        {
            get { return UniformType.Float; }
        }

        public override DrawAutomaticUniform Create(Uniform uniform)
        {
            return new PerspectiveFarPlaneDistanceUniform(uniform);
        }

        #endregion
    }
}
