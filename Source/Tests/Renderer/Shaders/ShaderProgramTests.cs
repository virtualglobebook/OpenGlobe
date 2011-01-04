#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using NUnit.Framework;
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
                Assert.AreEqual(15, sp.Uniforms.Count);

                Uniform<float> exampleFloat = (Uniform<float>)sp.Uniforms["exampleFloat"];
                Assert.AreEqual("exampleFloat", exampleFloat.Name);
                Assert.AreEqual(UniformType.Float, exampleFloat.Datatype);
                Assert.AreEqual(0, exampleFloat.Value);
                exampleFloat.Value = 0.75f;
                Assert.AreEqual(0.75f, exampleFloat.Value);

                Uniform<Vector2F> exampleVec2 = (Uniform<Vector2F>)sp.Uniforms["exampleVec2"];
                Assert.AreEqual("exampleVec2", exampleVec2.Name);
                Assert.AreEqual(UniformType.FloatVector2, exampleVec2.Datatype);
                Assert.AreEqual(new Vector2F(), exampleVec2.Value);
                exampleVec2.Value = new Vector2F(1, 0.5f);
                Assert.AreEqual(new Vector2F(1, 0.5f), exampleVec2.Value);

                Uniform<Vector3F> exampleVec3 = (Uniform<Vector3F>)sp.Uniforms["exampleVec3"];
                Assert.AreEqual("exampleVec3", exampleVec3.Name);
                Assert.AreEqual(UniformType.FloatVector3, exampleVec3.Datatype);
                Assert.AreEqual(new Vector3F(), exampleVec3.Value);
                exampleVec3.Value = new Vector3F(1, 0, 0);
                Assert.AreEqual(new Vector3F(1, 0, 0), exampleVec3.Value);

                Uniform<Vector4F> exampleVec4 = (Uniform<Vector4F>)sp.Uniforms["exampleVec4"];
                Assert.AreEqual("exampleVec4", exampleVec4.Name);
                Assert.AreEqual(UniformType.FloatVector4, exampleVec4.Datatype);
                Assert.AreEqual(new Vector4F(), exampleVec4.Value);
                exampleVec4.Value = new Vector4F(1, 0, 0, 0);
                Assert.AreEqual(new Vector4F(1, 0, 0, 0), exampleVec4.Value);

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

        [Test]
        public void TransformFeedback()
        {
            string vs =
                @"#version 330

                  in vec4 position;
                  out vec3 fsColor;
                  out float fsDepthScale;

                  void main()                     
                  {
                      gl_Position = position;
                      fsColor = position.xyz;
                      fsDepthScale = position.w;
                  }";
            string fs =
                @"#version 330
                 
                  out vec3 fragmentColor;
                  in vec3 fsColor;
                  in float fsDepthScale;

                  void main()
                  {
                      fragmentColor = fsColor;
                      gl_FragDepth *= fsDepthScale;
                  }";

            using (GraphicsWindow window = Device.CreateWindow(1, 1))
            using (ShaderProgram sp = Device.CreateShaderProgram(vs, fs,
                new string[] { "gl_Position", "fsDepthScale" },
                TransformFeedbackAttributeLayout.Interleaved))
            {
                Assert.AreEqual(2, sp.TransformFeedbackOutputs.Count);
                Assert.AreEqual(TransformFeedbackAttributeLayout.Interleaved, sp.TransformFeedbackAttributeLayout);

                TransformFeedbackOutput positionOutput = sp.TransformFeedbackOutputs["gl_Position"];
                Assert.AreEqual("gl_Position", positionOutput.Name);
                Assert.AreEqual(ShaderVertexAttributeType.FloatVector4, positionOutput.Datatype);
                Assert.AreEqual(4, positionOutput.NumberOfComponents);

                TransformFeedbackOutput depthScaleOutput = sp.TransformFeedbackOutputs["fsDepthScale"];
                Assert.AreEqual("fsDepthScale", depthScaleOutput.Name);
                Assert.AreEqual(ShaderVertexAttributeType.Float, depthScaleOutput.Datatype);
                Assert.AreEqual(1, depthScaleOutput.NumberOfComponents);
            }
        }
    }
}
