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
using System.Globalization;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenGlobe.Core;

namespace OpenGlobe.Renderer.GL3x
{
    internal class ShaderObjectGL3x : Disposable
    {
        public ShaderObjectGL3x(ShaderType shaderType, string source)
        {
            string builtInConstants =
                "#version 330 \n" +

                "#define og_positionVertexLocation          0 \n" +
                "#define og_normalVertexLocation            1 \n" +
                "#define og_textureCoordinateVertexLocation 2 \n" +
                "#define og_colorVertexLocation             3 \n" +

                "const float og_E =                " + Math.E.ToString(NumberFormatInfo.InvariantInfo) + "; \n" +
                "const float og_pi =               " + Math.PI.ToString(NumberFormatInfo.InvariantInfo) + "; \n" +
                "const float og_oneOverPi =        " + (1.0 / Math.PI).ToString(NumberFormatInfo.InvariantInfo) + "; \n" +
                "const float og_piOverTwo =        " + (Math.PI * 0.5).ToString(NumberFormatInfo.InvariantInfo) + "; \n" +
                "const float og_piOverThree =      " + (Math.PI / 3.0).ToString(NumberFormatInfo.InvariantInfo) + "; \n" +
                "const float og_piOverFour =       " + (Math.PI / 4.0).ToString(NumberFormatInfo.InvariantInfo) + "; \n" +
                "const float og_piOverSix =        " + (Math.PI / 6.0).ToString(NumberFormatInfo.InvariantInfo) + "; \n" +
                "const float og_threePiOver2 =     " + ((3.0 * Math.PI) * 0.5).ToString(NumberFormatInfo.InvariantInfo) + "; \n" +
                "const float og_twoPi =            " + (Trig.TwoPI).ToString(NumberFormatInfo.InvariantInfo) + "; \n" +
                "const float og_oneOverTwoPi =     " + (1.0 / Trig.TwoPI).ToString(NumberFormatInfo.InvariantInfo) + "; \n" +
                "const float og_halfPi =           " + (Trig.HalfPI).ToString(NumberFormatInfo.InvariantInfo) + "; \n" +
                "const float og_radiansPerDegree = " + (Trig.RadiansPerDegree).ToString(NumberFormatInfo.InvariantInfo) + "; \n";

            string modifiedSource;

            //
            // This requires that #version be the first line in the shader.  This
            // doesn't follow the spec exactly, which allows whitespace and
            // comments to come beforehand.
            //
            if (source.StartsWith("#version", StringComparison.InvariantCulture))
            {
                Debug.Assert(source.StartsWith("#version 330", StringComparison.InvariantCulture));
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
                Console.WriteLine(sources[0]);
                Console.WriteLine(sources[1]);
                throw new CouldNotCreateVideoCardResourceException("Could not compile shader object.  Compile Log:  \n\n" + CompileLog);
            }
        }

        ~ShaderObjectGL3x()
        {
            FinalizerThreadContextGL3x.RunFinalizer(Dispose);
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
            if (_shaderObject != 0)
            {
                GL.DeleteShader(_shaderObject);
                _shaderObject = 0;
            }
            base.Dispose(disposing);
        }

        #endregion

        private int _shaderObject;
    }
}
