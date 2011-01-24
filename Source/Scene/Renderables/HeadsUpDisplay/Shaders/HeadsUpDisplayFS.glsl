#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
              
in vec2 fsTextureCoordinates;

out vec4 fragmentColor;

uniform vec3 u_color;
uniform bool u_showBackground;

void main()
{
    vec4 color = texture(og_texture0, fsTextureCoordinates);

	if (u_showBackground)
	{
		fragmentColor = vec4(
			mix(vec3(0.2), color.rgb * u_color.rgb, color.a),
			max(0.7, color.a));
	}
	else
	{
		if (color.a == 0.0)
		{
			discard;
		}
		fragmentColor = vec4(color.rgb * u_color.rgb, color.a);
	}
}