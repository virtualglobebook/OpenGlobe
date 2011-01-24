#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//

layout(location = og_positionVertexLocation) in vec2 position;
                  
out float gsDistanceToEye;

uniform sampler2DRect u_heightMap;
uniform float u_heightExaggeration;

void main()
{
    vec3 displacedPosition = vec3(position, texture(u_heightMap, position).r * u_heightExaggeration);

    gl_Position = vec4(displacedPosition, 1.0);
    gsDistanceToEye = distance(displacedPosition, og_cameraEye);
}