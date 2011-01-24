#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
                 
in vec3 fsNormal;
in vec3 fsPositionToLight;
in vec3 fsPositionToEye;
in vec2 textureCoordinate;
in vec2 repeatTextureCoordinate;
in float height;

out vec3 fragmentColor;

// og_texture6 - Color map
// og_texture1 - Color ramp for height
// og_texture7 - Color ramp for slope
// og_texture2 - Blend ramp for grass and stone
// og_texture3 - Grass
// og_texture4 - Stone
// og_texture5 - Blend map
uniform float u_minimumHeight;
uniform float u_maximumHeight;
uniform int u_normalAlgorithm;
uniform int u_shadingAlgorithm;
uniform bool u_showTerrain;
uniform bool u_showSilhouette;

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
    vec3 normal = normalize(fsNormal);
    vec3 positionToLight = normalize(fsPositionToLight);
    vec3 positionToEye = normalize(fsPositionToEye);

    if (u_showSilhouette)
    {
        if (abs(dot(normal, positionToEye)) < 0.25)
        {
            fragmentColor = vec3(0.0);
            return;
        }
    }

    if (!u_showTerrain)
    {
        discard;
    }

    float intensity = 1.0;
    if (u_normalAlgorithm != 0)   // TerrainNormalsAlgorithm.None
    {
        intensity = LightIntensity(normal,  positionToLight, positionToEye, og_diffuseSpecularAmbientShininess);
    }

    if (u_shadingAlgorithm == 0)  // TerrainShadingAlgorithm.ColorMap
    {
        fragmentColor = intensity * texture(og_texture6, textureCoordinate).rgb;
    }
    if (u_shadingAlgorithm == 1)  // TerrainShadingAlgorithm.Solid
    {
        fragmentColor = vec3(0.0, intensity, 0.0);
    }
    else if (u_shadingAlgorithm == 2)  // TerrainShadingAlgorithm.ByHeight
    {
        fragmentColor = vec3(0.0, intensity * ((height - u_minimumHeight) / (u_maximumHeight - u_minimumHeight)), 0.0);
    }
    else if (u_shadingAlgorithm == 3)  // TerrainShadingAlgorithm.HeightContour
    {
        float distanceToContour = mod(height, 5.0);  // Contour every 5 meters 
        float dx = abs(dFdx(height));
        float dy = abs(dFdy(height));
        float dF = max(dx, dy) * og_highResolutionSnapScale * 2.0;  // Line width

        fragmentColor = mix(vec3(0.0, intensity, 0.0), vec3(intensity, 0.0, 0.0), (distanceToContour < dF));
    }
    else if (u_shadingAlgorithm == 4)  // TerrainShadingAlgorithm.ColorRampByHeight
    {
        fragmentColor = intensity * texture(og_texture1, vec2(0.5, ((height - u_minimumHeight) / (u_maximumHeight - u_minimumHeight)))).rgb;
    }
    else if (u_shadingAlgorithm == 5)  // TerrainShadingAlgorithm.BlendRampByHeight
    {
        float normalizedHeight = (height - u_minimumHeight) / (u_maximumHeight - u_minimumHeight);
        fragmentColor = intensity * mix(
            texture(og_texture3, repeatTextureCoordinate).rgb,    // Grass
            texture(og_texture4, repeatTextureCoordinate).rgb,    // Stone
            texture(og_texture2, vec2(0.5, normalizedHeight)).r); // Blend Ramp
    }
    else if (u_shadingAlgorithm == 6)  // TerrainShadingAlgorithm.BySlope
    {
        fragmentColor = vec3(normal.z);
    }
    else if (u_shadingAlgorithm == 7)  // TerrainShadingAlgorithm.SlopeContour
    {
        float slopeAngle = acos(normal.z);
        float distanceToContour = mod(slopeAngle, radians(15.0));  // Contour every 15 degrees
        float dx = abs(dFdx(slopeAngle));
        float dy = abs(dFdy(slopeAngle));
        float dF = max(dx, dy) * og_highResolutionSnapScale * 2.0;  // Line width

        fragmentColor = mix(vec3(0.0, intensity, 0.0), vec3(intensity, 0.0, 0.0), (distanceToContour < dF));
    }
    else if (u_shadingAlgorithm == 8)  // TerrainShadingAlgorithm.ColorRampBySlope
    {
        fragmentColor = intensity * texture(og_texture7, vec2(0.5, normal.z)).rgb;
    }
    else if (u_shadingAlgorithm == 9)  // TerrainShadingAlgorithm.BlendRampBySlope
    {
        fragmentColor = intensity * mix(
            texture(og_texture4, repeatTextureCoordinate).rgb, // Stone
            texture(og_texture3, repeatTextureCoordinate).rgb, // Grass
            texture(og_texture2, vec2(0.5, normal.z)).r);
    }
    else if (u_shadingAlgorithm == 10)  // TerrainShadingAlgorithm.BlendMask
    {
        fragmentColor = intensity * mix(
            texture(og_texture3, repeatTextureCoordinate).rgb, // Grass
            texture(og_texture4, repeatTextureCoordinate).rgb, // Stone
            texture(og_texture5, textureCoordinate).r);        // Blend mask
    }
}
