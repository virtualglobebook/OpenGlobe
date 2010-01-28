#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using NUnit.Framework;
using OpenTK;

namespace MiniGlobe.Renderer
{
    [TestFixture]
    public class ShaderProgramTests
    {
        [Test]
        public void PassThrough()
        {
            MiniGlobeWindow window = Device.CreateWindow(1, 1);

            ShaderProgram sp = Device.CreateShaderProgram(ShaderSources.PassThroughVertexShader(), ShaderSources.PassThroughFragmentShader());
            Assert.IsFalse(sp.LinkLog.Contains("warning"));
            Assert.IsEmpty(sp.Uniforms);
            Assert.IsEmpty(sp.UniformBlocks);

            Assert.AreEqual(1, sp.VertexAttributes.Count);
            Assert.IsTrue(sp.VertexAttributes.Contains("position"));

            ShaderVertexAttribute attribute = sp.VertexAttributes["position"];
            Assert.AreEqual("position", attribute.Name);
            Assert.AreEqual(ShaderVertexAttributeType.FloatVector4, attribute.DataType);
            Assert.AreEqual(1, attribute.Length);

            sp.Dispose();
            window.Dispose();
        }

        [Test]
        public void PassThroughWithGeometryShader()
        {
            MiniGlobeWindow window = Device.CreateWindow(1, 1);

            ShaderProgram sp = Device.CreateShaderProgram(
                ShaderSources.PassThroughVertexShader(),
                ShaderSources.PassThroughGeometryShader(),
                ShaderSources.PassThroughFragmentShader());

            Assert.IsFalse(sp.LinkLog.Contains("warning"));
            Assert.IsEmpty(sp.Uniforms);
            Assert.IsEmpty(sp.UniformBlocks);

            Assert.AreEqual(1, sp.VertexAttributes.Count);
            Assert.IsTrue(sp.VertexAttributes.Contains("position"));

            ShaderVertexAttribute attribute = sp.VertexAttributes["position"];
            Assert.AreEqual("position", attribute.Name);
            Assert.AreEqual(ShaderVertexAttributeType.FloatVector4, attribute.DataType);
            Assert.AreEqual(1, attribute.Length);

            sp.Dispose();
            window.Dispose();
        }

        [Test]
        public void DefaultUniformBlock()
        {
            MiniGlobeWindow window = Device.CreateWindow(1, 1);

            string fs =
                @"#version 150
                  #extension GL_EXT_gpu_shader4 : enable        // for texture1DArray

                  uniform float exampleFloat = 0.25f;
                  uniform vec2  exampleVec2  = vec2(0.0f, 0.0f);
                  uniform vec3  exampleVec3  = vec3(0.0f, 1.0f, 2.0f);
                  uniform vec4  exampleVec4  = vec4(0.0f, 1.0f, 2.0f, 3.0f);

                  uniform int   exampleInt   = 2;
                  uniform ivec2 exampleIVec2 = ivec2(0, 1);
                  uniform ivec3 exampleIVec3 = ivec3(0, 1, 2);
                  uniform ivec4 exampleIVec4 = ivec4(0, 1, 2, 3);

                  uniform bool  exampleBool  = false;
                  uniform bvec2 exampleBVec2 = bvec2(false, true);
                  uniform bvec3 exampleBVec3 = bvec3(false, true, false);
                  uniform bvec4 exampleBVec4 = bvec4(false, true, false, true);

                  uniform mat4   exampleMat4  = mat4(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15);
                  uniform mat3   exampleMat3  = mat3(0, 1, 2, 3, 4, 5, 6, 7, 8);
                  uniform mat2   exampleMat2  = mat2(0, 1, 2, 3);
                  uniform mat2x3 exampleMat23 = mat2x3(0, 1, 2, 3, 4, 5);
                  uniform mat2x4 exampleMat24 = mat2x4(0, 1, 2, 3, 4, 5, 6, 7);
                  uniform mat3x2 exampleMat32 = mat3x2(0, 1, 2, 3, 4, 5);
                  uniform mat3x4 exampleMat34 = mat3x4(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11);
                  uniform mat4x2 exampleMat42 = mat4x2(0, 1, 2, 3, 4, 5, 6, 7);
                  uniform mat4x3 exampleMat43 = mat4x3(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11);

                  uniform sampler2D exampleSampler2D;
                  uniform sampler1DArray exampleSampler1DArray;

                  out vec4 FragColor;

                  void main()
                  {
                      float red = 0;

                      red += exampleFloat;
                      red += exampleVec2.x;
                      red += exampleVec3.x;
                      red += exampleVec4.x;
                      red += float(exampleInt);
                      red += float(exampleIVec2.x);
                      red += float(exampleIVec3.x);
                      red += float(exampleIVec4.x);
                      red += float(exampleBool);
                      red += float(exampleBVec2.x);
                      red += float(exampleBVec3.x);
                      red += float(exampleBVec4.x);
                      red += exampleMat4[0].x;
                      red += exampleMat3[0].x;
                      red += exampleMat2[0].x;
                      red += exampleMat23[0].x;
                      red += exampleMat24[0].x;
                      red += exampleMat32[0].x;
                      red += exampleMat34[0].x;
                      red += exampleMat42[0].x;
                      red += exampleMat43[0].x;
                      red += texture2D(exampleSampler2D, vec2(0, 0)).r;
                      red += texture1DArray(exampleSampler1DArray, vec2(0, 0)).r;

                      FragColor = vec4(red, 0, 0, 1);
                  }";

            ShaderProgram sp = Device.CreateShaderProgram(ShaderSources.PassThroughVertexShader(), fs);
            Assert.IsFalse(sp.LinkLog.Contains("warning"));
            Assert.IsEmpty(sp.UniformBlocks);
            Assert.AreEqual(23, sp.Uniforms.Count);

            //
            // I'm not verifying uniform initializer values because there may be some driver bugs:
            //
            //    http://www.opengl.org/discussion_boards/ubbthreads.php?ubb=showflat&Main=48817&Number=250791#Post250791
            //    http://www.opengl.org/discussion_boards/ubbthreads.php?ubb=showflat&Main=51624&Number=266719#Post266719
            //

            Uniform<float> exampleFloat = sp.Uniforms["exampleFloat"] as Uniform<float>;
            Assert.AreEqual("exampleFloat", exampleFloat.Name);
            Assert.AreEqual(UniformType.Float, exampleFloat.DataType);
            exampleFloat.Value = 0.75f;
            Assert.AreEqual(0.75f, exampleFloat.Value);

            Uniform<Vector2> exampleVec2 = sp.Uniforms["exampleVec2"] as Uniform<Vector2>;
            Assert.AreEqual("exampleVec2", exampleVec2.Name);
            Assert.AreEqual(UniformType.FloatVector2, exampleVec2.DataType);
            exampleVec2.Value = new Vector2(1, 0.5f);
            Assert.AreEqual(new Vector2(1, 0.5f), exampleVec2.Value);

            Uniform<Vector3> exampleVec3 = sp.Uniforms["exampleVec3"] as Uniform<Vector3>;
            Assert.AreEqual("exampleVec3", exampleVec3.Name);
            Assert.AreEqual(UniformType.FloatVector3, exampleVec3.DataType);
            exampleVec3.Value = new Vector3(1, 0, 0);
            Assert.AreEqual(new Vector3(1, 0, 0), exampleVec3.Value);

            Uniform<Vector4> exampleVec4 = sp.Uniforms["exampleVec4"] as Uniform<Vector4>;
            Assert.AreEqual("exampleVec4", exampleVec4.Name);
            Assert.AreEqual(UniformType.FloatVector4, exampleVec4.DataType);
            exampleVec4.Value = new Vector4(1, 0, 0, 0);
            Assert.AreEqual(new Vector4(1, 0, 0, 0), exampleVec4.Value);

            ///////////////////////////////////////////////////////////////////

            Uniform<int> exampleInt = sp.Uniforms["exampleInt"] as Uniform<int>;
            Assert.AreEqual("exampleInt", exampleInt.Name);
            Assert.AreEqual(UniformType.Int, exampleInt.DataType);
            exampleInt.Value = 1;
            Assert.AreEqual(1, exampleInt.Value);

            Uniform<Vector2i> exampleIVec2 = sp.Uniforms["exampleIVec2"] as Uniform<Vector2i>;
            Assert.AreEqual("exampleIVec2", exampleIVec2.Name);
            Assert.AreEqual(UniformType.IntVector2, exampleIVec2.DataType);
            exampleIVec2.Value = new Vector2i(1, 0);
            Assert.AreEqual(new Vector2i(1, 0), exampleIVec2.Value);

            Uniform<Vector3i> exampleIVec3 = sp.Uniforms["exampleIVec3"] as Uniform<Vector3i>;
            Assert.AreEqual("exampleIVec3", exampleIVec3.Name);
            Assert.AreEqual(UniformType.IntVector3, exampleIVec3.DataType);
            exampleIVec3.Value = new Vector3i(1, 0, 0);
            Assert.AreEqual(new Vector3i(1, 0, 0), exampleIVec3.Value);

            Uniform<Vector4i> exampleIVec4 = sp.Uniforms["exampleIVec4"] as Uniform<Vector4i>;
            Assert.AreEqual("exampleIVec4", exampleIVec4.Name);
            Assert.AreEqual(UniformType.IntVector4, exampleIVec4.DataType);
            exampleIVec4.Value = new Vector4i(1, 0, 0, 0);
            Assert.AreEqual(new Vector4i(1, 0, 0, 0), exampleIVec4.Value);

            ///////////////////////////////////////////////////////////////////

            Uniform<bool> exampleBool = sp.Uniforms["exampleBool"] as Uniform<bool>;
            Assert.AreEqual("exampleBool", exampleBool.Name);
            Assert.AreEqual(UniformType.Bool, exampleBool.DataType);
            exampleBool.Value = true;
            Assert.AreEqual(true, exampleBool.Value);

            Uniform<Vector2b> exampleBVec2 = sp.Uniforms["exampleBVec2"] as Uniform<Vector2b>;
            Assert.AreEqual("exampleBVec2", exampleBVec2.Name);
            Assert.AreEqual(UniformType.BoolVector2, exampleBVec2.DataType);
            exampleBVec2.Value = new Vector2b(true, false);
            Assert.AreEqual(new Vector2b(true, false), exampleBVec2.Value);

            Uniform<Vector3b> exampleBVec3 = sp.Uniforms["exampleBVec3"] as Uniform<Vector3b>;
            Assert.AreEqual("exampleBVec3", exampleBVec3.Name);
            Assert.AreEqual(UniformType.BoolVector3, exampleBVec3.DataType);
            exampleBVec3.Value = new Vector3b(true, false, false);
            Assert.AreEqual(new Vector3b(true, false, false), exampleBVec3.Value);

            Uniform<Vector4b> exampleBVec4 = sp.Uniforms["exampleBVec4"] as Uniform<Vector4b>;
            Assert.AreEqual("exampleBVec4", exampleBVec4.Name);
            Assert.AreEqual(UniformType.BoolVector4, exampleBVec4.DataType);
            exampleBVec4.Value = new Vector4b(true, false, false, false);
            Assert.AreEqual(new Vector4b(true, false, false, false), exampleBVec4.Value);

            ///////////////////////////////////////////////////////////////////

            Uniform<int> exampleSampler2D = sp.Uniforms["exampleSampler2D"] as Uniform<int>;
            Assert.AreEqual("exampleSampler2D", exampleSampler2D.Name);
            Assert.AreEqual(UniformType.Sampler2D, exampleSampler2D.DataType);
            exampleSampler2D.Value = 1;
            Assert.AreEqual(1, exampleSampler2D.Value);

            Uniform<int> exampleSampler1DArray = sp.Uniforms["exampleSampler1DArray"] as Uniform<int>;
            Assert.AreEqual("exampleSampler1DArray", exampleSampler1DArray.Name);
            Assert.AreEqual(UniformType.Sampler1DArray, exampleSampler1DArray.DataType);
            exampleSampler1DArray.Value = 1;
            Assert.AreEqual(1, exampleSampler1DArray.Value);

            ///////////////////////////////////////////////////////////////////

            Matrix4 m4 = new Matrix4(
                new Vector4(0, 0, 0, 0),
                new Vector4(0.25f, 0, 0, 0),
                new Vector4(0, 0, 0, 0),
                new Vector4(0, 0, 0, 0));
            Uniform<Matrix4> exampleMat4 = sp.Uniforms["exampleMat4"] as Uniform<Matrix4>;
            Assert.AreEqual("exampleMat4", exampleMat4.Name);
            Assert.AreEqual(UniformType.FloatMatrix44, exampleMat4.DataType);
            exampleMat4.Value = m4;
            Assert.AreEqual(m4, exampleMat4.Value);

            Matrix3 m3 = new Matrix3(
                new Vector3(0, 0, 0),
                new Vector3(0, 0, 0.25f),
                new Vector3(0, 0, 0));
            Uniform<Matrix3> exampleMat3 = sp.Uniforms["exampleMat3"] as Uniform<Matrix3>;
            Assert.AreEqual("exampleMat3", exampleMat3.Name);
            Assert.AreEqual(UniformType.FloatMatrix33, exampleMat3.DataType);
            exampleMat3.Value = m3;
            Assert.AreEqual(m3, exampleMat3.Value);

            Matrix2 m2 = new Matrix2(
                new Vector2(0, 1),
                new Vector2(0, 0));
            Uniform<Matrix2> exampleMat2 = sp.Uniforms["exampleMat2"] as Uniform<Matrix2>;
            Assert.AreEqual("exampleMat2", exampleMat2.Name);
            Assert.AreEqual(UniformType.FloatMatrix22, exampleMat2.DataType);
            exampleMat2.Value = m2;
            Assert.AreEqual(m2, exampleMat2.Value);

            Matrix23 m23 = new Matrix23(
                new Vector2(0, 0),
                new Vector2(0, 0.25f),
                new Vector2(0, 0));
            Uniform<Matrix23> exampleMat23 = sp.Uniforms["exampleMat23"] as Uniform<Matrix23>;
            Assert.AreEqual("exampleMat23", exampleMat23.Name);
            Assert.AreEqual(UniformType.FloatMatrix23, exampleMat23.DataType);
            exampleMat23.Value = m23;
            Assert.AreEqual(m23, exampleMat23.Value);

            Matrix24 m24 = new Matrix24(
                new Vector2(0, 0),
                new Vector2(0, 0),
                new Vector2(0.25f, 0),
                new Vector2(0, 0));
            Uniform<Matrix24> exampleMat24 = sp.Uniforms["exampleMat24"] as Uniform<Matrix24>;
            Assert.AreEqual("exampleMat24", exampleMat24.Name);
            Assert.AreEqual(UniformType.FloatMatrix24, exampleMat24.DataType);
            exampleMat24.Value = m24;
            Assert.AreEqual(m24, exampleMat24.Value);

            Matrix32 m32 = new Matrix32(
                new Vector3(0, 0, 0),
                new Vector3(0, 0.25f, 0));
            Uniform<Matrix32> exampleMat32 = sp.Uniforms["exampleMat32"] as Uniform<Matrix32>;
            Assert.AreEqual("exampleMat32", exampleMat32.Name);
            Assert.AreEqual(UniformType.FloatMatrix32, exampleMat32.DataType);
            exampleMat32.Value = m32;
            Assert.AreEqual(m32, exampleMat32.Value);

            Matrix34 m34 = new Matrix34(
                new Vector3(0, 0, 0),
                new Vector3(0, 0, 0),
                new Vector3(0.25f, 0, 0),
                new Vector3(0, 0, 0));
            Uniform<Matrix34> exampleMat34 = sp.Uniforms["exampleMat34"] as Uniform<Matrix34>;
            Assert.AreEqual("exampleMat34", exampleMat34.Name);
            Assert.AreEqual(UniformType.FloatMatrix34, exampleMat34.DataType);
            exampleMat34.Value = m34;
            Assert.AreEqual(m34, exampleMat34.Value);

            Matrix42 m42 = new Matrix42(
                new Vector4(0, 0, 0, 0.25f),
                new Vector4(0, 0, 0, 0));
            Uniform<Matrix42> exampleMat42 = sp.Uniforms["exampleMat42"] as Uniform<Matrix42>;
            Assert.AreEqual("exampleMat42", exampleMat42.Name);
            Assert.AreEqual(UniformType.FloatMatrix42, exampleMat42.DataType);
            exampleMat42.Value = m42;
            Assert.AreEqual(m42, exampleMat42.Value);

            Matrix43 m43 = new Matrix43(
                new Vector4(0, 0, 0, 0),
                new Vector4(0, 0, 0, 0.25f),
                new Vector4(0, 0, 0, 0));
            Uniform<Matrix43> exampleMat43 = sp.Uniforms["exampleMat43"] as Uniform<Matrix43>;
            Assert.AreEqual("exampleMat43", exampleMat43.Name);
            Assert.AreEqual(UniformType.FloatMatrix43, exampleMat43.DataType);
            exampleMat43.Value = m43;
            Assert.AreEqual(m43, exampleMat43.Value);

            sp.Dispose();
            window.Dispose();
        }

        [Test]
        public void LinkAutomaticUniforms()
        {
            MiniGlobeWindow window = Device.CreateWindow(1, 1);

            string fs =
                @"#version 150
                 
                  uniform sampler2D mg_Texture0;
                  uniform sampler2D mg_Texture1;
                  uniform sampler2D mg_Texture2;
                  uniform sampler2D mg_Texture3;
                  out vec4 FragColor;

                  void main()
                  {
                      FragColor = 
                          texture2D(mg_Texture0, vec2(0, 0)) + 
                          texture2D(mg_Texture1, vec2(0, 0)) + 
                          texture2D(mg_Texture2, vec2(0, 0)) + 
                          texture2D(mg_Texture3, vec2(0, 0));
                  }";
            ShaderProgram sp = Device.CreateShaderProgram(ShaderSources.PassThroughVertexShader(), fs);
            Assert.AreEqual(0, (sp.Uniforms["mg_Texture0"] as Uniform<int>).Value);
            Assert.AreEqual(1, (sp.Uniforms["mg_Texture1"] as Uniform<int>).Value);
            Assert.AreEqual(2, (sp.Uniforms["mg_Texture2"] as Uniform<int>).Value);
            Assert.AreEqual(3, (sp.Uniforms["mg_Texture3"] as Uniform<int>).Value);

            sp.Dispose();
            window.Dispose();
        }

        [Test]
        public void UniformBlock()
        {
            MiniGlobeWindow window = Device.CreateWindow(1, 1);

            ShaderProgram sp = Device.CreateShaderProgram(
                ShaderSources.PassThroughVertexShader(),
                ShaderSources.RedUniformBlockFragmentShader());
            Assert.IsFalse(sp.LinkLog.Contains("warning"));
            Assert.IsEmpty(sp.Uniforms);
            Assert.AreEqual(1, sp.UniformBlocks.Count);

            UniformBlock redBlock = sp.UniformBlocks["RedBlock"];
            Assert.AreEqual("RedBlock", redBlock.Name);
            Assert.AreEqual(16, redBlock.SizeInBytes);
            Assert.AreEqual(1, redBlock.Members.Count);

            UniformBlockMember red = redBlock.Members["red"];
            Assert.AreEqual("red", red.Name);
            Assert.AreEqual(0, red.OffsetInBytes);
            Assert.AreEqual(UniformType.Float, red.DataType);

            sp.Dispose();
            window.Dispose();
        }
    }
}
