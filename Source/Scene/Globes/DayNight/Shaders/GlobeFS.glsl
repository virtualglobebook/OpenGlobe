#version 150
                 
in vec3 worldPosition;
out vec4 dayColor;
out vec4 nightColor;
out float blendAlpha;

uniform mat4x2 mg_modelZToClipCoordinates;
uniform vec4 mg_diffuseSpecularAmbientShininess;
uniform sampler2D mg_texture0;                    // Day
uniform sampler2D mg_texture1;                    // Night
uniform vec3 mg_sunPosition;
uniform vec3 mg_cameraEye;
uniform vec3 u_cameraEyeSquared;
uniform vec3 u_globeOneOverRadiiSquared;
uniform float u_blendDuration;
uniform float u_blendDurationScale;
uniform bool u_useAverageDepth;

struct Intersection
{
    bool  Intersects;
    float NearTime;         // Along ray
    float FarTime;          // Along ray
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
        return Intersection(false, 0.0, 0.0);
    }
    else if (discriminant == 0.0)
    {
        float time = -0.5 * b / a;
        return Intersection(true, time, time);
    }

    float t = -0.5 * (b + (b > 0.0 ? 1.0 : -1.0) * sqrt(discriminant));
    float root1 = t / a;
    float root2 = c / t;

    return Intersection(true, min(root1, root2), max(root1, root2));
}

float ComputeWorldPositionDepth(vec3 position, mat4x2 modelZToClipCoordinates)
{ 
    vec2 v = modelZToClipCoordinates * vec4(position, 1);   // clip coordinates
    v.x /= v.y;                                             // normalized device coordinates
    v.x = (v.x + 1.0) * 0.5;
    return v.x;
}

vec3 ComputeDeticSurfaceNormal(vec3 positionOnEllipsoid, vec3 oneOverEllipsoidRadiiSquared)
{
    return normalize(positionOnEllipsoid * oneOverEllipsoidRadiiSquared);
}

float LightIntensity(vec3 normal, vec3 toLight, vec3 toEye, float diffuseDot, vec4 diffuseSpecularAmbientShininess)
{
    vec3 toReflectedLight = reflect(-toLight, normal);

    float diffuse = max(diffuseDot, 0.0);
    float specular = max(dot(toReflectedLight, toEye), 0.0);
    specular = pow(specular, diffuseSpecularAmbientShininess.w);

    return (diffuseSpecularAmbientShininess.x * diffuse) +
            (diffuseSpecularAmbientShininess.y * specular) +
            diffuseSpecularAmbientShininess.z;
}

vec2 ComputeTextureCoordinates(vec3 normal)
{
    return vec2(atan(normal.y, normal.x) * mg_oneOverTwoPi + 0.5, asin(normal.z) * mg_oneOverPi + 0.5);
}

vec3 NightColor(vec3 normal)
{
    return texture(mg_texture1, ComputeTextureCoordinates(normal)).rgb;
}

vec3 DayColor(vec3 normal, vec3 toLight, vec3 toEye, float diffuseDot, vec4 diffuseSpecularAmbientShininess)
{
    float intensity = LightIntensity(normal, toLight, toEye, diffuseDot, diffuseSpecularAmbientShininess);
    return intensity * texture(mg_texture0, ComputeTextureCoordinates(normal)).rgb;
}

void main()
{
    vec3 rayDirection = normalize(worldPosition - mg_cameraEye);
    Intersection i = RayIntersectEllipsoid(mg_cameraEye, u_cameraEyeSquared, rayDirection, u_globeOneOverRadiiSquared);

    if (i.Intersects)
    {
        vec3 position = mg_cameraEye + (i.NearTime * rayDirection);
        vec3 normal = ComputeDeticSurfaceNormal(position, u_globeOneOverRadiiSquared);

        vec3 toLight = normalize(mg_sunPosition - position);
        vec3 toEye = normalize(mg_cameraEye - position);

        float diffuse = dot(toLight, normal);

        dayColor = vec4(DayColor(normal, toLight, normalize(toLight), diffuse, mg_diffuseSpecularAmbientShininess), 1.0);
        nightColor = vec4(NightColor(normal), 1.0);
        blendAlpha = clamp((diffuse + u_blendDuration) * u_blendDurationScale, 0.0, 1.0);

        if (u_useAverageDepth)
        {
            position = mg_cameraEye + (mix(i.NearTime, i.FarTime, 0.5) * rayDirection);
        }

        gl_FragDepth = ComputeWorldPositionDepth(position, mg_modelZToClipCoordinates);
    }
    else
    {
        discard;
    }
}