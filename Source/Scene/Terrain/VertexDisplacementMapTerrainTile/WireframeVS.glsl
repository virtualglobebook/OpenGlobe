#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//

in vec2 position;
out vec2 windowPosition;
out float distanceToEyeGS;

uniform mat4 og_modelViewPerspectiveProjectionMatrix;
uniform mat4 og_viewportTransformationMatrix;
uniform vec3 og_cameraEye;
uniform sampler2DRect og_texture0;    // Height map
uniform float u_heightExaggeration;

vec4 ClipToWindowCoordinates(vec4 v, mat4 viewportTransformationMatrix)
{
    v.xyz /= v.w;                                                  // normalized device coordinates
    v.xyz = (viewportTransformationMatrix * vec4(v.xyz, 1.0)).xyz; // windows coordinates
    return v;
}

void main()                     
{
    vec4 displacedPosition = vec4(position, texture(og_texture0, position).r * u_heightExaggeration, 1.0);

    gl_Position = og_modelViewPerspectiveProjectionMatrix * displacedPosition;
    windowPosition = ClipToWindowCoordinates(gl_Position, og_viewportTransformationMatrix).xy;
    distanceToEyeGS = distance(displacedPosition.xyz, og_cameraEye);
}