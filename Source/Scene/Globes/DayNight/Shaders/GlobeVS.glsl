#version 150

in vec4 position;
out vec3 worldPosition;

uniform mat4 mg_modelViewPerspectiveProjectionMatrix;

void main()                     
{
    gl_Position = mg_modelViewPerspectiveProjectionMatrix * position; 
    worldPosition = position.xyz;
}