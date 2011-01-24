#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//

flat in vec4 fsColor;
flat in vec4 fsOutlineColor;
in float fsTextureCoordinate;

out vec4 fragmentColor;

void main()
{
    vec2 texel = texture(og_texture0, vec2(fsTextureCoordinate, 0.5)).rg;
    float interior = texel.r;
    float alpha = texel.g;

    vec4 color = mix(fsOutlineColor, fsColor, interior);
    fragmentColor = vec4(color.rgb, color.a * alpha);
}