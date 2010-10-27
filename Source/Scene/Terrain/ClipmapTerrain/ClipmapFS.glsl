#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//

in float height;
in vec3 normalFS;
in vec3 positionToLightFS;
in vec3 positionToEyeFS;
in vec2 modulus;
in vec2 textureCoordinateFS;
                 
out vec3 fragmentColor;

uniform vec4 og_diffuseSpecularAmbientShininess;
uniform vec3 u_color;
uniform sampler2DRect og_texture0;
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
    vec3 normal = normalize(normalFS);
    vec3 positionToLight = normalize(positionToLightFS);
    vec3 positionToEye = normalize(positionToEyeFS);

	float intensity = LightIntensity(normal, positionToLight, positionToEye, og_diffuseSpecularAmbientShininess);
	//float intensity = height / 2000.0;
	
	//fragmentColor = mix(vec3(u_color * intensity), vec3(0.0, 0.0, intensity), (modulus.x > -0.5 && modulus.x < 0.5) || (modulus.y > -0.5 && modulus.y < 0.5));
	//fragmentColor = mix(vec3(u_color * intensity), vec3(0.0, 0.0, intensity), height <= 0);
	vec3 color = texture(og_texture2, textureCoordinateFS).rgb;
	fragmentColor = u_color*0.000000001 + mix(color /*+ vec3(1.0, 0.0, 0.0)*/, color, height <= 0) * intensity;
	//fragmentColor = fragmentColor*0.0000001 + normal;
}
