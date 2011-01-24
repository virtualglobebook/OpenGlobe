#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//

layout(location = og_positionVertexLocation) in vec2 position;

out vec2 fsTextureCoordinates;

uniform vec2 u_updateSize;
uniform vec2 u_origin;
uniform vec2 u_oneOverHeightMapSize;

void main()                     
{
    vec2 coordinates = position * u_updateSize + u_origin;
    gl_Position = og_viewportOrthographicMatrix * vec4(coordinates, 0.0, 1.0);
    fsTextureCoordinates = coordinates * u_oneOverHeightMapSize;
}