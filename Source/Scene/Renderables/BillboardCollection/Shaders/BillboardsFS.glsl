#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//

in vec2 fsTextureCoordinates;
in vec4 fsColor;

out vec4 fragmentColor;

uniform sampler2D og_texture0;

void main()
{
    vec4 color = texture(og_texture0, fsTextureCoordinates);

    if (color.a == 0.0)
    {
        discard;
    }
    fragmentColor = vec4(color.rgb * fsColor.rgb, color.a);
}