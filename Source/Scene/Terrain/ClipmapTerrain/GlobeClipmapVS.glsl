#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//

in vec2 position;

out vec2 fsFineUv;
out vec2 fsCoarseUv;
out vec3 fsPositionToLight;
out float fsAlpha;
out float fsHeight;

uniform mat4 og_modelViewPerspectiveMatrix;
uniform vec3 og_sunPosition;
uniform vec3 u_sunPositionRelativeToViewer;
uniform vec2 u_patchOriginInClippedLevel;
uniform vec2 u_levelScaleFactor;
uniform vec2 u_levelZeroWorldScaleFactor;
uniform vec2 u_levelOffsetFromWorldOrigin;
uniform vec2 u_fineLevelOriginInCoarse;
uniform vec2 u_viewPosInClippedLevel;
uniform vec2 u_unblendedRegionSize;
uniform vec2 u_oneOverBlendedRegionSize;
uniform vec2 u_fineTextureOrigin;
uniform float u_heightExaggeration;
uniform float u_oneOverClipmapSize;
uniform bool u_useBlendRegions;
uniform sampler2D og_texture0;    // finer height map
uniform sampler2D og_texture1;    // coarser height map

vec3 GeodeticToCartesian(vec3 geodetic)
{
    vec2 geodeticRadians = geodetic.xy * og_radiansPerDegree;
    vec2 cosGeodetic = cos(geodeticRadians);
    vec2 sinGeodetic = sin(geodeticRadians);

    vec3 normal = vec3(cosGeodetic.y * cosGeodetic.x,
                       cosGeodetic.y * sinGeodetic.x,
                       sinGeodetic.y);
    //const vec3 Radii = vec3(1.0, 1.0, 0.99664718933522437664791458697109);
    const vec3 Radii = vec3(6378137.0, 6378137.0, 6356752.314245);
    vec3 position = normalize(normal * Radii) * Radii;
    return normal * geodetic.z + position;
}

float SampleHeight(vec2 levelPos)
{
    fsFineUv = (levelPos + u_fineTextureOrigin) * u_oneOverClipmapSize;
    fsCoarseUv = (levelPos * 0.5 + u_fineLevelOriginInCoarse) * u_oneOverClipmapSize;

    if (u_useBlendRegions)
    {
        vec2 alpha = clamp((abs(levelPos - u_viewPosInClippedLevel) - u_unblendedRegionSize) * u_oneOverBlendedRegionSize, 0, 1);
        fsAlpha = max(alpha.x, alpha.y);
    }
    else
    {
        fsAlpha = 0.0;
    }

    float fineHeight = texture(og_texture0, fsFineUv).r;
    float coarseHeight = texture(og_texture1, fsCoarseUv).r;
    return mix(fineHeight, coarseHeight, fsAlpha) * u_heightExaggeration;
}

void main()
{
    vec2 levelPos = position + u_patchOriginInClippedLevel;

    float height = SampleHeight(levelPos);
	fsHeight = height;
    vec2 worldPos = (levelPos + u_levelOffsetFromWorldOrigin) * u_levelScaleFactor * u_levelZeroWorldScaleFactor;
    vec3 displacedPosition = GeodeticToCartesian(vec3(worldPos, height));

    fsPositionToLight = og_sunPosition - displacedPosition;

    gl_Position = og_modelViewPerspectiveMatrix * vec4(displacedPosition, 1.0);
}
