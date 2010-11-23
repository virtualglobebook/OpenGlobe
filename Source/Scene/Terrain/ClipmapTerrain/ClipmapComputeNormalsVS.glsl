#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//

layout(location = og_positionVertexLocation) in vec2 position;

out vec2 fsTextureCoordinates;
out vec2 fsPosition;

uniform mat4 og_viewportOrthographicMatrix;
uniform vec2 u_updateSize;
uniform vec2 u_origin;
uniform vec2 u_oneOverHeightMapSize;

void main()                     
{
    vec2 coordinates = position * u_updateSize + u_origin;
    gl_Position = og_viewportOrthographicMatrix * vec4(coordinates, 0.0, 1.0);
    fsPosition = coordinates;
    fsTextureCoordinates = coordinates * u_oneOverHeightMapSize;
}