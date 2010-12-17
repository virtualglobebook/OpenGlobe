#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
in vec4 position;
in vec3 color;

out vec3 fsColor;

uniform mat4 u_modelViewPerspectiveMatrixRelativeToEye;
uniform float u_pointSize;

void main()                     
{
    gl_Position = u_modelViewPerspectiveMatrixRelativeToEye * position; 
    gl_PointSize = u_pointSize;
    fsColor = color;
}