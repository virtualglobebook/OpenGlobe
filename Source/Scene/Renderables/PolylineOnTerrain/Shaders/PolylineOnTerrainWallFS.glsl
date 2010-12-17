#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//          
    
uniform sampler2D og_texture0;
uniform sampler2D og_texture1;
uniform vec2 og_inverseViewportDimensions;
noperspective in vec2 vTexture0;
out vec3 fragmentColor;

void main()
{
    vec2 dZ = vec2(dFdx(gl_FragCoord.z), dFdy(gl_FragCoord.z));
    float z = gl_FragCoord.z;
    vec2 of = og_inverseViewportDimensions * gl_FragCoord.xy;
    float center = texture(og_texture1, of).r;
    if ((texture(og_texture0, of).r != 0.0) && (gl_FragCoord.z < center))
    {
        //
        // Fragment is above the terrain
        //
        float upperLeft = textureOffset(og_texture1, of, ivec2(-1.0, 1.0)).r;
        float upperCenter = textureOffset(og_texture1, of, ivec2(0.0, 1.0)).r;
        float upperRight = textureOffset(og_texture1, of, ivec2(1.0, 1.0)).r;
        float left = textureOffset(og_texture1, of, ivec2(-1.0, 0.0)).r;
        float right = textureOffset(og_texture1, of, ivec2(1.0, 0.0)).r;
        float lowerLeft = textureOffset(og_texture1, of, ivec2(-1.0, -1.0)).r;
        float lowerCenter = textureOffset(og_texture1, of, ivec2(0.0, -1.0)).r;
        float lowerRight = textureOffset(og_texture1, of, ivec2(1.0, -1.0)).r;

        float upperLeftM =  z - dZ.x + dZ.y;
        float upperCenterM = z + dZ.y;
        float upperRightM = z + dZ.x + dZ.y;
        float leftM = z - dZ.x;
        float rightM = z + dZ.x;
        float lowerLeftM = z - dZ.x - dZ.y;
        float lowerCenterM = z - dZ.y;
        float lowerRightM = z + dZ.x - dZ.y;

        if ((upperLeft < upperLeftM) ||
            (upperCenter < upperCenterM) ||
            (upperRight < upperRightM) ||
            (left < leftM) ||
            (right < rightM) ||
            (lowerLeft < lowerLeftM) ||
            (lowerCenter < lowerCenterM) ||
            (lowerRight < lowerRightM))
        {
            fragmentColor = vec3(1.0, 1.0, 0.0);
        }
        else
        {
            discard;
        }
    }
    else
    {
        discard;
    }
}