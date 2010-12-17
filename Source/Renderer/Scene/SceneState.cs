#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using OpenTK;
using System.Drawing;
using System.Diagnostics;
using OpenGlobe.Core;

namespace OpenGlobe.Renderer
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
            SunPosition = new Vector3D(200000, 0, 0);
            ModelMatrix = Matrix4D.Identity;
            HighResolutionSnapScale = 1;
        }

        public float DiffuseIntensity { get; set; }
        public float SpecularIntensity { get; set; }
        public float AmbientIntensity { get; set; }
        public float Shininess { get; set; }
        public Camera Camera { get; set; }

        public Vector3D SunPosition { get; set; }

        public Vector3D CameraLightPosition
        {
            get { return Camera.Eye; }
        }

        public Matrix4D ComputeViewportTransformationMatrix(Rectangle viewport, double nearDepthRange, double farDepthRange)
        {
            double halfWidth = viewport.Width * 0.5;
            double halfHeight = viewport.Height * 0.5;
            double halfDepth = (farDepthRange - nearDepthRange) * 0.5;

            //
            // Bottom and top swapped:  MS -> OpenGL
            //
            return new Matrix4D(
                halfWidth, 0.0,        0.0,       viewport.Left + halfWidth,
                0.0,       halfHeight, 0.0,       viewport.Top + halfHeight,
                0.0,       0.0,        halfDepth, nearDepthRange + halfDepth,
                0.0,       0.0,        0.0,       1.0);
        }

        public static Matrix4D ComputeViewportOrthographicMatrix(Rectangle viewport)
        {
            //
            // Bottom and top swapped:  MS -> OpenGL
            //
            return Matrix4D.CreateOrthographicOffCenter(
                viewport.Left, viewport.Right, 
                viewport.Top, viewport.Bottom, 
                0.0, 1.0);
        }

        public Matrix4D OrthographicMatrix
        {
            //
            // Bottom and top swapped:  MS -> OpenGL
            //
            get
            {
                return Matrix4D.CreateOrthographicOffCenter(Camera.OrthographicLeft, Camera.OrthographicRight, 
                    Camera.OrthographicTop, Camera.OrthographicBottom,
                    Camera.OrthographicNearPlaneDistance, Camera.OrthographicFarPlaneDistance);
            }
        }

        public Matrix4D PerspectiveMatrix 
        {
            get
            {
                return Matrix4D.CreatePerspectiveFieldOfView(Camera.FieldOfViewY, Camera.AspectRatio,
                    Camera.PerspectiveNearPlaneDistance, Camera.PerspectiveFarPlaneDistance);
            }
        }

        public Matrix4D ViewMatrix
        {
            get { return Matrix4D.LookAt(Camera.Eye, Camera.Target, Camera.Up); }
        }

        public Matrix4D ModelMatrix { get; set; }

        public Matrix4D ModelViewMatrix
        {
            get { return ViewMatrix * ModelMatrix; }
        }

        public Matrix4D ModelViewMatrixRelativeToEye
        {
            get 
            {
                Matrix4D m = ModelViewMatrix;
                return new Matrix4D(
                    m.Column0Row0, m.Column1Row0, m.Column2Row0, 0.0,
                    m.Column0Row1, m.Column1Row1, m.Column2Row1, 0.0,
                    m.Column0Row2, m.Column1Row2, m.Column2Row2, 0.0,
                    m.Column0Row3, m.Column1Row3, m.Column2Row3, m.Column3Row3);
            }
        }

        public Matrix4D ModelViewPerspectiveMatrixRelativeToEye
        {
            get { return PerspectiveMatrix * ModelViewMatrixRelativeToEye; }
        }

        public Matrix4D ModelViewPerspectiveMatrix
        {
            get { return PerspectiveMatrix * ModelViewMatrix; }
        }

        public Matrix4D ModelViewOrthographicMatrix
        {
            get { return ModelViewMatrix * OrthographicMatrix; }
        }

        public Matrix42<double> ModelZToClipCoordinates
        {
            get
            {
                //
                // Bottom two rows of model-view-projection matrix
                //
                Matrix4D m = ModelViewPerspectiveMatrix;
                return new Matrix42<double>(
                    m.Column0Row2, m.Column1Row2, m.Column2Row2, m.Column3Row2,
                    m.Column0Row3, m.Column1Row3, m.Column2Row3, m.Column3Row3);
            }
        }

        public double HighResolutionSnapScale { get; set; }
    }
}
