#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//

flat in vec4 fsColor;
flat in vec4 fsOutlineColor;
in float fsTextureCoordinate;

out vec4 fragmentColor;

uniform sampler2D og_texture0;

void main()
{
    vec2 texel = texture(og_texture0, vec2(fsTextureCoordinate, 0.5)).rg;
    float interior = texel.r;
    float alpha = texel.g;

    vec4 color = mix(fsOutlineColor, fsColor, interior);
    fragmentColor = vec4(color.rgb, color.a * alpha);
}