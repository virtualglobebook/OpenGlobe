#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTK.Graphics.OpenGL;
using MiniGlobe.Renderer;

namespace MiniGlobe.Renderer.GL32
{
    internal class ShaderProgramGL32 : ShaderProgram
    {
        public ShaderProgramGL32(
            string vertexShaderSource,
            string fragmentShaderSource)
            : this(vertexShaderSource, string.Empty, fragmentShaderSource)
        {
        }

        public ShaderProgramGL32(
            string vertexShaderSource,
            string geometryShaderSource,
            string fragmentShaderSource)
        {
            _vertexShader = new ShaderObjectGL32(ShaderType.VertexShader, vertexShaderSource);
            if (geometryShaderSource.Length > 0)
            {
                _geometryShader = new ShaderObjectGL32(ShaderType.GeometryShaderExt, geometryShaderSource);
            }
            _fragmentShader = new ShaderObjectGL32(ShaderType.FragmentShader, fragmentShaderSource);

            _program = GL.CreateProgram();
            GL.AttachShader(_program, _vertexShader.Handle);
            if (geometryShaderSource.Length > 0)
            {
                GL.AttachShader(_program, _geometryShader.Handle);
            }
            GL.AttachShader(_program, _fragmentShader.Handle);

            GL.LinkProgram(_program);

            int linkStatus;
            GL.GetProgram(_program, ProgramParameter.LinkStatus, out linkStatus);

            if (linkStatus == 0)
            {
                throw new CouldNotCreateVideoCardResourceException("Could not link shader program.  Link Log:  \n\n" + ProgramInfoLog);
            }

            _vertexAttributes = FindVertexAttributes(_program);
            _uniforms = FindUniforms(_program);
            _uniformsToClean = CreateUniformsToClean(_uniforms);
            _uniformBlocks = FindUniformBlocks(_program);

            InitializeAutomaticUniforms(_uniforms);
        }

        private static ShaderVertexAttributeCollection FindVertexAttributes(int program)
        {
            int numberOfAttributes;
            GL.GetProgram(program, ProgramParameter.ActiveAttributes, out numberOfAttributes);

            int attributeNameMaxLength;
            GL.GetProgram(program, ProgramParameter.ActiveAttributeMaxLength, out attributeNameMaxLength);

            ShaderVertexAttributeCollection vertexAttributes = new ShaderVertexAttributeCollection();
            for (int i = 0; i < numberOfAttributes; ++i)
            {
                int attributeNameLength;
                int attributeLength;
                ActiveAttribType attributeType;
                StringBuilder attributeNameBuilder = new StringBuilder();

                GL.GetActiveAttrib(program, i, attributeNameMaxLength,
                    out attributeNameLength, out attributeLength, out attributeType, attributeNameBuilder);

                string attributeName = attributeNameBuilder.ToString();

                if (attributeName.StartsWith("gl_", StringComparison.CurrentCulture))
                {
                    //
                    // Names starting with the reserved prefix of "gl_" have a location of -1.
                    //
                    continue;
                }

                int attributeLocation = GL.GetAttribLocation(program, attributeName);

                vertexAttributes.Add(new ShaderVertexAttribute(
                    attributeName, attributeLocation, TypeConverterGL32.To(attributeType), attributeLength));
            }

            return vertexAttributes;
        }

        private UniformCollection FindUniforms(int program)
        {
            int numberOfUniforms;
            GL.GetProgram(program, ProgramParameter.ActiveUniforms, out numberOfUniforms);

            int uniformNameMaxLength;
            GL.GetProgram(program, ProgramParameter.ActiveUniformMaxLength, out uniformNameMaxLength);

            UniformCollection uniforms = new UniformCollection();
            for (int i = 0; i < numberOfUniforms; ++i)
            {
                int uniformNameLength;
                int uniformSize;
                ActiveUniformType uniformType;
                StringBuilder uniformNameBuilder = new StringBuilder();

                GL.GetActiveUniform(program, i, uniformNameMaxLength,
                    out uniformNameLength, out uniformSize, out uniformType, uniformNameBuilder);

                string uniformName = CorrectUniformName(uniformNameBuilder.ToString());

                if (uniformName.StartsWith("gl_", StringComparison.CurrentCulture))
                {
                    //
                    // Names starting with the reserved prefix of "gl_" have a location of -1.
                    //
                    continue;
                }

                //
                // Skip uniforms in a named block
                //
                int uniformBlockIndex;
                GL.GetActiveUniforms(program, 1, ref i, ArbUniformBufferObject.UniformBlockIndex, out uniformBlockIndex);

                if (uniformBlockIndex != -1)
                {
                    continue;
                }

                // TODO:  Support arrays
                Debug.Assert(uniformSize == 1);

                int uniformLocation = GL.GetUniformLocation(program, uniformName);
                uniforms.Add(CreateUniform(uniformName, uniformLocation, uniformType));
            }

            return uniforms;
        }

        private Uniform CreateUniform(
            string name, 
            int location, 
            ActiveUniformType type)
        {
            if (type == ActiveUniformType.Float)
            {
                return new UniformFloatGL32(_program, name, location);
            }
            else if (type == ActiveUniformType.FloatVec2)
            {
                return new UniformFloatVector2GL32(_program, name, location);
            }
            else if (type == ActiveUniformType.FloatVec3)
            {
                return new UniformFloatVector3GL32(_program, name, location);
            }
            else if (type == ActiveUniformType.FloatVec4)
            {
                return new UniformFloatVector4GL32(_program, name, location);
            }
            else if (type == ActiveUniformType.Int)
            {
                return new UniformIntGL32(_program, name, location, UniformType.Int);
            }
            else if (type == ActiveUniformType.IntVec2)
            {
                return new UniformIntVector2GL32(_program, name, location);
            }
            else if (type == ActiveUniformType.IntVec3)
            {
                return new UniformIntVector3GL32(_program, name, location);
            }
            else if (type == ActiveUniformType.IntVec4)
            {
                return new UniformIntVector4GL32(_program, name, location);
            }
            else if (type == ActiveUniformType.Bool)
            {
                return new UniformBoolGL32(_program, name, location);
            }
            else if (type == ActiveUniformType.BoolVec2)
            {
                return new UniformBoolVector2GL32(_program, name, location);
            }
            else if (type == ActiveUniformType.BoolVec3)
            {
                return new UniformBoolVector3GL32(_program, name, location);
            }
            else if (type == ActiveUniformType.BoolVec4)
            {
                return new UniformBoolVector4GL32(_program, name, location);
            }
            else if (type == ActiveUniformType.FloatMat2)
            {
                return new UniformFloatMatrix22GL32(_program, name, location);
            }
            else if (type == ActiveUniformType.FloatMat3)
            {
                return new UniformFloatMatrix33GL32(_program, name, location);
            }
            else if (type == ActiveUniformType.FloatMat4)
            {
                return new UniformFloatMatrix44GL32(_program, name, location);
            }
            else if (type == ActiveUniformType.FloatMat2x3)
            {
                return new UniformFloatMatrix23GL32(_program, name, location);
            }
            else if (type == ActiveUniformType.FloatMat2x4)
            {
                return new UniformFloatMatrix24GL32(_program, name, location);
            }
            else if (type == ActiveUniformType.FloatMat3x2)
            {
                return new UniformFloatMatrix32GL32(_program, name, location);
            }
            else if (type == ActiveUniformType.FloatMat3x4)
            {
                return new UniformFloatMatrix34GL32(_program, name, location);
            }
            else if (type == ActiveUniformType.FloatMat4x2)
            {
                return new UniformFloatMatrix42GL32(_program, name, location);
            }
            else if (type == ActiveUniformType.FloatMat4x3)
            {
                return new UniformFloatMatrix43GL32(_program, name, location);
            }
            else if ((type == ActiveUniformType.Sampler1D) ||
                     (type == ActiveUniformType.Sampler2D) ||
                     (type == ActiveUniformType.Sampler3D) ||
                     (type == ActiveUniformType.SamplerCube) ||
                     (type == ActiveUniformType.Sampler1DShadow) ||
                     (type == ActiveUniformType.Sampler2DShadow) ||
                     (type == ActiveUniformType.Sampler1DArray) ||
                     (type == ActiveUniformType.Sampler2DArray) ||
                     (type == ActiveUniformType.Sampler1DArrayShadow) ||
                     (type == ActiveUniformType.Sampler2DArrayShadow) ||
                     (type == ActiveUniformType.SamplerCubeShadow) ||
                     (type == ActiveUniformType.IntSampler1D) ||
                     (type == ActiveUniformType.IntSampler2D) ||
                     (type == ActiveUniformType.IntSampler3D) ||
                     (type == ActiveUniformType.IntSamplerCube) ||
                     (type == ActiveUniformType.IntSampler1DArray) ||
                     (type == ActiveUniformType.IntSampler2DArray) ||
                     (type == ActiveUniformType.UnsignedIntSampler1D) ||
                     (type == ActiveUniformType.UnsignedIntSampler2D) ||
                     (type == ActiveUniformType.UnsignedIntSampler3D) ||
                     (type == ActiveUniformType.UnsignedIntSamplerCube) ||
                     (type == ActiveUniformType.UnsignedIntSampler1DArray) ||
                     (type == ActiveUniformType.UnsignedIntSampler2DArray))
            {
                return new UniformIntGL32(_program, name, location, TypeConverterGL32.To(type));
            }

            //
            // A new Uniform derived class needs to be added to support this uniform type.
            //
            Debug.Assert(false);
            return null;
        }

        private static ICleanable[] CreateUniformsToClean(UniformCollection uniforms)
        {
            ICleanable[] uniformsToClean = new ICleanable[uniforms.Count];

            for (int i = 0; i < uniforms.Count; ++i)
            {
                uniformsToClean[i] = uniforms[i] as ICleanable;
            }

            return uniformsToClean;
        }

        private static UniformBlockCollection FindUniformBlocks(int program)
        {
            int numberOfUniformBlocks;
            GL.GetProgram(program, ProgramParameter.ActiveUniformBlocks, out numberOfUniformBlocks);

            UniformBlockCollection uniformBlocks = new UniformBlockCollection();
            for (int i = 0; i < numberOfUniformBlocks; ++i)
            {
                string uniformBlockName = GL.GetActiveUniformBlockName(program, i);

                int uniformBlockSizeInBytes;
                GL.GetActiveUniformBlock(program, i, ActiveUniformBlockParameter.UniformBlockDataSize, out uniformBlockSizeInBytes);

                int numberOfUniformsInBlock;
                GL.GetActiveUniformBlock(program, i, ActiveUniformBlockParameter.UniformBlockActiveUniforms, out numberOfUniformsInBlock);

                int[] uniformIndicesInBlock = new int[numberOfUniformsInBlock];
                GL.GetActiveUniformBlock(program, i, ActiveUniformBlockParameter.UniformBlockActiveUniformIndices, uniformIndicesInBlock);

                //
                // Query uniforms in this named uniform block
                //
                int[] uniformTypes = new int[numberOfUniformsInBlock];
                int[] uniformOffsetsInBytes = new int[numberOfUniformsInBlock];
                int[] uniformLengths = new int[numberOfUniformsInBlock];
                int[] uniformArrayStridesInBytes = new int[numberOfUniformsInBlock];
                int[] uniformmatrixStrideInBytess = new int[numberOfUniformsInBlock];
                int[] uniformRowMajors = new int[numberOfUniformsInBlock];
                GL.GetActiveUniforms(program, numberOfUniformsInBlock, uniformIndicesInBlock, ArbUniformBufferObject.UniformType, uniformTypes);
                GL.GetActiveUniforms(program, numberOfUniformsInBlock, uniformIndicesInBlock, ArbUniformBufferObject.UniformOffset, uniformOffsetsInBytes);
                GL.GetActiveUniforms(program, numberOfUniformsInBlock, uniformIndicesInBlock, ArbUniformBufferObject.UniformSize, uniformLengths);
                GL.GetActiveUniforms(program, numberOfUniformsInBlock, uniformIndicesInBlock, ArbUniformBufferObject.UniformArrayStride, uniformArrayStridesInBytes);
                GL.GetActiveUniforms(program, numberOfUniformsInBlock, uniformIndicesInBlock, ArbUniformBufferObject.UniformMatrixStride, uniformmatrixStrideInBytess);
                GL.GetActiveUniforms(program, numberOfUniformsInBlock, uniformIndicesInBlock, ArbUniformBufferObject.UniformIsRowMajor, uniformRowMajors);

                UniformBlock uniformBlock = new UniformBlockGL32(uniformBlockName, uniformBlockSizeInBytes, i);

                for (int j = 0; j < numberOfUniformsInBlock; ++j)
                {
                    string uniformName = GL.GetActiveUniformName(program, uniformIndicesInBlock[j]);
                    uniformName = CorrectUniformName(uniformName);

                    UniformType uniformType = TypeConverterGL32.To((ActiveUniformType)uniformTypes[j]);

                    uniformBlock.Members.Add(CreateUniformBlockMember(uniformName,
                        uniformType, uniformOffsetsInBytes[j], uniformLengths[j], uniformArrayStridesInBytes[j],
                        uniformmatrixStrideInBytess[j], uniformRowMajors[j]));
                }

                uniformBlocks.Add(uniformBlock);

                //
                // Create a one to one mapping between uniform blocks and uniform buffer objects.
                //
                GL.UniformBlockBinding(program, i, i);
            }

            return uniformBlocks;
        }

        private static UniformBlockMember CreateUniformBlockMember(
            string name,
            UniformType type,
            int offsetInBytes,
            int length,
            int arrayStrideInBytes,
            int matrixStrideInBytes,
            int rowMajor)
        {
            if (length == 1)
            {
                if (!IsMatrix(type))
                {
                    //
                    // Non array, non matrix member
                    //
                    return new UniformBlockMember(name, type, offsetInBytes);
                }
                else
                {
                    //
                    // Non array, matrix member
                    //
                    return new UniformBlockMatrixMember(name, type, offsetInBytes,
                        matrixStrideInBytes, Convert.ToBoolean(rowMajor));
                }
            }
            else
            {
                if (!IsMatrix(type))
                {
                    //
                    // Array, non matrix member
                    //
                    return new UniformBlockArrayMember(name, type, offsetInBytes,
                        length, arrayStrideInBytes);
                }
                else
                {
                    //
                    // Array, matrix member
                    //
                    return new UniformBlockMatrixArrayMember(name, type, offsetInBytes,
                        length, arrayStrideInBytes, matrixStrideInBytes, Convert.ToBoolean(rowMajor));
                }
            }
        }

        private static bool IsMatrix(UniformType type)
        {
            return
                (type == UniformType.FloatMatrix22) ||
                (type == UniformType.FloatMatrix33) ||
                (type == UniformType.FloatMatrix44) ||
                (type == UniformType.FloatMatrix23) ||
                (type == UniformType.FloatMatrix24) ||
                (type == UniformType.FloatMatrix32) ||
                (type == UniformType.FloatMatrix34) ||
                (type == UniformType.FloatMatrix42) ||
                (type == UniformType.FloatMatrix43);
        }

        private static string CorrectUniformName(string name)
        {
            //
            // On ATI, array uniforms have a [0] suffix
            //
            if (name.EndsWith("[0]", StringComparison.CurrentCulture))
            {
                return name.Remove(name.Length - 3);
            }

            return name;
        }

        internal int Handle
        {
            get { return _program; }
        }

        internal void Bind()
        {
            GL.UseProgram(_program);
        }

        internal void Clean(Context context, SceneState sceneState)
        {
            SetDrawAutomaticUniforms(context, sceneState);

            for (int i = 0; i < _uniformsToClean.Length; ++i)
            {
                _uniformsToClean[i].Clean();
            }
        }

        private string ProgramInfoLog
        {
            get { return GL.GetProgramInfoLog(_program); }
        }

        #region ShaderProgram Members

        public override string LinkLog
        {
            get { return ProgramInfoLog; }
        }

        public override ShaderVertexAttributeCollection VertexAttributes
        {
            get { return _vertexAttributes; }
        }

        public override UniformCollection Uniforms
        {
            get { return _uniforms; }
        }

        public override UniformBlockCollection UniformBlocks
        {
            get { return _uniformBlocks; }
        }

        #endregion

        #region Disposable Members

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                GL.DeleteProgram(_program);

                _vertexShader.Dispose();
                if (_geometryShader != null)
                {
                    _geometryShader.Dispose();
                }
                _fragmentShader.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion

        private readonly ShaderObjectGL32 _vertexShader;
        private readonly ShaderObjectGL32 _geometryShader;
        private readonly ShaderObjectGL32 _fragmentShader;
        private readonly int _program;
        private readonly ShaderVertexAttributeCollection _vertexAttributes;
        private readonly UniformCollection _uniforms;
        private readonly ICleanable[] _uniformsToClean;
        private readonly UniformBlockCollection _uniformBlocks;
    }
}
