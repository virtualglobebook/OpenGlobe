#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//

in vec3 normalFS;
in vec3 positionToLightFS;
in vec2 textureCoordinateFS;
                 
out vec3 fragmentColor;

uniform vec4 og_diffuseSpecularAmbientShininess;

void main()
{
    vec3 normal = normalize(normalFS);
    vec3 positionToLight = normalize(positionToLightFS);

	float diffuse = og_diffuseSpecularAmbientShininess.x * max(dot(positionToLight, normal), 0.0);
	float intensity = diffuse + og_diffuseSpecularAmbientShininess.z;
	fragmentColor = vec3(0.0, intensity, 0.0);
}
