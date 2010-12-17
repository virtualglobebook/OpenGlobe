#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//

in vec3 positionHigh;
in vec3 positionLow;
in vec3 color;

out vec3 fsColor;

uniform vec3 u_cameraEyeHigh;
uniform vec3 u_cameraEyeLow;
uniform mat4 u_modelViewPerspectiveMatrixRelativeToEye;
uniform float u_pointSize;

void main()                     
{
    vec3 highDifference = positionHigh - u_cameraEyeHigh;
    vec3 lowDifference = positionLow - u_cameraEyeLow;

    gl_Position = u_modelViewPerspectiveMatrixRelativeToEye * vec4(highDifference + lowDifference, 1.0);
    gl_PointSize = u_pointSize;
    fsColor = color;
}