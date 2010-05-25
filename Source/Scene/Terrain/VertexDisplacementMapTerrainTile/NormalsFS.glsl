#version 150
                 
in float distanceToEyeFS;
out vec4 fragmentColor;
uniform vec3 u_color;

void main()
{
    //
    // Apply linear attenuation to alpha
    //
    float a = min(1.0 / (0.015 * distanceToEyeFS), 1.0);
    if (a == 0.0)
    {
        discard;
    }

    fragmentColor = vec4(u_color, a);
}