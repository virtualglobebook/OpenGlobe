#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//

layout(location = og_positionVertexLocation) in vec2 position;
                  
out float distanceToEyeGS;

uniform mat4 og_modelViewPerspectiveMatrix;
uniform vec3 og_cameraEye;
uniform sampler2DRect og_texture0;    // Height map
uniform float u_heightExaggeration;

void main()
{
    vec3 displacedPosition = vec3(position, texture(og_texture0, position).r * u_heightExaggeration);

    gl_Position = vec4(displacedPosition, 1.0);
    distanceToEyeGS = distance(displacedPosition, og_cameraEye);
}