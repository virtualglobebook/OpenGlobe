#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using OpenTK;

namespace OpenGlobe.Renderer
{
    internal class ViewportTransformationMatrixUniform : DrawAutomaticUniform
    {
        public ViewportTransformationMatrixUniform(Uniform uniform)
        {
            _uniform = uniform as Uniform<Matrix4>;
        }

        #region DrawAutomaticUniform Members

        public override void Set(Context context, DrawState drawState, SceneState sceneState)
        {
            _uniform.Value = Conversion.ToMatrix4(sceneState.ComputeViewportTransformationMatrix(context.Viewport,
                drawState.RenderState.DepthRange.Near, drawState.RenderState.DepthRange.Far));
        }

        #endregion

        private Uniform<Matrix4> _uniform;
    }
}
