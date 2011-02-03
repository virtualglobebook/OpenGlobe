#region License
//
// (C) Copyright 2011 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;
using OpenGlobe.Renderer;

namespace OpenGlobe.Renderer.GL3x
{
    internal static class StringConverterGL3x
    {
        public static string UniformTypeToString(UniformType type)
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

        public static string ShaderVertexAttributeTypeToString(ShaderVertexAttributeType type)
        {
            switch (type)
            {
                case ShaderVertexAttributeType.Float:
                    return "float";
                case ShaderVertexAttributeType.FloatVector2:
                    return "vec2";
                case ShaderVertexAttributeType.FloatVector3:
                    return "vec3";
                case ShaderVertexAttributeType.FloatVector4:
                    return "vec4";
                case ShaderVertexAttributeType.FloatMatrix22:
                    return "mat2";
                case ShaderVertexAttributeType.FloatMatrix33:
                    return "mat3";
                case ShaderVertexAttributeType.FloatMatrix44:
                    return "mat4";
                case ShaderVertexAttributeType.Int:
                    return "int";
                case ShaderVertexAttributeType.IntVector2:
                    return "ivec2";
                case ShaderVertexAttributeType.IntVector3:
                    return "ivec3";
                case ShaderVertexAttributeType.IntVector4:
                    return "ivec4";
            }

            throw new ArgumentException("type");
        }
    }
}
