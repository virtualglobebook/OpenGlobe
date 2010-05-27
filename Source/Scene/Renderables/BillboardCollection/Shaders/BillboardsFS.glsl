#version 150
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//

in vec2 fsTextureCoordinates;
in vec4 fsColor;

out vec4 fragmentColor;

uniform sampler2D mg_texture0;

void main()
{
    vec4 color = texture(mg_texture0, fsTextureCoordinates);

    if (color.a == 0.0)
    {
        discard;
    }
    fragmentColor = vec4(color.rgb * fsColor.rgb, color.a);
}