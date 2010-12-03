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

noperspective in vec3 fsDistanceToEdges;
in float fsDistanceToEye;

out vec3 fragmentColor;

uniform vec4 og_diffuseSpecularAmbientShininess;
uniform sampler2D og_texture2;    // finer normal map
uniform sampler2D og_texture3;    // coarser normal map
uniform float og_highResolutionSnapScale;

uniform bool u_showBlendRegions;
uniform vec3 u_color;
uniform vec3 u_blendRegionColor;
uniform bool u_wireFrame;

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

	vec3 terrainColor;

	if (u_showBlendRegions)
		terrainColor = mix(u_color, u_blendRegionColor, fsAlpha) * intensity;
	else
		terrainColor = u_color * intensity;

	if (!u_wireFrame)
	{
		fragmentColor = terrainColor;
	}
	else
	{
		//uniform float u_halfLineWidth;
		float u_halfLineWidth = 2.0 * og_highResolutionSnapScale;

		float d = min(fsDistanceToEdges.x, min(fsDistanceToEdges.y, fsDistanceToEdges.z));

		if (d > u_halfLineWidth + 1.0)
		{
			fragmentColor = terrainColor;
			return;
		}

		d = clamp(d - (u_halfLineWidth - 1.0), 0.0, 2.0);
		float a = exp2(-2.0 * d * d);

		//
		// Apply linear attenuation to alpha
		//
		a *= min(1.0 / (0.015 * fsDistanceToEye), 1.0);

		fragmentColor = mix(terrainColor, vec3(0.0), a);
	}
}
