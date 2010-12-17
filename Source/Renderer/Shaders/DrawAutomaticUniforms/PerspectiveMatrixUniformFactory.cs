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
    internal class PerspectiveMatrixUniformFactory : DrawAutomaticUniformFactory
    {
        #region DrawAutomaticUniformFactory Members

        public override string Name
        {
            get { return "og_perspectiveMatrix"; }
        }

        public override DrawAutomaticUniform Create(Uniform uniform)
        {
            return new PerspectiveMatrixUniform(uniform);
        }

        #endregion
    }
}
