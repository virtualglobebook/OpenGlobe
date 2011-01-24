#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
                 
in vec2 fsTextureCoordinates;

out vec4 fragmentColor;

// og_texture0 - Day
// og_texture1 - Night
// og_texture2 - Blend
uniform int u_DayNightOutput;

void main()
{
	if (u_DayNightOutput == 0)  // DayNightOutput.Composite
	{
		vec4 dayColor = texture(og_texture0, fsTextureCoordinates);
		vec4 nightColor = texture(og_texture1, fsTextureCoordinates);
		float blend = texture(og_texture2, fsTextureCoordinates).r;
		fragmentColor = mix(nightColor, dayColor, blend);
	}
	else if (u_DayNightOutput == 1)  // DayNightOutput.DayBuffer
	{
		fragmentColor = texture(og_texture0, fsTextureCoordinates);
	}
	else if (u_DayNightOutput == 2)  // DayNightOutput.NightBuffer
	{
		fragmentColor = texture(og_texture1, fsTextureCoordinates);
	}
	else if (u_DayNightOutput == 3)  // DayNightOutput.BlendBuffer
	{
		fragmentColor = vec4(texture(og_texture2, fsTextureCoordinates).r);
	}
}