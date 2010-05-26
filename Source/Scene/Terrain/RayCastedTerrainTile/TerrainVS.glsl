#version 150
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//

in vec4 position;
out vec3 boxExit;
uniform mat4 mg_modelViewPerspectiveProjectionMatrix;

void main()
{
    gl_Position = mg_modelViewPerspectiveProjectionMatrix * position;
    boxExit = position.xyz;
}