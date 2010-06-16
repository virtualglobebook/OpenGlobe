#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System.Collections.Generic;
using OpenGlobe.Core;

namespace OpenGlobe.Renderer
{
    public abstract class ShaderProgram : Disposable
    {
        public abstract string Log { get; }
        public abstract FragmentOutputs FragmentOutputs  { get; }
        public abstract ShaderVertexAttributeCollection VertexAttributes { get; }
        public abstract UniformCollection Uniforms { get; }
        public abstract UniformBlockCollection UniformBlocks { get; }

        protected void InitializeAutomaticUniforms(UniformCollection uniforms)
        {
            foreach (Uniform uniform in uniforms)
            {
                if (Device.LinkAutomaticUniforms.Contains(uniform.Name))
                {
                    Device.LinkAutomaticUniforms[uniform.Name].Set(uniform);
                }
                else if (Device.DrawAutomaticUniformFactories.Contains(uniform.Name))
                {
                    _drawAutomaticUniforms.Add(Device.DrawAutomaticUniformFactories[uniform.Name].Create(uniform));
                }
            }
        }

        protected void SetDrawAutomaticUniforms(Context context, SceneState sceneState)
        {
            for (int i = 0; i < _drawAutomaticUniforms.Count; ++i)
            {
                _drawAutomaticUniforms[i].Set(context, sceneState);
            }
        }

        private List<DrawAutomaticUniform> _drawAutomaticUniforms = new List<DrawAutomaticUniform>();
    }
}
