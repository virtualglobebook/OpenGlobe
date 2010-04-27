#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using MiniGlobe.Core;

namespace MiniGlobe.Renderer.GL3x
{
    internal class ShaderObjectGL3x : Disposable
    {
        public ShaderObjectGL3x(ShaderType shaderType, string source)
        {
            string builtInConstants =
                "#version 150                                                   \n" +
                "const float mg_E =                " + Math.E + "; \n" +
                "const float mg_pi =               " + Math.PI + "; \n" +
                "const float mg_oneOverPi =        " + 1.0 / Math.PI + "; \n" +
                "const float mg_piOverTwo =        " + Math.PI * 0.5 + "; \n" +
                "const float mg_piOverThree =      " + Math.PI / 3.0 + "; \n" +
                "const float mg_piOverFour =       " + Math.PI / 4.0 + "; \n" +
                "const float mg_piOverSix =        " + Math.PI / 6.0 + "; \n" +
                "const float mg_threePiOver2 =     " + (3.0 * Math.PI) * 0.5 + "; \n" +
                "const float mg_twoPi =            " + Trig.TwoPI + "; \n" +
                "const float mg_oneOverTwoPi =     " + 1.0 / Trig.TwoPI + "; \n" +
                "const float mg_halfPi =           " + Trig.HalfPI + "; \n" +
                "const float mg_radiansPerDegree = " + Trig.RadiansPerDegree + "; \n";

            string modifiedSource;

            if (source.StartsWith("#version", StringComparison.CurrentCulture))
            {
                Debug.Assert(source.StartsWith("#version 150", StringComparison.CurrentCulture));
                modifiedSource = "//" + source;
            }
            else
            {
                modifiedSource = source;
            }

            string[] sources = new[] { builtInConstants, modifiedSource };
            int[] lengths = new[] { builtInConstants.Length, modifiedSource.Length };

            _shaderObject = GL.CreateShader(shaderType);
            unsafe
            {
                fixed (int *lengthPointer = lengths)
                {
                    GL.ShaderSource(_shaderObject, 2, sources, lengthPointer);
                }
            }
            GL.CompileShader(_shaderObject);

            int compileStatus;
            GL.GetShader(_shaderObject, ShaderParameter.CompileStatus, out compileStatus);

            if (compileStatus == 0)
            {
                throw new CouldNotCreateVideoCardResourceException("Could not compile shader object.  Compile Log:  \n\n" + CompileLog);
            }
        }

        ~ShaderObjectGL3x()
        {
            Dispose(false);
        }

        public int Handle
        {
            get { return _shaderObject; }
        }

        public string CompileLog
        {
            get { return GL.GetShaderInfoLog(_shaderObject); }
        }

        #region Disposable Members

        protected override void Dispose(bool disposing)
        {
            // Always delete the shader, even in the finalizer.
            GL.DeleteShader(_shaderObject);
            base.Dispose(disposing);
        }

        #endregion

        private readonly int _shaderObject;
    }
}
