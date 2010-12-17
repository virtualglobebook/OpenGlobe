#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//

layout(location = og_positionVertexLocation) in vec4 position;
layout(location = og_colorVertexLocation) in vec4 color;
out vec4 gsColor;

void main()                     
{
	gl_Position = position;
	gsColor = color;
}