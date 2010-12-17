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
    internal class CameraEyeLowUniformFactory : DrawAutomaticUniformFactory
    {
        #region DrawAutomaticUniformFactory Members

        public override string Name
        {
            get { return "og_cameraEyeLow"; }
        }

        public override DrawAutomaticUniform Create(Uniform uniform)
        {
            return new CameraEyeLowUniform(uniform);
        }

        #endregion
    }
}
