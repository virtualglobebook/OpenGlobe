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
using System.IO;
using System.Globalization;
using System.Diagnostics;
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
            bool hasGS = (geometryShaderSource != string.Empty);
            using (ShaderObjectGL3x vertexShader = new ShaderObjectGL3x(ShaderType.VertexShader, vertexShaderSource))
            using (ShaderObjectGL3x geometryShader = hasGS ? new ShaderObjectGL3x(ShaderType.GeometryShaderExt, geometryShaderSource) : null)
            using (ShaderObjectGL3x fragmentShader = new ShaderObjectGL3x(ShaderType.FragmentShader, fragmentShaderSource))
            {
                GL.AttachShader(programHandle, vertexShader.Handle);
                if (hasGS)
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
            _fragmentOutputs = new FragmentOutputsGL3x(programHandle);
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
            Rebuild();
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

        private void ShaderChanged()
        {
            _shaderChanged = true;
        }

        private void Rebuild()
        {
            // TODO:  This function doesn't take into account transform feedback.

            //
            // Recompile and link if a shader's source file changed.
            //
            // This makes the huge assumption that the uniform locations,
            // attribute locations, etc. did not change, nor were any
            // uniforms added or removed.  In time, we may relax these
            // assumptions.
            //
            if (_shaderChanged)
            {
                _shaderChanged = false;

                ShaderProgramNameGL3x program = null;
                try
                {
                    program = new ShaderProgramNameGL3x();
                    int programHandle = program.Value;

                    bool hasGS = (_watch.GeometryShaderFilePath != string.Empty);
                    string vs = File.ReadAllText(_watch.VertexShaderFilePath);
                    string gs = hasGS ? File.ReadAllText(_watch.GeometryShaderFilePath) : string.Empty;
                    string fs = File.ReadAllText(_watch.FragmentShaderFilePath);

                    using (ShaderObjectGL3x vertexShader = new ShaderObjectGL3x(ShaderType.VertexShader, vs))
                    using (ShaderObjectGL3x geometryShader = hasGS ? new ShaderObjectGL3x(ShaderType.GeometryShaderExt, gs) : null)
                    using (ShaderObjectGL3x fragmentShader = new ShaderObjectGL3x(ShaderType.FragmentShader, fs))
                    {
                        GL.AttachShader(programHandle, vertexShader.Handle);
                        if (hasGS)
                        {
                            GL.AttachShader(programHandle, geometryShader.Handle);
                        }
                        GL.AttachShader(programHandle, fragmentShader.Handle);
                    }

                    GL.LinkProgram(programHandle);

                    int linkStatus;
                    GL.GetProgram(programHandle, ProgramParameter.LinkStatus, out linkStatus);

                    if (linkStatus == 0)
                    {
                        Debug.WriteLine("\nCould not link shader program.  Link Log:  \n\n" + ProgramInfoLog);
                        throw new Exception();
                    }

                    VerifyRebuild(programHandle);

                    _program.Dispose();
                    _program = program;
                    _fragmentOutputs = new FragmentOutputsGL3x(programHandle);

                    //
                    // All uniforms are now dirty.  To keep everything consistent, 
                    // we should also set their private dirty flag to true, but that is a pain.
                    //
                    _dirtyUniforms.Clear();
                    for (int i = 0; i < _uniforms.Count; ++i)
                    {
                        _dirtyUniforms.Add(_uniforms[i] as ICleanable);
                    }
                }
                catch
                {
                    //
                    // Shader program failed to compile/link.  See
                    // Debug Output window for details.
                    //
                    Debugger.Break();

                    if (program != null)
                    {
                        program.Dispose();
                    }
                }
            }
        }

        private void VerifyRebuild(int programHandle)
        {
            //
            // Verify that vertex attributes, uniforms, and uniform blocks did not change.
            // 
            // Requiring an application recompile for these types of changes is a limitation
            // of our engine.  With enough effort, we could remove some or all of these.
            //
            // In particular, it would be reasonable to allow the addition and removal
            // of automatic uniforms.
            //
            VerifyRebuildVertexAttributes(programHandle);
            VerifyRebuildUniforms(programHandle);
            VerifyRebuildUniformsBlocks(programHandle);
        }

        private void VerifyRebuildVertexAttributes(int programHandle)
        {
            ShaderVertexAttributeCollection vertexAttributes = FindVertexAttributes(programHandle);

            if (vertexAttributes.Count != _vertexAttributes.Count)
            {
                Debug.WriteLine("\nRecompiling/linking shader changed the number of vertex attributes from " +
                    _vertexAttributes.Count.ToString(NumberFormatInfo.InvariantInfo) + " to " +
                    vertexAttributes.Count.ToString(NumberFormatInfo.InvariantInfo) + ".  The application must be recompiled.");
                throw new Exception();
            }

            for (int i = 0; i < vertexAttributes.Count; ++i)
            {
                ShaderVertexAttribute newAttribute = vertexAttributes[i];
                ShaderVertexAttribute oldAttribute = _vertexAttributes[i];

                if ((newAttribute.Name != oldAttribute.Name) ||
                    (newAttribute.Location != oldAttribute.Location) ||
                    (newAttribute.Datatype != oldAttribute.Datatype) ||
                    (newAttribute.Length != oldAttribute.Length))
                {
                    Debug.WriteLine("\nRecompiling/linking shader changed a vertex attribute:\n" +
                        "\t Old name: [" + oldAttribute.Name + "] New name: [" + newAttribute.Name + "]\n" +
                        "\t Old location: [" + oldAttribute.Location.ToString(NumberFormatInfo.InvariantInfo) + "] New location: [" + newAttribute.Location.ToString(NumberFormatInfo.InvariantInfo) + "]\n" +
                        "\t Old datatype: [" + StringConverterGL3x.ShaderVertexAttributeTypeToString(oldAttribute.Datatype) + "] New datatype: [" + StringConverterGL3x.ShaderVertexAttributeTypeToString(newAttribute.Datatype) + "]\n" +
                        "\t Old length: [" + oldAttribute.Length.ToString(NumberFormatInfo.InvariantInfo) + "] New length: [" + newAttribute.Length.ToString(NumberFormatInfo.InvariantInfo) + "]\n" +
                        "The application must be recompiled.");
                    throw new Exception();
                }
            }
        }

        private void VerifyRebuildUniforms(int programHandle)
        {
            UniformCollection uniforms = FindUniforms(programHandle);

            if (uniforms.Count != _uniforms.Count)
            {
                Debug.WriteLine("\nRecompiling/linking shader changed the number of uniforms from " +
                    _uniforms.Count.ToString(NumberFormatInfo.InvariantInfo) + " to " +
                    uniforms.Count.ToString(NumberFormatInfo.InvariantInfo) + ".\nThe application must be recompiled.");

                if (uniforms.Count < _uniforms.Count)
                {
                    Debug.WriteLine("Even if you did not explicitly remove a uniform, a code change may have allowed the compiler/linked to optimize out a uniform.");
                }

                throw new Exception();
            }

            for (int i = 0; i < uniforms.Count; ++i)
            {
                Uniform newUniform = uniforms[i];
                Uniform oldUniform = _uniforms[i];

                if ((newUniform.Name != oldUniform.Name) || (newUniform.Datatype != oldUniform.Datatype))
                {
                    Debug.WriteLine("\nRecompiling/linking shader changed a uniform:\n" +
                        "\t Old name: [" + oldUniform.Name + "] New name: [" + newUniform.Name + "]\n" +
                        "\t Old datatype: [" + StringConverterGL3x.UniformTypeToString(oldUniform.Datatype) + "] New datatype: [" + StringConverterGL3x.UniformTypeToString(newUniform.Datatype) + "]\n" +
                        "The application must be recompiled.");
                    throw new Exception();
                }
            }
        }

        private void VerifyRebuildUniformsBlocks(int programHandle)
        {
            UniformBlockCollection uniformBlocks = FindUniformBlocks(programHandle);

            if (uniformBlocks.Count != _uniformBlocks.Count)
            {
                Debug.WriteLine("\nRecompiling/linking shader changed the number of uniform blocks from " +
                    _uniformBlocks.Count.ToString(NumberFormatInfo.InvariantInfo) + " to " +
                    uniformBlocks.Count.ToString(NumberFormatInfo.InvariantInfo) + ".  The application must be recompiled.");
                throw new Exception();
            }

            for (int i = 0; i < uniformBlocks.Count; ++i)
            {
                UniformBlock newBlock = uniformBlocks[i];
                UniformBlock oldBlock = _uniformBlocks[i];

                if ((newBlock.Name != oldBlock.Name) || 
                    (newBlock.SizeInBytes != oldBlock.SizeInBytes) ||
                    (newBlock.Members.Count != oldBlock.Members.Count))
                {
                    Debug.WriteLine("\nRecompiling/linking shader changed a uniform block:\n" +
                        "\t Old name: [" + oldBlock.Name + "] New name: [" + newBlock.Name + "]\n" +
                        "\t Old size in bytes: [" + oldBlock.SizeInBytes + "] New size in bytes: [" + newBlock.SizeInBytes + "]\n" +
                        "\t Old members count: [" + oldBlock.Members.Count + "] New members count: [" + newBlock.Members.Count + "]\n" +
                        "The application must be recompiled.");
                    throw new Exception();
                }

                for (int j = 0; j < newBlock.Members.Count; ++j)
                {
                    UniformBlockMember newMember = newBlock.Members[j];
                    UniformBlockMember oldMember = oldBlock.Members[j];

                    if ((newMember.Name != oldMember.Name) ||
                        (newMember.Datatype != oldMember.Datatype) ||
                        (newMember.OffsetInBytes != oldMember.OffsetInBytes))
                    {
                        Debug.WriteLine("\nRecompiling/linking shader changed a member of a uniform block:\n" +
                            "\t Uniform block: [" + newBlock.Name + "]\n" +
                            "\t Old name: [" + oldMember.Name + "] New name: [" + newMember.Name + "]\n" +
                            "\t Old datatype: [" + StringConverterGL3x.UniformTypeToString(oldMember.Datatype) + "] New datatype: [" + StringConverterGL3x.UniformTypeToString(newMember.Datatype) + "]\n" +
                            "\t Old offset in bytes: [" + newMember.OffsetInBytes + "] New offset in bytes: [" + oldMember.OffsetInBytes + "]\n" +
                            "The application must be recompiled.");
                    }
                }
            }
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
        
        public override void StartWatch(
            string vertexShaderFilePath,
            string geometryShaderFilePath,
            string fragmentShaderFilePath)
        {
            StopWatch();
            _watch = new ShaderWatch(
                vertexShaderFilePath,
                geometryShaderFilePath,
                fragmentShaderFilePath,
                ShaderChanged);
        }

        public override void StopWatch()
        {
            if (_watch != null)
            {
                _watch.Dispose();
                _watch = null;
            }
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

                if (_watch != null)
                {
                    _watch.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #endregion

        private ShaderProgramNameGL3x _program;
        private readonly TransformFeedbackOutputCollection _transformFeedbackOutputs;
        private readonly TransformFeedbackAttributeLayout _transformFeedbackAttributeLayout;
        private FragmentOutputsGL3x _fragmentOutputs;
        private readonly ShaderVertexAttributeCollection _vertexAttributes;
        private readonly IList<ICleanable> _dirtyUniforms;
        private readonly UniformCollection _uniforms;
        private readonly UniformBlockCollection _uniformBlocks;

        private ShaderWatch _watch;
        private bool _shaderChanged;
    }
}
