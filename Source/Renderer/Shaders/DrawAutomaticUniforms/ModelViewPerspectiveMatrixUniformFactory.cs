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
    internal class ModelViewPerspectiveMatrixUniformFactory : DrawAutomaticUniformFactory
    {
        #region DrawAutomaticUniformFactory Members

        public override string Name
        {
            get { return "og_modelViewPerspectiveMatrix"; }
        }

        public override UniformType Datatype
        {
            get { return UniformType.FloatMatrix44; }
        }

        public override DrawAutomaticUniform Create(Uniform uniform)
        {
            return new ModelViewPerspectiveMatrixUniform(uniform);
        }

        #endregion
    }
}
