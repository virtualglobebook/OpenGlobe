#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using OpenTK;
using System.Drawing;

namespace MiniGlobe.Renderer
{
    public class SceneState
    {
        public SceneState()
	    {
            DiffuseIntensity = 0.65f;
            SpecularIntensity = 0.25f;
            AmbientIntensity = 0.10f;
            Shininess = 12;
            Camera = new Camera();
            SunPosition = new Vector3d(200000, 0, 0);
            ModelMatrix = Matrix4d.Identity;
        }

        public float DiffuseIntensity { get; set; }
        public float SpecularIntensity { get; set; }
        public float AmbientIntensity { get; set; }
        public float Shininess { get; set; }
        public Camera Camera { get; set; }

        public Vector3d SunPosition { get; set; }

        public Vector3d CameraLightPosition
        {
            get { return Camera.Eye + (3.0 * Camera.Up); }
        }

        public Matrix4d ComputeViewportTransformationMatrix(Rectangle viewport)
        {
            double halfWidth = viewport.Width * 0.5;
            double halfHeight = viewport.Height * 0.5;
            double halfDepth = Camera.OrthographicDepth * 0.5;

            //
            // Bottom and top swapped:  MS -> OpenGL
            //
            return new Matrix4d(
                halfWidth, 0, 0, 0,
                0, halfHeight, 0, 0,
                0, 0, -halfDepth, 0,
                viewport.Left, viewport.Top, Camera.OrthographicNearPlaneDistance, 1);
        }

        public Matrix4d ComputeOrthographicProjectionMatrix(Rectangle viewport)
        {
            //
            // Bottom and top swapped:  MS -> OpenGL
            //
            return Matrix4d.CreateOrthographicOffCenter(viewport.Left, viewport.Right, viewport.Top,
                viewport.Bottom, Camera.OrthographicNearPlaneDistance, Camera.OrthographicFarPlaneDistance);
        }

        public Matrix4d PerspectiveProjectionMatrix 
        {
            get
            {
                return Matrix4d.CreatePerspectiveFieldOfView(Camera.FieldOfViewY, Camera.AspectRatio,
                    Camera.PerspectiveNearPlaneDistance, Camera.PerspectiveFarPlaneDistance);
            }
        }

        public Matrix4d ViewMatrix
        {
            get  { return Matrix4d.LookAt(Camera.Eye, Camera.Target, Camera.Up); }
        }

        public Matrix4d ModelMatrix { get; set; }
        
        public Matrix4d ModelViewMatrix
        {
            get { return ModelMatrix * ViewMatrix; }
        }

        public Matrix4d ModelViewPerspectiveProjectionMatrix
        {
            get { return ModelViewMatrix * PerspectiveProjectionMatrix; }
        }

        // TODO:  Should return matrix in double precision
        public Matrix42 ModelZToClipCoordinates
        {
            get
            {
                //
                // Bottom two rows of model-view-projection matirx
                //
                Matrix4d m = ModelViewPerspectiveProjectionMatrix;
                return new Matrix42(
                    (float)m.M13, (float)m.M23, (float)m.M33, (float)m.M43,
                    (float)m.M14, (float)m.M24, (float)m.M34, (float)m.M44);
            }
        }
    }
}
