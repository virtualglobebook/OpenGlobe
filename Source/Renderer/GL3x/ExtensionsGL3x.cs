#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using OpenTK.Graphics.OpenGL;

namespace OpenGlobe.Renderer.GL3x
{
    internal class ExtensionsGL3x : Extensions
    {
        public ExtensionsGL3x()
        {
            _anisotropicFiltering = false;
            int numberOfExtensions;
            GL.GetInteger(GetPName.NumExtensions, out numberOfExtensions);
            for (int i = 0; i < numberOfExtensions; ++i)
            {
                if (GL.GetString(StringName.Extensions, i) == "GL_EXT_texture_filter_anisotropic")
                {
                    _anisotropicFiltering = true;
                    break;
                }
            }
        }

        public override bool AnisotropicFiltering
        {
            get { return _anisotropicFiltering; }
        }

        private bool _anisotropicFiltering;
    }
}
