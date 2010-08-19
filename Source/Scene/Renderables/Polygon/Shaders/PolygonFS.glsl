#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
                 
in vec3 worldPosition;
in vec3 positionToLight;
in vec3 positionToEye;
out vec4 fragmentColor;

uniform vec4 og_diffuseSpecularAmbientShininess;
uniform vec3 u_globeOneOverRadiiSquared;
uniform vec4 u_color;

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

vec3 ComputeDeticSurfaceNormal(vec3 positionOnEllipsoid, vec3 oneOverEllipsoidRadiiSquared)
{
    return normalize(positionOnEllipsoid * oneOverEllipsoidRadiiSquared);
}

void main()
{
	vec3 normal = ComputeDeticSurfaceNormal(worldPosition, u_globeOneOverRadiiSquared);
    float intensity = LightIntensity(normal,  normalize(positionToLight), normalize(positionToEye), og_diffuseSpecularAmbientShininess);

	fragmentColor = vec4(intensity * u_color.rgb, u_color.a);
}