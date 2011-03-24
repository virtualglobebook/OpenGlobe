#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//

in vec2 fsFineUv;
in vec2 fsCoarseUv;
in vec2 fsFineColorUv;
in vec3 fsPositionToLight;
in float fsAlpha;
in float fsHeight;
in vec2 fsLonLat;
                 
out vec3 fragmentColor;

uniform vec4 og_diffuseSpecularAmbientShininess;
uniform sampler2D og_texture2;    // finer normal map
uniform sampler2D og_texture3;    // coarser normal map
uniform sampler2D og_texture4;    // color map

uniform bool u_showBlendRegions;
uniform vec3 u_color;
uniform vec3 u_blendRegionColor;

vec3 ComputeNormal()
{
    vec3 fineNormal = normalize(texture(og_texture2, fsFineUv).rgb);
	vec3 coarseNormal = normalize(texture(og_texture3, fsCoarseUv).rgb);
	return normalize(mix(fineNormal, coarseNormal, fsAlpha));
}

vec3 ComputeColor()
{
    vec3 fineColor = texture(og_texture4, fsFineColorUv).rgb;
    return fineColor;
    //return vec3(fsFineColorUv, 0.0) + fineColor * vec3(0.0, 0.00000001, 0.0);
}

void main()
{
    vec3 normal = ComputeNormal();
    vec3 color = ComputeColor();
    vec3 positionToLight = normalize(fsPositionToLight);

	float diffuse = og_diffuseSpecularAmbientShininess.x * max(dot(positionToLight, normal), 0.0);
	float intensity = diffuse + og_diffuseSpecularAmbientShininess.z;

	if (u_showBlendRegions)
		fragmentColor = mix(u_color, u_blendRegionColor, fsAlpha) * intensity;
	else
		fragmentColor = color; // * intensity;
}
