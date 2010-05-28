#version 150
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
               
in vec3 worldPosition;
in vec3 positionToLight;
in vec3 positionToEye;
out vec3 fragmentColor;

uniform vec2 u_gridLineWidth;
uniform vec2 u_gridResolution;
uniform vec3 u_globeOneOverRadiiSquared;

uniform vec4 mg_diffuseSpecularAmbientShininess;
uniform sampler2D mg_texture0;

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

vec2 ComputeTextureCoordinates(vec3 normal)
{
    return vec2(atan(normal.y, normal.x) * mg_oneOverTwoPi + 0.5, asin(normal.z) * mg_oneOverPi + 0.5);
}

void main()
{
    vec3 normal = ComputeDeticSurfaceNormal(worldPosition, u_globeOneOverRadiiSquared);
    vec2 textureCoordinate = ComputeTextureCoordinates(normal);

    vec2 distanceToLine = mod(textureCoordinate, u_gridResolution);
    vec2 dx = abs(dFdx(textureCoordinate));
    vec2 dy = abs(dFdy(textureCoordinate));
    vec2 dF = vec2(max(dx.s, dy.s), max(dx.t, dy.t)) * u_gridLineWidth;

//                      if (abs(0.5 - textureCoordinate.t) < (dF.y * 2.0))                        // Equator
//                      {
//                          fragmentColor = vec3(1.0, 1.0, 0.0);
//                          return;
//                      }
//                      else if ((abs(0.5 + (23.5 / 180.0) - textureCoordinate.t) < dF.y) ||      // Tropic of Cancer
//                               (abs(0.5 - (23.5 / 180.0) - textureCoordinate.t) < dF.y)  ||     // Tropic of Capricorn
//                               (abs(0.5 + (66.56083 / 180.0) - textureCoordinate.t) < dF.y) ||  // Arctic Circle
//                               (abs(0.5 - (66.56083 / 180.0) - textureCoordinate.t) < dF.y))    // Antarctic Circle
//                      {
//                          fragmentColor = vec3(1.0, 1.0, 0.0);
//                          return;
//                      }
//                      else if (abs(0.5 - textureCoordinate.s) < dF.x)                           // Prime Meridian
//                      {
//                          fragmentColor = vec3(0.0, 1.0, 0.0);
//                          return;
//                      }

    if (any(lessThan(distanceToLine, dF)))
    {
        fragmentColor = vec3(1.0, 0.0, 0.0);
    }
    else
    {
        float intensity = LightIntensity(normal,  normalize(positionToLight), normalize(positionToEye), mg_diffuseSpecularAmbientShininess);
        fragmentColor = intensity * texture(mg_texture0, textureCoordinate).rgb;
    }
}