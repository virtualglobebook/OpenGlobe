#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

namespace OpenGlobe.Renderer
{
    public static class VertexLocations
    {
        public const int Position = 0;
        public const int Normal = 2;
        public const int TextureCoordinate = 3;
        public const int Color = 4;

        //
        // Having Position and PositionHigh share the same location
        // allows different shaders to share the same vertex array,
        // even if one is using DSFUN90 and one is not.
        //
        // FYI There is/was an ATI bug where location was required:
        //
        // http://www.opengl.org/discussion_boards/ubbthreads.php?ubb=showflat&Number=286280
        //
        public const int PositionHigh = Position;
        public const int PositionLow = 1;
    }
}