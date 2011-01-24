#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//

layout(location = og_positionVertexLocation) in vec4 position;

uniform float u_heightExaggeration;

void main()                     
{
    vec4 exaggeratedPosition = vec4(position.xy, position.z * u_heightExaggeration, 1.0);
    gl_Position = og_modelViewPerspectiveMatrix * exaggeratedPosition;
}