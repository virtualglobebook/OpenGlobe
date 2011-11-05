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

//uniform vec4 og_diffuseSpecularAmbientShininess;
//uniform sampler2D og_texture2;    // finer normal map
//uniform sampler2D og_texture3;    // coarser normal map
//uniform sampler2D og_texture4;    // color map
//
uniform bool u_showImagery;
uniform bool u_showBlendRegions;
uniform bool u_shade;
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
    vec3 color = u_showImagery ? texture(og_texture4, fsFineColorUv).rgb : u_color;
    fragmentColor = u_showBlendRegions ? mix(color, u_blendRegionColor, fsAlpha) : color;

    if (u_shade)
    {
        vec3 positionToLight = normalize(fsPositionToLight);
        vec3 normal = ComputeNormal();
        float diffuse = og_diffuseSpecularAmbientShininess.x * max(dot(positionToLight, normal), 0.0);
    	float intensity = diffuse + og_diffuseSpecularAmbientShininess.z;
        fragmentColor *= intensity;
    }
}
