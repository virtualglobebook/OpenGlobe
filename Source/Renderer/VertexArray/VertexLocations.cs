#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

namespace OpenGlobe.Renderer
{
    public static class VertexLocations
    {
        public const int Position = 0;
        public const int Normal = 1;
        public const int TextureCoordinate = 2;
        public const int Color = 3;

        //
        // We would prefer these not overlap Position and Normal above.
        // There is a potential bug on ATI:
        //
        // http://www.opengl.org/discussion_boards/ubbthreads.php?ubb=showflat&Number=286280
        //
        public const int PositionHigh = 0;
        public const int PositionLow = 1;
    }
}