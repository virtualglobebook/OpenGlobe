#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

namespace OpenGlobe.Renderer
{
    internal class CameraEyeHighUniformFactory : DrawAutomaticUniformFactory
    {
        #region DrawAutomaticUniformFactory Members

        public override string Name
        {
            get { return "og_cameraEyeHigh"; }
        }

        public override DrawAutomaticUniform Create(Uniform uniform)
        {
            return new CameraEyeHighUniform(uniform);
        }

        #endregion
    }
}
