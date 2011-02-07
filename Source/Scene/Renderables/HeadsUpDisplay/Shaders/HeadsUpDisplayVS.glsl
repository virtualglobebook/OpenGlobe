#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//

layout(location = og_positionVertexLocation) in vec2 position;
layout(location = og_textureCoordinateVertexLocation) in vec2 textureCoordinates;

out vec2 fsTextureCoordinates;

uniform vec2 u_originScale;
uniform vec2 u_halfTextureSize;

void main()                     
{
    vec2 center = position + (u_originScale * u_halfTextureSize);
    vec2 halfSize = u_halfTextureSize * ((textureCoordinates * 2.0) - 1.0);

    gl_Position = og_viewportOrthographicMatrix * vec4(center + halfSize, 0.0, 1.0);
    fsTextureCoordinates = textureCoordinates;
}