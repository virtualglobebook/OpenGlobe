#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;
using System.Globalization;
using System.Diagnostics;
using OpenTK.Graphics.OpenGL;
using OpenGlobe.Core;

namespace OpenGlobe.Renderer.GL3x
{
    internal class ShaderObjectGL3x : Disposable
    {
        public ShaderObjectGL3x(ShaderType shaderType, string source)
        {
            string builtinConstants =
                "#version 330 \n" +

                "#define og_positionVertexLocation          " + VertexLocations.Position.ToString(NumberFormatInfo.InvariantInfo) + " \n" +
                "#define og_normalVertexLocation            " + VertexLocations.Normal.ToString(NumberFormatInfo.InvariantInfo) + " \n" +
                "#define og_textureCoordinateVertexLocation " + VertexLocations.TextureCoordinate.ToString(NumberFormatInfo.InvariantInfo) + " \n" +
                "#define og_colorVertexLocation             " + VertexLocations.Color.ToString(NumberFormatInfo.InvariantInfo) + " \n" +
                "#define og_positionHighVertexLocation      " + VertexLocations.PositionHigh.ToString(NumberFormatInfo.InvariantInfo) + " \n" +
                "#define og_positionLowVertexLocation       " + VertexLocations.PositionLow.ToString(NumberFormatInfo.InvariantInfo) + " \n" +
                //
                // If the shader is authored in a tool like RenderMonkey,
                // the author will need to declare automatic uniforms, which
                // should be wrapped with:
                //
                //   #ifndef OPENGLOBE_AUTOMATIC_UNIFORMS
                //   #endif
                //
                // so OpenGlobe doesn't declare the uniforms again.  If the
                // shader is only ever executed with OpenGlobe, the author
                // doesn't need to declare any uniforms.
                //
                "#define OPENGLOBE_AUTOMATIC_UNIFORMS 1 \n\n" +

                "const float og_E =                " + Math.E.ToString(NumberFormatInfo.InvariantInfo) + "; \n" +
                "const float og_pi =               " + Math.PI.ToString(NumberFormatInfo.InvariantInfo) + "; \n" +
                "const float og_oneOverPi =        " + Trig.OneOverPi.ToString(NumberFormatInfo.InvariantInfo) + "; \n" +
                "const float og_piOverTwo =        " + Trig.PiOverTwo.ToString(NumberFormatInfo.InvariantInfo) + "; \n" +
                "const float og_piOverThree =      " + Trig.PiOverThree.ToString(NumberFormatInfo.InvariantInfo) + "; \n" +
                "const float og_piOverFour =       " + Trig.PiOverFour.ToString(NumberFormatInfo.InvariantInfo) + "; \n" +
                "const float og_piOverSix =        " + Trig.PiOverSix.ToString(NumberFormatInfo.InvariantInfo) + "; \n" +
                "const float og_threePiOver2 =     " + Trig.ThreePiOver2.ToString(NumberFormatInfo.InvariantInfo) + "; \n" +
                "const float og_twoPi =            " + Trig.TwoPi.ToString(NumberFormatInfo.InvariantInfo) + "; \n" +
                "const float og_oneOverTwoPi =     " + Trig.OneOverTwoPi.ToString(NumberFormatInfo.InvariantInfo) + "; \n" +
                "const float og_radiansPerDegree = " + Trig.RadiansPerDegree.ToString(NumberFormatInfo.InvariantInfo) + "; \n" +
                "const float og_maximumFloat =     " + float.MaxValue.ToString(NumberFormatInfo.InvariantInfo) + "; \n" +
                "const float og_minimumFloat =     " + float.MinValue.ToString(NumberFormatInfo.InvariantInfo) + "; \n";

            string builtinFunctions = EmbeddedResources.GetText("OpenGlobe.Renderer.GL3x.GLSL.BuiltinFunctions.glsl");

            string modifiedSource;

            //
            // This requires that #version be the first line in the shader.  This
            // doesn't follow the spec exactly, which allows whitespace and
            // comments to come beforehand.
            //
            if (source.StartsWith("#version", StringComparison.InvariantCulture))
            {
                if (!source.StartsWith("#version 330", StringComparison.InvariantCulture))
                {
                    throw new ArgumentException("Only GLSL version 330 is supported.");
                }

                modifiedSource = "//" + source;
            }
            else
            {
                modifiedSource = source;
            }

            //
            // NVIDIA apparently concatenates each source string together
            // so the line number reported for errors and warnings is
            // way off.  Here, we fix that.
            //
            // For whatever reason, NVIDIA also doesn't want #line to be 
            // the first line, so we add the dummy comment line.
            //
            // Also, we should really use '#line 3', because according to
            // the GLSL 3.3 spec:
            //
            //    "The number provided is the number of the next line of 
            //     code, not the current line.  This makes it match C++ 
            //     semantics"
            //
            // The line number is almost certainly to be wrong on ATI.
            //
            modifiedSource =
                "//#version 330 \n" +
                "#line 1 \n" +
                modifiedSource;

            string automaticUniforms = DrawAutomaticUniformDeclarations();

            string[] sources = new[] 
            { 
                builtinConstants, 
                automaticUniforms,
                builtinFunctions, 
                modifiedSource 
            };
            int[] lengths = new[] 
            { 
                builtinConstants.Length, 
                automaticUniforms.Length,
                builtinFunctions.Length, 
                modifiedSource.Length };

            _shaderObject = GL.CreateShader(shaderType);
            unsafe
            {
                fixed (int *lengthPointer = lengths)
                {
                    GL.ShaderSource(_shaderObject, sources.Length, sources, lengthPointer);
                }
            }
            GL.CompileShader(_shaderObject);

            int compileStatus;
            GL.GetShader(_shaderObject, ShaderParameter.CompileStatus, out compileStatus);

            if (compileStatus == 0)
            {
                Debug.WriteLine("Could not compile shader object.  Compile Log:  \n\n" + CompileLog);
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

        private static string DrawAutomaticUniformDeclarations()
        {
            string s = "";

            foreach (LinkAutomaticUniform uniform in Device.LinkAutomaticUniforms)
            {
                s += "uniform " + UniformTypeToName(uniform.Datatype) + " " + uniform.Name + "; \n";
            }

            foreach (DrawAutomaticUniformFactory uniform in Device.DrawAutomaticUniformFactories)
            {
                s += "uniform " + UniformTypeToName(uniform.Datatype) + " " + uniform.Name + "; \n";
            }

            return s;
        }

        private static string UniformTypeToName(UniformType type)
        {
            switch (type)
            {
                case UniformType.Int:
                    return "int";
                case UniformType.Float:
                    return "float";
                case UniformType.FloatVector2:
                    return "vec2";
                case UniformType.FloatVector3:
                    return "vec3";
                case UniformType.FloatVector4:
                    return "vec4";
                case UniformType.IntVector2:
                    return "ivec2";
                case UniformType.IntVector3:
                    return "ivec3";
                case UniformType.IntVector4:
                    return "ivec4";
                case UniformType.Bool:
                    return "bool";
                case UniformType.BoolVector2:
                    return "bvec2";
                case UniformType.BoolVector3:
                    return "bvec3";
                case UniformType.BoolVector4:
                    return "bvec4";
                case UniformType.FloatMatrix22:
                    return "mat2";
                case UniformType.FloatMatrix33:
                    return "mat3";
                case UniformType.FloatMatrix44:
                    return "mat4";
                case UniformType.Sampler1D:
                    return "sampler1D";
                case UniformType.Sampler2D:
                    return "sampler2D";
                case UniformType.Sampler2DRectangle:
                    return "sampler2DRect";
                case UniformType.Sampler2DRectangleShadow:
                    return "sampler2DRectShadow";
                case UniformType.Sampler3D:
                    return "sampler3D";
                case UniformType.SamplerCube:
                    return "samplerCube";
                case UniformType.Sampler1DShadow:
                    return "sampler1DShadow";
                case UniformType.Sampler2DShadow:
                    return "sampler2DShadow";
                case UniformType.FloatMatrix23:
                    return "mat2x3";
                case UniformType.FloatMatrix24:
                    return "mat2x4";
                case UniformType.FloatMatrix32:
                    return "mat3x2";
                case UniformType.FloatMatrix34:
                    return "mat3x4";
                case UniformType.FloatMatrix42:
                    return "mat4x2";
                case UniformType.FloatMatrix43:
                    return "mat4x3";
                case UniformType.Sampler1DArray:
                    return "sampler1DArray";
                case UniformType.Sampler2DArray:
                    return "sampler2DArray";
                case UniformType.Sampler1DArrayShadow:
                    return "sampler1DArrayShadow";
                case UniformType.Sampler2DArrayShadow:
                    return "sampler2DArrayShadow";
                case UniformType.SamplerCubeShadow:
                    return "samplerCubeShadow";
                case UniformType.IntSampler1D:
                    return "isampler1D";
                case UniformType.IntSampler2D:
                    return "isampler2D";
                case UniformType.IntSampler2DRectangle:
                    return "isampler2DRect";
                case UniformType.IntSampler3D:
                    return "isampler3D";
                case UniformType.IntSamplerCube:
                    return "isamplerCube";
                case UniformType.IntSampler1DArray:
                    return "isampler1DArray";
                case UniformType.IntSampler2DArray:
                    return "isampler2DArray";
                case UniformType.UnsignedIntSampler1D:
                    return "usampler1D";
                case UniformType.UnsignedIntSampler2D:
                    return "usampler2D";
                case UniformType.UnsignedIntSampler2DRectangle:
                    return "usampler2DRect";
                case UniformType.UnsignedIntSampler3D:
                    return "usampler3D";
                case UniformType.UnsignedIntSamplerCube:
                    return "usamplerCube";
                case UniformType.UnsignedIntSampler1DArray:
                    return "usampler1DArray";
                case UniformType.UnsignedIntSampler2DArray:
                    return "usampler2DArray";
            }

            throw new ArgumentException("type");
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
