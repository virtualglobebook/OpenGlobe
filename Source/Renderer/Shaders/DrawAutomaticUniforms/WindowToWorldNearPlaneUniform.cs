#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using OpenTK;
using MiniGlobe.Core;

namespace MiniGlobe.Renderer
{
    internal class WindowToWorldNearPlaneUniform : DrawAutomaticUniform
    {
        public WindowToWorldNearPlaneUniform(Uniform uniform)
        {
            _uniform = uniform as Uniform<Matrix4>;
        }

        #region DrawAutomaticUniform Members

        public override void Set(Context context, SceneState sceneState)
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

            _uniform.Value = Conversion.ToMatrix4(new Matrix4d(
                    xAxis.X, xAxis.Y, xAxis.Z, 0,
                    yAxis.X, yAxis.Y, yAxis.Z, 0,
                    0, 0, 0, 0,
                    origin.X, origin.Y, origin.Z, 1));
        }

        #endregion

        private Uniform<Matrix4> _uniform;
    }
}
