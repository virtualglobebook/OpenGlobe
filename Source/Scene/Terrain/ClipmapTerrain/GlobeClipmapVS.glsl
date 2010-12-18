#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//

in vec2 position;

out vec2 fsFineUv;
out vec2 fsCoarseUv;
out vec3 fsPositionToLight;
out float fsAlpha;
out float fsHeight;
out vec2 fsLonLat;

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
uniform vec3 u_globeRadiiSquared;
uniform sampler2D og_texture0;    // finer height map
uniform sampler2D og_texture1;    // coarser height map

vec3 GeodeticSurfaceNormal(vec3 geodetic)
{
	float cosLatitude = cos(geodetic.y);

	return vec3(
		cosLatitude * cos(geodetic.x),
		cosLatitude * sin(geodetic.x),
		sin(geodetic.y));
}

vec3 GeodeticToCartesian(vec3 globeRadiiSquared, vec3 geodetic)
{
	vec3 n = GeodeticSurfaceNormal(geodetic);
	vec3 k = globeRadiiSquared * n;
	vec3 g = k * n;
	float gamma = sqrt(g.x + g.y + g.z);

	vec3 rSurface = k / gamma;
	return rSurface + (geodetic.z * n);
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
	fsLonLat = worldPos;

	vec3 displacedPosition;
	if (abs(worldPos.y) > 90.0)
	{
		displacedPosition = vec3(0.0, 0.0, 0.0);
	}
	else
	{
		displacedPosition = GeodeticToCartesian(u_globeRadiiSquared, vec3(worldPos * og_radiansPerDegree, height));
	}

    fsPositionToLight = og_sunPosition - displacedPosition;

    gl_Position = og_modelViewPerspectiveMatrix * vec4(displacedPosition, 1.0);
}
