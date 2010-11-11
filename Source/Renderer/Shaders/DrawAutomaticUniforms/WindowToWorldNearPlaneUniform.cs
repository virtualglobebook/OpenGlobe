#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using OpenGlobe.Core;

namespace OpenGlobe.Renderer
{
    internal class WindowToWorldNearPlaneUniform : DrawAutomaticUniform
    {
        public WindowToWorldNearPlaneUniform(Uniform uniform)
        {
            _uniform = (Uniform<Matrix4F>)uniform;
        }

        #region DrawAutomaticUniform Members

        public override void Set(Context context, DrawState drawState, SceneState sceneState)
        {
            Camera camera = sceneState.Camera;
            double theta = camera.FieldOfViewX * 0.5;
            double phi = camera.FieldOfViewY * 0.5;
            double nearDistance = camera.PerspectiveNearPlaneDistance;

            //
            // Coordinate system for the near plane:  origin is at center, x and y
            // span [-1, 1] just like noramlized device coordinates.
            //
            Vector3D origin = camera.Eye + (nearDistance * camera.Forward);    // Project eye onto near plane
            Vector3D xAxis = camera.Right * (nearDistance * Math.Tan(theta));  // Rescale right to near plane
            Vector3D yAxis = camera.Up * (nearDistance * Math.Tan(phi));       // Rescale up to near plane

            _uniform.Value = new Matrix4D(
                    xAxis.X, yAxis.X, 0.0, origin.X,
                    xAxis.Y, yAxis.Y, 0.0, origin.Y,
                    xAxis.Z, yAxis.Z, 0.0, origin.Z,
                    0.0,     0.0,     0.0, 1.0).ToMatrix4F();
        }

        #endregion

        private Uniform<Matrix4F> _uniform;
    }
}
