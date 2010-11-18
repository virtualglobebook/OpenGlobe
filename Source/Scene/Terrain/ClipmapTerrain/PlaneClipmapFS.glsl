#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//

in vec2 fineUvFS;
in vec2 coarseUvFS;
in vec3 positionToLightFS;
in float alphaFS;
                 
out vec3 fragmentColor;

uniform vec4 og_diffuseSpecularAmbientShininess;
uniform sampler2D og_texture2;    // finer normal map
uniform sampler2D og_texture3;    // coarser normal map

uniform bool u_showBlendRegions;

vec3 ComputeNormal()
{
    vec3 fineNormal = normalize(texture(og_texture2, fineUvFS).rgb);
	vec3 coarseNormal = normalize(texture(og_texture3, coarseUvFS).rgb);
	return normalize(mix(fineNormal, coarseNormal, alphaFS));
}

void main()
{
    vec3 normal = ComputeNormal();
    vec3 positionToLight = normalize(positionToLightFS);

	float diffuse = og_diffuseSpecularAmbientShininess.x * max(dot(positionToLight, normal), 0.0);
	float intensity = diffuse + og_diffuseSpecularAmbientShininess.z;

	if (u_showBlendRegions)
		fragmentColor = mix(vec3(0.0, intensity, 0.0), vec3(0.0, 0.0, intensity), alphaFS);
	else
		fragmentColor = vec3(0.0, intensity, 0.0);
}
