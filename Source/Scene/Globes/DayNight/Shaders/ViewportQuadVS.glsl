#version 150

in vec4 position;
in vec2 textureCoordinates;

out vec2 fsTextureCoordinates;

uniform mat4 mg_viewportOrthographicProjectionMatrix;

void main()                     
{
    gl_Position = mg_viewportOrthographicProjectionMatrix * position;
    fsTextureCoordinates = textureCoordinates;
}