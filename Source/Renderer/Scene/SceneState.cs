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
            ModelMatrix = Matrix4d.Identity;
        }

        public float DiffuseIntensity { get; set; }
        public float SpecularIntensity { get; set; }
        public float AmbientIntensity { get; set; }
        public float Shininess { get; set; }
        public Camera Camera { get; set; }

        public Vector3d LightPosition
        {
            get { return Camera.Eye + (3.0 * Camera.Up); }
        }

        public Matrix4d ComputeOrthographicProjectionMatrix(Rectangle viewport)
        {
            //Matrix4d m = Matrix4d.CreateOrthographic(800, 600, Camera.NearPlaneDistance, Camera.FarPlaneDistance);
            //return m;

            double left = viewport.Left;
            double bottom = viewport.Top;       // Swapped:  MS -> OpenGL
            double right = viewport.Width;
            double top = viewport.Bottom;
            double zNear = Camera.NearPlaneDistance;
            double zFar = Camera.FarPlaneDistance;

            double deltaX = right - left;
            double deltaY = top - bottom;
            double deltaZ = zFar - zNear;

            // TODO: -z doesn't work for normal orthographic rendering
            Matrix4d m = new Matrix4d(
              2.0 / deltaX, 0, 0, -(right + left) / deltaX,
              0, 2.0 / deltaY, 0, -(top + bottom) / deltaY,
              0, 0, -2.0 / deltaZ, -(zFar + zNear) / deltaZ,
              0, 0, 0, 1);
            m.Transpose();
            return m;
        }

        public Matrix4d PerspectiveProjectionMatrix 
        {
            get
            {
                return Matrix4d.CreatePerspectiveFieldOfView(Camera.FieldOfViewY, Camera.AspectRatio, 
                    Camera.NearPlaneDistance, Camera.FarPlaneDistance);
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
    }
}
