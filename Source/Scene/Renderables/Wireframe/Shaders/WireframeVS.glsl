#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//

layout(location = og_positionVertexLocation) in vec4 position;
out vec2 windowPosition;
uniform mat4 og_modelViewPerspectiveMatrix;
uniform mat4 og_viewportTransformationMatrix;

void main()                     
{
    gl_Position = og_modelViewPerspectiveMatrix * position;
    windowPosition = og_ClipToWindowCoordinates(gl_Position, og_viewportTransformationMatrix).xy;
}