#version 150
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
                 
in vec2 fsTextureCoordinates;

out vec4 fragmentColor;

uniform sampler2D mg_texture0;    // Day
uniform sampler2D mg_texture1;    // Night
uniform sampler2D mg_texture2;    // Blend

void main()
{
    vec4 dayColor = texture(mg_texture0, fsTextureCoordinates);
    vec4 nightColor = texture(mg_texture1, fsTextureCoordinates);
    float blend = texture(mg_texture2, fsTextureCoordinates).r;

    fragmentColor = mix(nightColor, dayColor, blend);
}