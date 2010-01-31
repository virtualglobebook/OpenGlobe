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
            //
            // Bottom and top swapped:  MS -> OpenGL
            //
            return Matrix4d.CreateOrthographicOffCenter(viewport.Left, viewport.Right, viewport.Top,
                viewport.Bottom, Camera.NearPlaneDistance, Camera.FarPlaneDistance);
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
