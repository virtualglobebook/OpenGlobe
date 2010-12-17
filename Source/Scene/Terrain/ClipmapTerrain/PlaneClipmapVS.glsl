#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//

in vec2 position;

out vec2 gsFineUv;
out vec2 gsCoarseUv;
out vec3 gsPositionToLight;
out float gsAlpha;

out vec2 gsWindowPosition;
out float gsDistanceToEye;

uniform mat4 og_modelViewPerspectiveMatrix;
uniform mat4 og_viewportTransformationMatrix;
uniform vec3 og_cameraEye;
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

float SampleHeight(vec2 levelPos)
{
	gsFineUv = (levelPos + u_fineTextureOrigin) * u_oneOverClipmapSize;
	gsCoarseUv = (levelPos * 0.5 + u_fineLevelOriginInCoarse) * u_oneOverClipmapSize;

    if (u_useBlendRegions)
    {
	    vec2 alpha = clamp((abs(levelPos - u_viewPosInClippedLevel) - u_unblendedRegionSize) * u_oneOverBlendedRegionSize, 0, 1);
	    gsAlpha = max(alpha.x, alpha.y);
    }
    else
    {
        gsAlpha = 0.0;
    }

	float fineHeight = texture(og_texture0, gsFineUv).r;
	float coarseHeight = texture(og_texture1, gsCoarseUv).r;
	return mix(fineHeight, coarseHeight, gsAlpha) * u_heightExaggeration;
}

void main()
{
	vec2 levelPos = position + u_patchOriginInClippedLevel;

    float height = SampleHeight(levelPos);
	vec2 worldPos = levelPos * u_levelScaleFactor * u_levelZeroWorldScaleFactor + u_levelOffsetFromWorldOrigin;
	vec3 displacedPosition = vec3(worldPos, height);

    gsPositionToLight = og_sunPosition - displacedPosition;
	
    gl_Position = og_modelViewPerspectiveMatrix * vec4(displacedPosition, 1.0);

    gsWindowPosition = og_ClipToWindowCoordinates(gl_Position, og_viewportTransformationMatrix).xy;
    gsDistanceToEye = distance(displacedPosition.xyz, og_cameraEye);
}
