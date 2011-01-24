#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//

layout(location = og_positionVertexLocation) in vec4 position;
out vec3 worldPosition;
out vec3 positionToLight;
out vec3 positionToEye;

void main()                     
{
    gl_Position = og_modelViewPerspectiveMatrix * position; 

    worldPosition = position.xyz;
    positionToLight = og_cameraLightPosition - worldPosition;
    positionToEye = og_cameraEye - worldPosition;
}