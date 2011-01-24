#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//

layout(location = og_positionVertexLocation) in vec2 position;
out vec2 windowPosition;
out float gsDistanceToEye;

uniform sampler2DRect u_heightMap;
uniform float u_heightExaggeration;

void main()                     
{
    vec4 displacedPosition = vec4(position, texture(u_heightMap, position).r * u_heightExaggeration, 1.0);

    gl_Position = og_modelViewPerspectiveMatrix * displacedPosition;
    windowPosition = og_ClipToWindowCoordinates(gl_Position, og_viewportTransformationMatrix).xy;
    gsDistanceToEye = distance(displacedPosition.xyz, og_cameraEye);
}