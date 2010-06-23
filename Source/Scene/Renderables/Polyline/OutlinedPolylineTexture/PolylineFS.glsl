#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//

flat in vec4 fsColor;
flat in vec4 fsOutlineColor;
in vec2 fsTextureCoordinate;

out vec4 fragmentColor;

uniform sampler2D og_texture0;

void main()
{
    vec2 texel = texture(og_texture0, fsTextureCoordinate).rg;
    float fill = texel.r;
    float alpha = texel.g;

    vec4 color = mix(fsOutlineColor, fsColor, fill);
    fragmentColor = vec4(color.rgb, color.a * alpha);
}