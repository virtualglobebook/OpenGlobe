#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//

in vec2 fsFineUv;
in vec2 fsCoarseUv;
in vec3 fsPositionToLight;
in float fsAlpha;
in float fsHeight;
                 
out vec3 fragmentColor;

uniform vec4 og_diffuseSpecularAmbientShininess;
uniform sampler2D og_texture2;    // finer normal map
uniform sampler2D og_texture3;    // coarser normal map

uniform bool u_showBlendRegions;
uniform vec3 u_color;
uniform vec3 u_blendRegionColor;

vec3 ComputeNormal()
{
    vec3 fineNormal = normalize(texture(og_texture2, fsFineUv).rgb);
	vec3 coarseNormal = normalize(texture(og_texture3, fsCoarseUv).rgb);
	return normalize(mix(fineNormal, coarseNormal, fsAlpha));
}

void main()
{
    vec3 normal = ComputeNormal();
    vec3 positionToLight = normalize(fsPositionToLight);

	float diffuse = og_diffuseSpecularAmbientShininess.x * max(dot(positionToLight, normal), 0.0);
	float intensity = diffuse + og_diffuseSpecularAmbientShininess.z;

	if (u_showBlendRegions)
		fragmentColor = mix(u_color, u_blendRegionColor, fsAlpha) * intensity;
	else
		fragmentColor = mix(u_color, vec3(0.0, 0.0, 1.0), fsHeight < 0.0) * intensity;
}
