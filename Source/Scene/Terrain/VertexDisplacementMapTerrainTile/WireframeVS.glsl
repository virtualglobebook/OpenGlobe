#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//

layout(location = og_positionVertexLocation) in vec2 position;
out vec2 windowPosition;
out float gsDistanceToEye;

uniform mat4 og_modelViewPerspectiveMatrix;
uniform mat4 og_viewportTransformationMatrix;
uniform vec3 og_cameraEye;
uniform sampler2DRect og_texture0;    // Height map
uniform float u_heightExaggeration;

void main()                     
{
    vec4 displacedPosition = vec4(position, texture(og_texture0, position).r * u_heightExaggeration, 1.0);

    gl_Position = og_modelViewPerspectiveMatrix * displacedPosition;
    windowPosition = og_ClipToWindowCoordinates(gl_Position, og_viewportTransformationMatrix).xy;
    gsDistanceToEye = distance(displacedPosition.xyz, og_cameraEye);
}