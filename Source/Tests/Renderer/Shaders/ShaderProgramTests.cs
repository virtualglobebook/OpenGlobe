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
using MiniGlobe.Core;

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

            //
            // Uniform initializers are not supported because there may be some driver bugs:
            //
            //    http://www.opengl.org/discussion_boards/ubbthreads.php?ubb=showflat&Main=48817&Number=250791#Post250791
            //    http://www.opengl.org/discussion_boards/ubbthreads.php?ubb=showflat&Main=51624&Number=266719#Post266719
            //    http://www.opengl.org/discussion_boards/ubbthreads.php?ubb=showflat&Main=52018&Number=268866#Post268866
            //
            string fs =
                @"#version 150
                  #extension GL_EXT_gpu_shader4 : enable        // for texture1DArray

                  uniform float exampleFloat;
                  uniform vec2  exampleVec2;
                  uniform vec3  exampleVec3;
                  uniform vec4  exampleVec4;

                  uniform int   exampleInt;
                  uniform ivec2 exampleIVec2;
                  uniform ivec3 exampleIVec3;
                  uniform ivec4 exampleIVec4;

                  uniform bool  exampleBool;
                  uniform bvec2 exampleBVec2;
                  uniform bvec3 exampleBVec3;
                  uniform bvec4 exampleBVec4;

                  uniform mat4   exampleMat4;
                  uniform mat3   exampleMat3;
                  uniform mat2   exampleMat2;
                  uniform mat2x3 exampleMat23;
                  uniform mat2x4 exampleMat24;
                  uniform mat3x2 exampleMat32;
                  uniform mat3x4 exampleMat34;
                  uniform mat4x2 exampleMat42;
                  uniform mat4x3 exampleMat43;

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
                      red += texture(exampleSampler2D, vec2(0, 0)).r;
                      red += texture1DArray(exampleSampler1DArray, vec2(0, 0)).r;

                      FragColor = vec4(red, 0, 0, 1);
                  }";

            ShaderProgram sp = Device.CreateShaderProgram(ShaderSources.PassThroughVertexShader(), fs);
            Assert.IsFalse(sp.LinkLog.Contains("warning"));
            Assert.IsEmpty(sp.UniformBlocks);
            Assert.AreEqual(23, sp.Uniforms.Count);

            Uniform<float> exampleFloat = sp.Uniforms["exampleFloat"] as Uniform<float>;
            Assert.AreEqual("exampleFloat", exampleFloat.Name);
            Assert.AreEqual(UniformType.Float, exampleFloat.DataType);
            Assert.AreEqual(0, exampleFloat.Value);
            exampleFloat.Value = 0.75f;
            Assert.AreEqual(0.75f, exampleFloat.Value);

            Uniform<Vector2S> exampleVec2 = sp.Uniforms["exampleVec2"] as Uniform<Vector2S>;
            Assert.AreEqual("exampleVec2", exampleVec2.Name);
            Assert.AreEqual(UniformType.FloatVector2, exampleVec2.DataType);
            Assert.AreEqual(new Vector2S(), exampleVec2.Value);
            exampleVec2.Value = new Vector2S(1, 0.5f);
            Assert.AreEqual(new Vector2S(1, 0.5f), exampleVec2.Value);

            Uniform<Vector3S> exampleVec3 = sp.Uniforms["exampleVec3"] as Uniform<Vector3S>;
            Assert.AreEqual("exampleVec3", exampleVec3.Name);
            Assert.AreEqual(UniformType.FloatVector3, exampleVec3.DataType);
            Assert.AreEqual(new Vector3S(), exampleVec3.Value);
            exampleVec3.Value = new Vector3S(1, 0, 0);
            Assert.AreEqual(new Vector3S(1, 0, 0), exampleVec3.Value);

            Uniform<Vector4S> exampleVec4 = sp.Uniforms["exampleVec4"] as Uniform<Vector4S>;
            Assert.AreEqual("exampleVec4", exampleVec4.Name);
            Assert.AreEqual(UniformType.FloatVector4, exampleVec4.DataType);
            Assert.AreEqual(new Vector4S(), exampleVec4.Value);
            exampleVec4.Value = new Vector4S(1, 0, 0, 0);
            Assert.AreEqual(new Vector4S(1, 0, 0, 0), exampleVec4.Value);

            ///////////////////////////////////////////////////////////////////

            Uniform<int> exampleInt = sp.Uniforms["exampleInt"] as Uniform<int>;
            Assert.AreEqual("exampleInt", exampleInt.Name);
            Assert.AreEqual(UniformType.Int, exampleInt.DataType);
            Assert.AreEqual(0, exampleInt.Value);
            exampleInt.Value = 1;
            Assert.AreEqual(1, exampleInt.Value);

            Uniform<Vector2i> exampleIVec2 = sp.Uniforms["exampleIVec2"] as Uniform<Vector2i>;
            Assert.AreEqual("exampleIVec2", exampleIVec2.Name);
            Assert.AreEqual(UniformType.IntVector2, exampleIVec2.DataType);
            Assert.AreEqual(new Vector2i(), exampleIVec2.Value);
            exampleIVec2.Value = new Vector2i(1, 0);
            Assert.AreEqual(new Vector2i(1, 0), exampleIVec2.Value);

            Uniform<Vector3i> exampleIVec3 = sp.Uniforms["exampleIVec3"] as Uniform<Vector3i>;
            Assert.AreEqual("exampleIVec3", exampleIVec3.Name);
            Assert.AreEqual(UniformType.IntVector3, exampleIVec3.DataType);
            Assert.AreEqual(new Vector3i(), exampleIVec3.Value);
            exampleIVec3.Value = new Vector3i(1, 0, 0);
            Assert.AreEqual(new Vector3i(1, 0, 0), exampleIVec3.Value);

            Uniform<Vector4i> exampleIVec4 = sp.Uniforms["exampleIVec4"] as Uniform<Vector4i>;
            Assert.AreEqual("exampleIVec4", exampleIVec4.Name);
            Assert.AreEqual(UniformType.IntVector4, exampleIVec4.DataType);
            Assert.AreEqual(new Vector4i(), exampleIVec4.Value);
            exampleIVec4.Value = new Vector4i(1, 0, 0, 0);
            Assert.AreEqual(new Vector4i(1, 0, 0, 0), exampleIVec4.Value);

            ///////////////////////////////////////////////////////////////////

            Uniform<bool> exampleBool = sp.Uniforms["exampleBool"] as Uniform<bool>;
            Assert.AreEqual("exampleBool", exampleBool.Name);
            Assert.AreEqual(UniformType.Bool, exampleBool.DataType);
            Assert.AreEqual(false, exampleBool.Value);
            exampleBool.Value = true;
            Assert.AreEqual(true, exampleBool.Value);

            Uniform<Vector2b> exampleBVec2 = sp.Uniforms["exampleBVec2"] as Uniform<Vector2b>;
            Assert.AreEqual("exampleBVec2", exampleBVec2.Name);
            Assert.AreEqual(UniformType.BoolVector2, exampleBVec2.DataType);
            Assert.AreEqual(new Vector2b(), exampleBVec2.Value);
            exampleBVec2.Value = new Vector2b(true, false);
            Assert.AreEqual(new Vector2b(true, false), exampleBVec2.Value);

            Uniform<Vector3b> exampleBVec3 = sp.Uniforms["exampleBVec3"] as Uniform<Vector3b>;
            Assert.AreEqual("exampleBVec3", exampleBVec3.Name);
            Assert.AreEqual(UniformType.BoolVector3, exampleBVec3.DataType);
            Assert.AreEqual(new Vector3b(), exampleBVec3.Value);
            exampleBVec3.Value = new Vector3b(true, false, false);
            Assert.AreEqual(new Vector3b(true, false, false), exampleBVec3.Value);

            Uniform<Vector4b> exampleBVec4 = sp.Uniforms["exampleBVec4"] as Uniform<Vector4b>;
            Assert.AreEqual("exampleBVec4", exampleBVec4.Name);
            Assert.AreEqual(UniformType.BoolVector4, exampleBVec4.DataType);
            Assert.AreEqual(new Vector4b(), exampleBVec4.Value);
            exampleBVec4.Value = new Vector4b(true, false, false, false);
            Assert.AreEqual(new Vector4b(true, false, false, false), exampleBVec4.Value);

            ///////////////////////////////////////////////////////////////////

            Uniform<int> exampleSampler2D = sp.Uniforms["exampleSampler2D"] as Uniform<int>;
            Assert.AreEqual("exampleSampler2D", exampleSampler2D.Name);
            Assert.AreEqual(UniformType.Sampler2D, exampleSampler2D.DataType);
            Assert.AreEqual(0, exampleSampler2D.Value);
            exampleSampler2D.Value = 1;
            Assert.AreEqual(1, exampleSampler2D.Value);

            Uniform<int> exampleSampler1DArray = sp.Uniforms["exampleSampler1DArray"] as Uniform<int>;
            Assert.AreEqual("exampleSampler1DArray", exampleSampler1DArray.Name);
            Assert.AreEqual(UniformType.Sampler1DArray, exampleSampler1DArray.DataType);
            Assert.AreEqual(0, exampleSampler1DArray.Value);
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
            Assert.AreEqual(new Matrix4(), exampleMat4.Value);
            exampleMat4.Value = m4;
            Assert.AreEqual(m4, exampleMat4.Value);

            Matrix3 m3 = new Matrix3(
                new Vector3(0, 0, 0),
                new Vector3(0, 0, 0.25f),
                new Vector3(0, 0, 0));
            Uniform<Matrix3> exampleMat3 = sp.Uniforms["exampleMat3"] as Uniform<Matrix3>;
            Assert.AreEqual("exampleMat3", exampleMat3.Name);
            Assert.AreEqual(UniformType.FloatMatrix33, exampleMat3.DataType);
            Assert.AreEqual(new Matrix3(), exampleMat3.Value);
            exampleMat3.Value = m3;
            Assert.AreEqual(m3, exampleMat3.Value);

            Matrix2 m2 = new Matrix2(
                new Vector2(0, 1),
                new Vector2(0, 0));
            Uniform<Matrix2> exampleMat2 = sp.Uniforms["exampleMat2"] as Uniform<Matrix2>;
            Assert.AreEqual("exampleMat2", exampleMat2.Name);
            Assert.AreEqual(UniformType.FloatMatrix22, exampleMat2.DataType);
            Assert.AreEqual(new Matrix2(), exampleMat2.Value);
            exampleMat2.Value = m2;
            Assert.AreEqual(m2, exampleMat2.Value);

            Matrix23 m23 = new Matrix23(
                new Vector2(0, 0),
                new Vector2(0, 0.25f),
                new Vector2(0, 0));
            Uniform<Matrix23> exampleMat23 = sp.Uniforms["exampleMat23"] as Uniform<Matrix23>;
            Assert.AreEqual("exampleMat23", exampleMat23.Name);
            Assert.AreEqual(UniformType.FloatMatrix23, exampleMat23.DataType);
            Assert.AreEqual(new Matrix23(), exampleMat23.Value);
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
            Assert.AreEqual(new Matrix24(), exampleMat24.Value);
            exampleMat24.Value = m24;
            Assert.AreEqual(m24, exampleMat24.Value);

            Matrix32 m32 = new Matrix32(
                new Vector3(0, 0, 0),
                new Vector3(0, 0.25f, 0));
            Uniform<Matrix32> exampleMat32 = sp.Uniforms["exampleMat32"] as Uniform<Matrix32>;
            Assert.AreEqual("exampleMat32", exampleMat32.Name);
            Assert.AreEqual(UniformType.FloatMatrix32, exampleMat32.DataType);
            Assert.AreEqual(new Matrix32(), exampleMat32.Value);
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
            Assert.AreEqual(new Matrix34(), exampleMat34.Value);
            exampleMat34.Value = m34;
            Assert.AreEqual(m34, exampleMat34.Value);

            Matrix42 m42 = new Matrix42(
                new Vector4(0, 0, 0, 0.25f),
                new Vector4(0, 0, 0, 0));
            Uniform<Matrix42> exampleMat42 = sp.Uniforms["exampleMat42"] as Uniform<Matrix42>;
            Assert.AreEqual("exampleMat42", exampleMat42.Name);
            Assert.AreEqual(UniformType.FloatMatrix42, exampleMat42.DataType);
            Assert.AreEqual(new Matrix42(), exampleMat42.Value);
            exampleMat42.Value = m42;
            Assert.AreEqual(m42, exampleMat42.Value);

            Matrix43 m43 = new Matrix43(
                new Vector4(0, 0, 0, 0),
                new Vector4(0, 0, 0, 0.25f),
                new Vector4(0, 0, 0, 0));
            Uniform<Matrix43> exampleMat43 = sp.Uniforms["exampleMat43"] as Uniform<Matrix43>;
            Assert.AreEqual("exampleMat43", exampleMat43.Name);
            Assert.AreEqual(UniformType.FloatMatrix43, exampleMat43.DataType);
            Assert.AreEqual(new Matrix43(), exampleMat43.Value);
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
                 
                  uniform sampler2D mg_texture0;
                  uniform sampler2D mg_texture1;
                  uniform sampler2D mg_texture2;
                  uniform sampler2D mg_texture3;
                  out vec4 FragColor;

                  void main()
                  {
                      FragColor = 
                          texture(mg_texture0, vec2(0, 0)) + 
                          texture(mg_texture1, vec2(0, 0)) + 
                          texture(mg_texture2, vec2(0, 0)) + 
                          texture(mg_texture3, vec2(0, 0));
                  }";
            ShaderProgram sp = Device.CreateShaderProgram(ShaderSources.PassThroughVertexShader(), fs);
            Assert.AreEqual(0, (sp.Uniforms["mg_texture0"] as Uniform<int>).Value);
            Assert.AreEqual(1, (sp.Uniforms["mg_texture1"] as Uniform<int>).Value);
            Assert.AreEqual(2, (sp.Uniforms["mg_texture2"] as Uniform<int>).Value);
            Assert.AreEqual(3, (sp.Uniforms["mg_texture3"] as Uniform<int>).Value);

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
