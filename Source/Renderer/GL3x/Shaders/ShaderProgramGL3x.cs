#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;
using System.Text;
using System.Collections.Generic;
using OpenGlobe.Core;
using OpenTK.Graphics.OpenGL;

namespace OpenGlobe.Renderer.GL3x
{
    internal class ShaderProgramGL3x : ShaderProgram, ICleanableObserver
    {
        public ShaderProgramGL3x(
            string vertexShaderSource,
            string geometryShaderSource,
            string fragmentShaderSource,
            IEnumerable<string> transformFeedbackOutputs,
            TransformFeedbackAttributeLayout transformFeedbackAttributeLayout)
        {
            _program = new ShaderProgramNameGL3x();
            int programHandle = _program.Value;

            //
            // Shader objects are disposed so they are marked for deletion
            // in GL, and will be deleted when the shader program is.
            //
            using (ShaderObjectGL3x vertexShader = new ShaderObjectGL3x(ShaderType.VertexShader, vertexShaderSource))
            using (ShaderObjectGL3x geometryShader = (geometryShaderSource.Length > 0) ?
                new ShaderObjectGL3x(ShaderType.GeometryShaderExt, geometryShaderSource) : null)
            using (ShaderObjectGL3x fragmentShader = new ShaderObjectGL3x(ShaderType.FragmentShader, fragmentShaderSource))
            {
                GL.AttachShader(programHandle, vertexShader.Handle);
                if (geometryShaderSource.Length > 0)
                {
                    GL.AttachShader(programHandle, geometryShader.Handle);
                }
                GL.AttachShader(programHandle, fragmentShader.Handle);
            }

            if (transformFeedbackOutputs != null)
            {
                string[] feedbackOutputs = CollectionAlgorithms.
                    EnumerableToArray(transformFeedbackOutputs);

                if (feedbackOutputs.Length > 0)
                {
                    if ((transformFeedbackAttributeLayout == TransformFeedbackAttributeLayout.Separate) &&
                        (feedbackOutputs.Length > Device.MaximumTransformFeedbackSeparateAttributes))
                    {
                        throw new InsufficientVideoCardException(
                            "The number of feedback outputs exceeds Device.MaximumTransformFeedbackSeparateAttributes.");
                    }

                    GL.TransformFeedbackVaryings(programHandle, feedbackOutputs.Length,
                        feedbackOutputs, TypeConverterGL3x.To(transformFeedbackAttributeLayout));
                }
            }

            GL.LinkProgram(programHandle);

            int linkStatus;
            GL.GetProgram(programHandle, ProgramParameter.LinkStatus, out linkStatus);

            if (linkStatus == 0)
            {
                throw new CouldNotCreateVideoCardResourceException("Could not link shader program.  Link Log:  \n\n" + ProgramInfoLog);
            }

            _transformFeedbackOutputs = FindTransformFeedbackOutputs(programHandle);
            _transformFeedbackAttributeLayout = transformFeedbackAttributeLayout;
            _fragmentOutputs = new FragmentOutputsGL3x(_program);
            _vertexAttributes = FindVertexAttributes(programHandle);
            _dirtyUniforms = new List<ICleanable>();
            _uniforms = FindUniforms(programHandle);
            _uniformBlocks = FindUniformBlocks(programHandle);

            InitializeAutomaticUniforms(_uniforms);
        }

        private static TransformFeedbackOutputCollection FindTransformFeedbackOutputs(int programHandle)
        {
            TransformFeedbackOutputCollection outputs = new TransformFeedbackOutputCollection();

            int numberOfVaryings;
            GL.GetProgram(programHandle, ProgramParameter.TransformFeedbackVaryings, out numberOfVaryings);

            int varyingNameMaxLength;
            GL.GetProgram(programHandle, ProgramParameter.TransformFeedbackVaryingMaxLength, out varyingNameMaxLength);

            for (int i = 0; i < numberOfVaryings; ++i)
            {
                int nameLength;
                int size;
                ActiveAttribType type;
                StringBuilder nameBuilder = new StringBuilder(varyingNameMaxLength);

                GL.GetTransformFeedbackVarying(programHandle, i,
                    varyingNameMaxLength, out nameLength, out size, out type, nameBuilder);

                String name = nameBuilder.ToString();

                outputs.Add(new TransformFeedbackOutput(name, TypeConverterGL3x.To(type), size));
            }

            return outputs;
        }

        private static ShaderVertexAttributeCollection FindVertexAttributes(int programHandle)
        {
            int numberOfAttributes;
            GL.GetProgram(programHandle, ProgramParameter.ActiveAttributes, out numberOfAttributes);

            int attributeNameMaxLength;
            GL.GetProgram(programHandle, ProgramParameter.ActiveAttributeMaxLength, out attributeNameMaxLength);

            ShaderVertexAttributeCollection vertexAttributes = new ShaderVertexAttributeCollection();
            for (int i = 0; i < numberOfAttributes; ++i)
            {
                int attributeNameLength;
                int attributeLength;
                ActiveAttribType attributeType;
                StringBuilder attributeNameBuilder = new StringBuilder(attributeNameMaxLength);

                GL.GetActiveAttrib(programHandle, i, attributeNameMaxLength,
                    out attributeNameLength, out attributeLength, out attributeType, attributeNameBuilder);

                string attributeName = attributeNameBuilder.ToString();

                if (attributeName.StartsWith("gl_", StringComparison.InvariantCulture))
                {
                    //
                    // Names starting with the reserved prefix of "gl_" have a location of -1.
                    //
                    continue;
                }

                int attributeLocation = GL.GetAttribLocation(programHandle, attributeName);

                vertexAttributes.Add(new ShaderVertexAttribute(
                    attributeName, attributeLocation, TypeConverterGL3x.To(attributeType), attributeLength));
            }

            return vertexAttributes;
        }

        private UniformCollection FindUniforms(int programHandle)
        {
            int numberOfUniforms;
            GL.GetProgram(programHandle, ProgramParameter.ActiveUniforms, out numberOfUniforms);

            int uniformNameMaxLength;
            GL.GetProgram(programHandle, ProgramParameter.ActiveUniformMaxLength, out uniformNameMaxLength);

            UniformCollection uniforms = new UniformCollection();
            for (int i = 0; i < numberOfUniforms; ++i)
            {
                int uniformNameLength;
                int uniformSize;
                ActiveUniformType uniformType;
                StringBuilder uniformNameBuilder = new StringBuilder(uniformNameMaxLength);

                GL.GetActiveUniform(programHandle, i, uniformNameMaxLength,
                    out uniformNameLength, out uniformSize, out uniformType, uniformNameBuilder);

                string uniformName = CorrectUniformName(uniformNameBuilder.ToString());

                if (uniformName.StartsWith("gl_", StringComparison.InvariantCulture))
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
                GL.GetActiveUniforms(programHandle, 1, ref i, ActiveUniformParameter.UniformBlockIndex, out uniformBlockIndex);
                if (uniformBlockIndex != -1)
                {
                    continue;
                }

                if (uniformSize != 1)
                {
                    // TODO:  Support arrays
                    throw new NotSupportedException("Uniform arrays are not supported.");
                }

                int uniformLocation = GL.GetUniformLocation(programHandle, uniformName);
                uniforms.Add(CreateUniform(uniformName, uniformLocation, uniformType));
            }

            return uniforms;
        }

        private Uniform CreateUniform(
            string name,
            int location,
            ActiveUniformType type)
        {
            switch (type)
            {
                case ActiveUniformType.Float:
                    return new UniformFloatGL3x(name, location, this);
                case ActiveUniformType.FloatVec2:
                    return new UniformFloatVector2GL3x(name, location, this);
                case ActiveUniformType.FloatVec3:
                    return new UniformFloatVector3GL3x(name, location, this);
                case ActiveUniformType.FloatVec4:
                    return new UniformFloatVector4GL3x(name, location, this);
                case ActiveUniformType.Int:
                    return new UniformIntGL3x(name, location, UniformType.Int, this);
                case ActiveUniformType.IntVec2:
                    return new UniformIntVector2GL3x(name, location, this);
                case ActiveUniformType.IntVec3:
                    return new UniformIntVector3GL3x(name, location, this);
                case ActiveUniformType.IntVec4:
                    return new UniformIntVector4GL3x(name, location, this);
                case ActiveUniformType.Bool:
                    return new UniformBoolGL3x(name, location, this);
                case ActiveUniformType.BoolVec2:
                    return new UniformBoolVector2GL3x(name, location, this);
                case ActiveUniformType.BoolVec3:
                    return new UniformBoolVector3GL3x(name, location, this);
                case ActiveUniformType.BoolVec4:
                    return new UniformBoolVector4GL3x(name, location, this);
                case ActiveUniformType.FloatMat2:
                    return new UniformFloatMatrix22GL3x(name, location, this);
                case ActiveUniformType.FloatMat3:
                    return new UniformFloatMatrix33GL3x(name, location, this);
                case ActiveUniformType.FloatMat4:
                    return new UniformFloatMatrix44GL3x(name, location, this);
                case ActiveUniformType.FloatMat2x3:
                    return new UniformFloatMatrix23GL3x(name, location, this);
                case ActiveUniformType.FloatMat2x4:
                    return new UniformFloatMatrix24GL3x(name, location, this);
                case ActiveUniformType.FloatMat3x2:
                    return new UniformFloatMatrix32GL3x(name, location, this);
                case ActiveUniformType.FloatMat3x4:
                    return new UniformFloatMatrix34GL3x(name, location, this);
                case ActiveUniformType.FloatMat4x2:
                    return new UniformFloatMatrix42GL3x(name, location, this);
                case ActiveUniformType.FloatMat4x3:
                    return new UniformFloatMatrix43GL3x(name, location, this);
                case ActiveUniformType.Sampler1D:
                case ActiveUniformType.Sampler2D:
                case ActiveUniformType.Sampler2DRect:
                case ActiveUniformType.Sampler2DRectShadow:
                case ActiveUniformType.Sampler3D:
                case ActiveUniformType.SamplerCube:
                case ActiveUniformType.Sampler1DShadow:
                case ActiveUniformType.Sampler2DShadow:
                case ActiveUniformType.Sampler1DArray:
                case ActiveUniformType.Sampler2DArray:
                case ActiveUniformType.Sampler1DArrayShadow:
                case ActiveUniformType.Sampler2DArrayShadow:
                case ActiveUniformType.SamplerCubeShadow:
                case ActiveUniformType.IntSampler1D:
                case ActiveUniformType.IntSampler2D:
                case ActiveUniformType.IntSampler2DRect:
                case ActiveUniformType.IntSampler3D:
                case ActiveUniformType.IntSamplerCube:
                case ActiveUniformType.IntSampler1DArray:
                case ActiveUniformType.IntSampler2DArray:
                case ActiveUniformType.UnsignedIntSampler1D:
                case ActiveUniformType.UnsignedIntSampler2D:
                case ActiveUniformType.UnsignedIntSampler2DRect:
                case ActiveUniformType.UnsignedIntSampler3D:
                case ActiveUniformType.UnsignedIntSamplerCube:
                case ActiveUniformType.UnsignedIntSampler1DArray:
                case ActiveUniformType.UnsignedIntSampler2DArray:
                    return new UniformIntGL3x(name, location, TypeConverterGL3x.To(type), this);
            }

            //
            // A new Uniform derived class needs to be added to support this uniform type.
            //
            throw new NotSupportedException("An implementation for uniform type " + type.ToString() + " does not exist.");
        }

        private static UniformBlockCollection FindUniformBlocks(int programHandle)
        {
            int numberOfUniformBlocks;
            GL.GetProgram(programHandle, ProgramParameter.ActiveUniformBlocks, out numberOfUniformBlocks);

            UniformBlockCollection uniformBlocks = new UniformBlockCollection();
            for (int i = 0; i < numberOfUniformBlocks; ++i)
            {
                string uniformBlockName = GL.GetActiveUniformBlockName(programHandle, i);

                int uniformBlockSizeInBytes;
                GL.GetActiveUniformBlock(programHandle, i, ActiveUniformBlockParameter.UniformBlockDataSize, out uniformBlockSizeInBytes);

                int numberOfUniformsInBlock;
                GL.GetActiveUniformBlock(programHandle, i, ActiveUniformBlockParameter.UniformBlockActiveUniforms, out numberOfUniformsInBlock);

                int[] uniformIndicesInBlock = new int[numberOfUniformsInBlock];
                GL.GetActiveUniformBlock(programHandle, i, ActiveUniformBlockParameter.UniformBlockActiveUniformIndices, uniformIndicesInBlock);

                //
                // Query uniforms in this named uniform block
                //
                int[] uniformTypes = new int[numberOfUniformsInBlock];
                int[] uniformOffsetsInBytes = new int[numberOfUniformsInBlock];
                int[] uniformLengths = new int[numberOfUniformsInBlock];
                int[] uniformArrayStridesInBytes = new int[numberOfUniformsInBlock];
                int[] uniformmatrixStrideInBytess = new int[numberOfUniformsInBlock];
                int[] uniformRowMajors = new int[numberOfUniformsInBlock];
                GL.GetActiveUniforms(programHandle, numberOfUniformsInBlock, uniformIndicesInBlock, ActiveUniformParameter.UniformType, uniformTypes);
                GL.GetActiveUniforms(programHandle, numberOfUniformsInBlock, uniformIndicesInBlock, ActiveUniformParameter.UniformOffset, uniformOffsetsInBytes);
                GL.GetActiveUniforms(programHandle, numberOfUniformsInBlock, uniformIndicesInBlock, ActiveUniformParameter.UniformSize, uniformLengths);
                GL.GetActiveUniforms(programHandle, numberOfUniformsInBlock, uniformIndicesInBlock, ActiveUniformParameter.UniformArrayStride, uniformArrayStridesInBytes);
                GL.GetActiveUniforms(programHandle, numberOfUniformsInBlock, uniformIndicesInBlock, ActiveUniformParameter.UniformMatrixStride, uniformmatrixStrideInBytess);
                GL.GetActiveUniforms(programHandle, numberOfUniformsInBlock, uniformIndicesInBlock, ActiveUniformParameter.UniformIsRowMajor, uniformRowMajors);

                UniformBlock uniformBlock = new UniformBlockGL3x(uniformBlockName, uniformBlockSizeInBytes, i);

                for (int j = 0; j < numberOfUniformsInBlock; ++j)
                {
                    string uniformName = GL.GetActiveUniformName(programHandle, uniformIndicesInBlock[j]);
                    uniformName = CorrectUniformName(uniformName);

                    UniformType uniformType = TypeConverterGL3x.To((ActiveUniformType)uniformTypes[j]);

                    uniformBlock.Members.Add(CreateUniformBlockMember(uniformName,
                        uniformType, uniformOffsetsInBytes[j], uniformLengths[j], uniformArrayStridesInBytes[j],
                        uniformmatrixStrideInBytess[j], uniformRowMajors[j]));
                }

                uniformBlocks.Add(uniformBlock);

                //
                // Create a one to one mapping between uniform blocks and uniform buffer objects.
                //
                GL.UniformBlockBinding(programHandle, i, i);
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
            if (name.EndsWith("[0]", StringComparison.InvariantCulture))
            {
                return name.Remove(name.Length - 3);
            }

            return name;
        }

        internal ShaderProgramNameGL3x Handle
        {
            get { return _program; }
        }

        internal void Bind()
        {
            GL.UseProgram(_program.Value);
        }

        internal void Clean(Context context, DrawState drawState, SceneState sceneState)
        {
            SetDrawAutomaticUniforms(context, drawState, sceneState);

            for (int i = 0; i < _dirtyUniforms.Count; ++i)
            {
                _dirtyUniforms[i].Clean();
            }
            _dirtyUniforms.Clear();
        }

        private string ProgramInfoLog
        {
            get { return GL.GetProgramInfoLog(_program.Value); }
        }

        #region ShaderProgram Members

        public override string Log
        {
            get { return ProgramInfoLog; }
        }

        public override TransformFeedbackOutputCollection TransformFeedbackOutputs
        {
            get { return _transformFeedbackOutputs; }
        }

        public override TransformFeedbackAttributeLayout TransformFeedbackAttributeLayout
        {
            get { return _transformFeedbackAttributeLayout; }
        }

        public override FragmentOutputs FragmentOutputs
        {
            get { return _fragmentOutputs; }
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

        #region ICleanableObserver Members

        public void NotifyDirty(ICleanable value)
        {
            _dirtyUniforms.Add(value);
        }

        #endregion

        #region Disposable Members

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_program != null)
                {
                    _program.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #endregion

        private readonly ShaderProgramNameGL3x _program;
        private readonly TransformFeedbackOutputCollection _transformFeedbackOutputs;
        private readonly TransformFeedbackAttributeLayout _transformFeedbackAttributeLayout;
        private readonly FragmentOutputsGL3x _fragmentOutputs;
        private readonly ShaderVertexAttributeCollection _vertexAttributes;
        private readonly IList<ICleanable> _dirtyUniforms;
        private readonly UniformCollection _uniforms;
        private readonly UniformBlockCollection _uniformBlocks;
    }
}
