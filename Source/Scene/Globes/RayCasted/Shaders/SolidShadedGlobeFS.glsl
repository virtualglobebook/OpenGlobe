#version 150
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
             
in vec3 worldPosition;
out vec3 fragmentColor;

uniform mat4x2 mg_modelZToClipCoordinates;
uniform vec4 mg_diffuseSpecularAmbientShininess;
uniform vec3 mg_cameraEye;
uniform vec3 u_cameraEyeSquared;
uniform vec3 u_globeOneOverRadiiSquared;

struct Intersection
{
    bool  Intersects;
    float Time;         // Along ray
};

//
// Assumes ellipsoid is at (0, 0, 0)
//
Intersection RayIntersectEllipsoid(vec3 rayOrigin, vec3 rayOriginSquared, vec3 rayDirection, vec3 oneOverEllipsoidRadiiSquared)
{
    float a = dot(rayDirection * rayDirection, oneOverEllipsoidRadiiSquared);
    float b = 2.0 * dot(rayOrigin * rayDirection, oneOverEllipsoidRadiiSquared);
    float c = dot(rayOriginSquared, oneOverEllipsoidRadiiSquared) - 1.0;
    float discriminant = b * b - 4.0 * a * c;

    if (discriminant < 0.0)
    {
        return Intersection(false, 0.0);
    }
    else if (discriminant == 0.0)
    {
        return Intersection(true, -0.5 * b / a);
    }

    float t = -0.5 * (b + (b > 0.0 ? 1.0 : -1.0) * sqrt(discriminant));
    float root1 = t / a;
    float root2 = c / t;

    return Intersection(true, min(root1, root2));
}

float ComputeWorldPositionDepth(vec3 position, mat4x2 modelZToClipCoordinates)
{ 
    vec2 v = modelZToClipCoordinates * vec4(position, 1);   // clip coordinates
    v.x /= v.y;                                             // normalized device coordinates
    v.x = (v.x + 1.0) * 0.5;
    return v.x;
}

void main()
{
    vec3 rayDirection = normalize(worldPosition - mg_cameraEye);
    Intersection i = RayIntersectEllipsoid(mg_cameraEye, u_cameraEyeSquared, rayDirection, u_globeOneOverRadiiSquared);

    if (i.Intersects)
    {
        vec3 position = mg_cameraEye + (i.Time * rayDirection);

        fragmentColor = vec3(0.0, 1.0, 1.0);
        gl_FragDepth = ComputeWorldPositionDepth(position, mg_modelZToClipCoordinates);
    }
    else
    {
        fragmentColor = vec3(0.2, 0.2, 0.2);
    }
}