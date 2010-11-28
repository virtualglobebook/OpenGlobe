#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//

in vec3 fsNormal;
in vec3 fsPositionToLight;
in vec3 fsPositionToEye;
in vec2 fsTextureCoordinate;
                 
out vec3 fragmentColor;

uniform vec4 og_diffuseSpecularAmbientShininess;
uniform sampler2DRect og_texture2;

float LightIntensity(vec3 normal, vec3 toLight, vec3 toEye, vec4 diffuseSpecularAmbientShininess)
{
    vec3 toReflectedLight = reflect(-toLight, normal);

    float diffuse = max(dot(toLight, normal), 0.0);
    float specular = max(dot(toReflectedLight, toEye), 0.0);
    specular = pow(specular, diffuseSpecularAmbientShininess.w);

    return (diffuseSpecularAmbientShininess.x * diffuse) +
            (diffuseSpecularAmbientShininess.y * specular) +
            diffuseSpecularAmbientShininess.z;
}

void main()
{
    vec3 normal = normalize(fsNormal);
    vec3 positionToLight = normalize(fsPositionToLight);
    vec3 positionToEye = normalize(fsPositionToEye);

	float intensity = LightIntensity(normal, positionToLight, positionToEye, og_diffuseSpecularAmbientShininess);
	fragmentColor = texture(og_texture2, fsTextureCoordinate).rgb * intensity;
}
