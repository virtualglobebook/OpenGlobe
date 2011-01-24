#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//

layout(location = og_positionVertexLocation) in vec2 position;

out vec2 fsTextureCoordinates;

uniform vec2 u_sourceOrigin;
uniform vec2 u_updateSize;
uniform vec2 u_destinationOffset;

void main()                     
{
    vec2 scaledPosition = position * u_updateSize;
    gl_Position = og_viewportOrthographicMatrix * vec4(scaledPosition + u_destinationOffset, 0.0, 1.0);
    fsTextureCoordinates = scaledPosition + u_sourceOrigin;
}