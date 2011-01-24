#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

namespace OpenGlobe.Renderer
{
    internal class LightPropertiesUniformFactory : DrawAutomaticUniformFactory
    {
        #region DrawAutomaticUniformFactory Members

        public override string Name
        {
            get { return "og_diffuseSpecularAmbientShininess"; }
        }

        public override UniformType Datatype
        {
            get { return UniformType.FloatVector4; }
        }

        public override DrawAutomaticUniform Create(Uniform uniform)
        {
            return new LightPropertiesUniform(uniform);
        }

        #endregion
    }
}
