#version 150
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//

in vec4 position;
out vec3 worldPosition;
out vec3 positionToLight;
out vec3 positionToEye;

uniform mat4 mg_modelViewPerspectiveProjectionMatrix;
uniform vec3 mg_cameraEye;
uniform vec3 mg_cameraLightPosition;

void main()                     
{
    gl_Position = mg_modelViewPerspectiveProjectionMatrix * position; 

    worldPosition = position.xyz;
    positionToLight = mg_cameraLightPosition - worldPosition;
    positionToEye = mg_cameraEye - worldPosition;
}