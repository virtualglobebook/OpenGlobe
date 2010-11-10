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
using OpenGlobe.Core;

namespace OpenGlobe.Renderer
{
    [TestFixture]
    public class ShaderProgramTests
    {
        [Test]
        public void PassThrough()
        {
            using (GraphicsWindow window = Device.CreateWindow(1, 1))
            using (ShaderProgram sp = Device.CreateShaderProgram(
                ShaderSources.PassThroughVertexShader(),
                ShaderSources.PassThroughFragmentShader()))
            {
                Assert.IsFalse(sp.Log.Contains("warning"));
                Assert.IsEmpty(sp.Uniforms);
                Assert.IsEmpty(sp.UniformBlocks);

                Assert.AreEqual(1, sp.VertexAttributes.Count);
                Assert.IsTrue(sp.VertexAttributes.Contains("position"));

                ShaderVertexAttribute attribute = sp.VertexAttributes["position"];
                Assert.AreEqual("position", attribute.Name);
                Assert.AreEqual(ShaderVertexAttributeType.FloatVector4, attribute.Datatype);
                Assert.AreEqual(1, attribute.Length);
            }
        }

        [Test]
        public void PassThroughWithGeometryShader()
        {
            using (GraphicsWindow window = Device.CreateWindow(1, 1))
            using (ShaderProgram sp = Device.CreateShaderProgram(
                ShaderSources.PassThroughVertexShader(),
                ShaderSources.PassThroughGeometryShader(),
                ShaderSources.PassThroughFragmentShader()))
            {
                Assert.IsFalse(sp.Log.Contains("warning"));
                Assert.IsEmpty(sp.Uniforms);
                Assert.IsEmpty(sp.UniformBlocks);

                Assert.AreEqual(1, sp.VertexAttributes.Count);
                Assert.IsTrue(sp.VertexAttributes.Contains("position"));

                ShaderVertexAttribute attribute = sp.VertexAttributes["position"];
                Assert.AreEqual("position", attribute.Name);
                Assert.AreEqual(ShaderVertexAttributeType.FloatVector4, attribute.Datatype);
                Assert.AreEqual(1, attribute.Length);
            }
        }

        [Test]
        public void DefaultUniformBlock()
        {
            //
            // Uniform initializers are not supported because there may be some driver bugs:
            //
            //    http://www.opengl.org/discussion_boards/ubbthreads.php?ubb=showflat&Main=48817&Number=250791#Post250791
            //    http://www.opengl.org/discussion_boards/ubbthreads.php?ubb=showflat&Main=51624&Number=266719#Post266719
            //    http://www.opengl.org/discussion_boards/ubbthreads.php?ubb=showflat&Main=52018&Number=268866#Post268866
            //
            string fs =
                @"#version 330
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
                  uniform sampler2DRect exampleSampler2DRect;
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
                      red += texture(exampleSampler2DRect, vec2(0, 0)).r;
                      red += texture1DArray(exampleSampler1DArray, vec2(0, 0)).r;

                      FragColor = vec4(red, 0, 0, 1);
                  }";

            using (GraphicsWindow window = Device.CreateWindow(1, 1))
            using (ShaderProgram sp = Device.CreateShaderProgram(ShaderSources.PassThroughVertexShader(), fs))
            {
                Assert.IsFalse(sp.Log.Contains("warning"));
                Assert.IsEmpty(sp.UniformBlocks);
                Assert.AreEqual(24, sp.Uniforms.Count);

                Uniform<float> exampleFloat = (Uniform<float>)sp.Uniforms["exampleFloat"];
                Assert.AreEqual("exampleFloat", exampleFloat.Name);
                Assert.AreEqual(UniformType.Float, exampleFloat.Datatype);
                Assert.AreEqual(0, exampleFloat.Value);
                exampleFloat.Value = 0.75f;
                Assert.AreEqual(0.75f, exampleFloat.Value);

                Uniform<Vector2S> exampleVec2 = (Uniform<Vector2S>)sp.Uniforms["exampleVec2"];
                Assert.AreEqual("exampleVec2", exampleVec2.Name);
                Assert.AreEqual(UniformType.FloatVector2, exampleVec2.Datatype);
                Assert.AreEqual(new Vector2S(), exampleVec2.Value);
                exampleVec2.Value = new Vector2S(1, 0.5f);
                Assert.AreEqual(new Vector2S(1, 0.5f), exampleVec2.Value);

                Uniform<Vector3S> exampleVec3 = (Uniform<Vector3S>)sp.Uniforms["exampleVec3"];
                Assert.AreEqual("exampleVec3", exampleVec3.Name);
                Assert.AreEqual(UniformType.FloatVector3, exampleVec3.Datatype);
                Assert.AreEqual(new Vector3S(), exampleVec3.Value);
                exampleVec3.Value = new Vector3S(1, 0, 0);
                Assert.AreEqual(new Vector3S(1, 0, 0), exampleVec3.Value);

                Uniform<Vector4S> exampleVec4 = (Uniform<Vector4S>)sp.Uniforms["exampleVec4"];
                Assert.AreEqual("exampleVec4", exampleVec4.Name);
                Assert.AreEqual(UniformType.FloatVector4, exampleVec4.Datatype);
                Assert.AreEqual(new Vector4S(), exampleVec4.Value);
                exampleVec4.Value = new Vector4S(1, 0, 0, 0);
                Assert.AreEqual(new Vector4S(1, 0, 0, 0), exampleVec4.Value);

                ///////////////////////////////////////////////////////////////

                Uniform<int> exampleInt = (Uniform<int>)sp.Uniforms["exampleInt"];
                Assert.AreEqual("exampleInt", exampleInt.Name);
                Assert.AreEqual(UniformType.Int, exampleInt.Datatype);
                Assert.AreEqual(0, exampleInt.Value);
                exampleInt.Value = 1;
                Assert.AreEqual(1, exampleInt.Value);

                Uniform<Vector2I> exampleIVec2 = (Uniform<Vector2I>)sp.Uniforms["exampleIVec2"];
                Assert.AreEqual("exampleIVec2", exampleIVec2.Name);
                Assert.AreEqual(UniformType.IntVector2, exampleIVec2.Datatype);
                Assert.AreEqual(new Vector2I(), exampleIVec2.Value);
                exampleIVec2.Value = new Vector2I(1, 0);
                Assert.AreEqual(new Vector2I(1, 0), exampleIVec2.Value);

                Uniform<Vector3I> exampleIVec3 = (Uniform<Vector3I>)sp.Uniforms["exampleIVec3"];
                Assert.AreEqual("exampleIVec3", exampleIVec3.Name);
                Assert.AreEqual(UniformType.IntVector3, exampleIVec3.Datatype);
                Assert.AreEqual(new Vector3I(), exampleIVec3.Value);
                exampleIVec3.Value = new Vector3I(1, 0, 0);
                Assert.AreEqual(new Vector3I(1, 0, 0), exampleIVec3.Value);

                Uniform<Vector4I> exampleIVec4 = (Uniform<Vector4I>)sp.Uniforms["exampleIVec4"];
                Assert.AreEqual("exampleIVec4", exampleIVec4.Name);
                Assert.AreEqual(UniformType.IntVector4, exampleIVec4.Datatype);
                Assert.AreEqual(new Vector4I(), exampleIVec4.Value);
                exampleIVec4.Value = new Vector4I(1, 0, 0, 0);
                Assert.AreEqual(new Vector4I(1, 0, 0, 0), exampleIVec4.Value);

                ///////////////////////////////////////////////////////////////

                Uniform<bool> exampleBool = (Uniform<bool>)sp.Uniforms["exampleBool"];
                Assert.AreEqual("exampleBool", exampleBool.Name);
                Assert.AreEqual(UniformType.Bool, exampleBool.Datatype);
                Assert.AreEqual(false, exampleBool.Value);
                exampleBool.Value = true;
                Assert.AreEqual(true, exampleBool.Value);

                Uniform<Vector2B> exampleBVec2 = (Uniform<Vector2B>)sp.Uniforms["exampleBVec2"];
                Assert.AreEqual("exampleBVec2", exampleBVec2.Name);
                Assert.AreEqual(UniformType.BoolVector2, exampleBVec2.Datatype);
                Assert.AreEqual(new Vector2B(), exampleBVec2.Value);
                exampleBVec2.Value = new Vector2B(true, false);
                Assert.AreEqual(new Vector2B(true, false), exampleBVec2.Value);

                Uniform<Vector3B> exampleBVec3 = (Uniform<Vector3B>)sp.Uniforms["exampleBVec3"];
                Assert.AreEqual("exampleBVec3", exampleBVec3.Name);
                Assert.AreEqual(UniformType.BoolVector3, exampleBVec3.Datatype);
                Assert.AreEqual(new Vector3B(), exampleBVec3.Value);
                exampleBVec3.Value = new Vector3B(true, false, false);
                Assert.AreEqual(new Vector3B(true, false, false), exampleBVec3.Value);

                Uniform<Vector4B> exampleBVec4 = (Uniform<Vector4B>)sp.Uniforms["exampleBVec4"];
                Assert.AreEqual("exampleBVec4", exampleBVec4.Name);
                Assert.AreEqual(UniformType.BoolVector4, exampleBVec4.Datatype);
                Assert.AreEqual(new Vector4B(), exampleBVec4.Value);
                exampleBVec4.Value = new Vector4B(true, false, false, false);
                Assert.AreEqual(new Vector4B(true, false, false, false), exampleBVec4.Value);

                ///////////////////////////////////////////////////////////////

                Uniform<int> exampleSampler2D = (Uniform<int>)sp.Uniforms["exampleSampler2D"];
                Assert.AreEqual("exampleSampler2D", exampleSampler2D.Name);
                Assert.AreEqual(UniformType.Sampler2D, exampleSampler2D.Datatype);
                Assert.AreEqual(0, exampleSampler2D.Value);
                exampleSampler2D.Value = 1;
                Assert.AreEqual(1, exampleSampler2D.Value);

                Uniform<int> exampleSampler2DRect = (Uniform<int>)sp.Uniforms["exampleSampler2DRect"];
                Assert.AreEqual("exampleSampler2DRect", exampleSampler2DRect.Name);
                Assert.AreEqual(UniformType.Sampler2DRectangle, exampleSampler2DRect.Datatype);
                Assert.AreEqual(0, exampleSampler2DRect.Value);
                exampleSampler2DRect.Value = 1;
                Assert.AreEqual(1, exampleSampler2DRect.Value);

                Uniform<int> exampleSampler1DArray = (Uniform<int>)sp.Uniforms["exampleSampler1DArray"];
                Assert.AreEqual("exampleSampler1DArray", exampleSampler1DArray.Name);
                Assert.AreEqual(UniformType.Sampler1DArray, exampleSampler1DArray.Datatype);
                Assert.AreEqual(0, exampleSampler1DArray.Value);
                exampleSampler1DArray.Value = 1;
                Assert.AreEqual(1, exampleSampler1DArray.Value);

                ///////////////////////////////////////////////////////////////

                Matrix4 m4 = new Matrix4(
                    new Vector4(0, 0, 0, 0),
                    new Vector4(0.25f, 0, 0, 0),
                    new Vector4(0, 0, 0, 0),
                    new Vector4(0, 0, 0, 0));
                Uniform<Matrix4> exampleMat4 = (Uniform<Matrix4>)sp.Uniforms["exampleMat4"];
                Assert.AreEqual("exampleMat4", exampleMat4.Name);
                Assert.AreEqual(UniformType.FloatMatrix44, exampleMat4.Datatype);
                Assert.AreEqual(new Matrix4(), exampleMat4.Value);
                exampleMat4.Value = m4;
                Assert.AreEqual(m4, exampleMat4.Value);

                Matrix3 m3 = new Matrix3(
                    new Vector3(0, 0, 0),
                    new Vector3(0, 0, 0.25f),
                    new Vector3(0, 0, 0));
                Uniform<Matrix3> exampleMat3 = (Uniform<Matrix3>)sp.Uniforms["exampleMat3"];
                Assert.AreEqual("exampleMat3", exampleMat3.Name);
                Assert.AreEqual(UniformType.FloatMatrix33, exampleMat3.Datatype);
                Assert.AreEqual(new Matrix3(), exampleMat3.Value);
                exampleMat3.Value = m3;
                Assert.AreEqual(m3, exampleMat3.Value);

                Matrix2 m2 = new Matrix2(
                    new Vector2(0, 1),
                    new Vector2(0, 0));
                Uniform<Matrix2> exampleMat2 = (Uniform<Matrix2>)sp.Uniforms["exampleMat2"];
                Assert.AreEqual("exampleMat2", exampleMat2.Name);
                Assert.AreEqual(UniformType.FloatMatrix22, exampleMat2.Datatype);
                Assert.AreEqual(new Matrix2(), exampleMat2.Value);
                exampleMat2.Value = m2;
                Assert.AreEqual(m2, exampleMat2.Value);

                Matrix23 m23 = new Matrix23(
                    new Vector2(0, 0),
                    new Vector2(0, 0.25f),
                    new Vector2(0, 0));
                Uniform<Matrix23> exampleMat23 = (Uniform<Matrix23>)sp.Uniforms["exampleMat23"];
                Assert.AreEqual("exampleMat23", exampleMat23.Name);
                Assert.AreEqual(UniformType.FloatMatrix23, exampleMat23.Datatype);
                Assert.AreEqual(new Matrix23(), exampleMat23.Value);
                exampleMat23.Value = m23;
                Assert.AreEqual(m23, exampleMat23.Value);

                Matrix24 m24 = new Matrix24(
                    new Vector2(0, 0),
                    new Vector2(0, 0),
                    new Vector2(0.25f, 0),
                    new Vector2(0, 0));
                Uniform<Matrix24> exampleMat24 = (Uniform<Matrix24>)sp.Uniforms["exampleMat24"];
                Assert.AreEqual("exampleMat24", exampleMat24.Name);
                Assert.AreEqual(UniformType.FloatMatrix24, exampleMat24.Datatype);
                Assert.AreEqual(new Matrix24(), exampleMat24.Value);
                exampleMat24.Value = m24;
                Assert.AreEqual(m24, exampleMat24.Value);

                Matrix32 m32 = new Matrix32(
                    new Vector3(0, 0, 0),
                    new Vector3(0, 0.25f, 0));
                Uniform<Matrix32> exampleMat32 = (Uniform<Matrix32>)sp.Uniforms["exampleMat32"];
                Assert.AreEqual("exampleMat32", exampleMat32.Name);
                Assert.AreEqual(UniformType.FloatMatrix32, exampleMat32.Datatype);
                Assert.AreEqual(new Matrix32(), exampleMat32.Value);
                exampleMat32.Value = m32;
                Assert.AreEqual(m32, exampleMat32.Value);

                Matrix34 m34 = new Matrix34(
                    new Vector3(0, 0, 0),
                    new Vector3(0, 0, 0),
                    new Vector3(0.25f, 0, 0),
                    new Vector3(0, 0, 0));
                Uniform<Matrix34> exampleMat34 = (Uniform<Matrix34>)sp.Uniforms["exampleMat34"];
                Assert.AreEqual("exampleMat34", exampleMat34.Name);
                Assert.AreEqual(UniformType.FloatMatrix34, exampleMat34.Datatype);
                Assert.AreEqual(new Matrix34(), exampleMat34.Value);
                exampleMat34.Value = m34;
                Assert.AreEqual(m34, exampleMat34.Value);

                Matrix42 m42 = new Matrix42(
                    new Vector4(0, 0, 0, 0.25f),
                    new Vector4(0, 0, 0, 0));
                Uniform<Matrix42> exampleMat42 = (Uniform<Matrix42>)sp.Uniforms["exampleMat42"];
                Assert.AreEqual("exampleMat42", exampleMat42.Name);
                Assert.AreEqual(UniformType.FloatMatrix42, exampleMat42.Datatype);
                Assert.AreEqual(new Matrix42(), exampleMat42.Value);
                exampleMat42.Value = m42;
                Assert.AreEqual(m42, exampleMat42.Value);

                Matrix43 m43 = new Matrix43(
                    new Vector4(0, 0, 0, 0),
                    new Vector4(0, 0, 0, 0.25f),
                    new Vector4(0, 0, 0, 0));
                Uniform<Matrix43> exampleMat43 = (Uniform<Matrix43>)sp.Uniforms["exampleMat43"];
                Assert.AreEqual("exampleMat43", exampleMat43.Name);
                Assert.AreEqual(UniformType.FloatMatrix43, exampleMat43.Datatype);
                Assert.AreEqual(new Matrix43(), exampleMat43.Value);
                exampleMat43.Value = m43;
                Assert.AreEqual(m43, exampleMat43.Value);
            }
        }

        [Test]
        public void LinkAutomaticUniforms()
        {
            string fs =
                @"#version 330
                 
                  uniform sampler2D og_texture0;
                  uniform sampler2D og_texture1;
                  uniform sampler2D og_texture2;
                  uniform sampler2D og_texture3;
                  out vec4 FragColor;

                  void main()
                  {
                      FragColor = 
                          texture(og_texture0, vec2(0, 0)) + 
                          texture(og_texture1, vec2(0, 0)) + 
                          texture(og_texture2, vec2(0, 0)) + 
                          texture(og_texture3, vec2(0, 0));
                  }";

            using (GraphicsWindow window = Device.CreateWindow(1, 1))
            using (ShaderProgram sp = Device.CreateShaderProgram(ShaderSources.PassThroughVertexShader(), fs))
            {
                Assert.AreEqual(0, ((Uniform<int>)sp.Uniforms["og_texture0"]).Value);
                Assert.AreEqual(1, ((Uniform<int>)sp.Uniforms["og_texture1"]).Value);
                Assert.AreEqual(2, ((Uniform<int>)sp.Uniforms["og_texture2"]).Value);
                Assert.AreEqual(3, ((Uniform<int>)sp.Uniforms["og_texture3"]).Value);
            }
        }

        [Test]
        public void UniformBlock()
        {
            using (GraphicsWindow window = Device.CreateWindow(1, 1))
            using (ShaderProgram sp = Device.CreateShaderProgram(
                ShaderSources.PassThroughVertexShader(),
                ShaderSources.RedUniformBlockFragmentShader()))
            {
                Assert.IsFalse(sp.Log.Contains("warning"));
                Assert.IsEmpty(sp.Uniforms);
                Assert.AreEqual(1, sp.UniformBlocks.Count);

                UniformBlock redBlock = sp.UniformBlocks["RedBlock"];
                Assert.AreEqual("RedBlock", redBlock.Name);
                Assert.GreaterOrEqual(redBlock.SizeInBytes, 4);
                Assert.AreEqual(1, redBlock.Members.Count);

                UniformBlockMember red = redBlock.Members["red"];
                Assert.AreEqual("red", red.Name);
                Assert.AreEqual(0, red.OffsetInBytes);
                Assert.AreEqual(UniformType.Float, red.Datatype);
            }
        }
    }
}
