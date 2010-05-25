#version 150

in vec2 position;
                  
out float distanceToEyeGS;

uniform mat4 mg_modelViewPerspectiveProjectionMatrix;
uniform vec3 mg_cameraEye;
uniform sampler2DRect mg_texture0;    // Height map
uniform float u_heightExaggeration;

void main()
{
    vec3 displacedPosition = vec3(position, texture(mg_texture0, position).r * u_heightExaggeration);

    gl_Position = vec4(displacedPosition, 1.0);
    distanceToEyeGS = distance(displacedPosition, mg_cameraEye);
}