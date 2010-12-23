#region License | 
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;

namespace OpenGlobe.Scene
{
    [Flags]
    public enum PerformanceCounters
    {
        NumberOfPointsRendered = 1,
        NumberOfLinesRendered = 2,
        NumberOfTrianglesRendered = 4,
        NumberOfPrimitivesRendered = 8,
        NumberOfDrawCalls = 16,
        NumberOfClearCalls = 32,
        MillisecondsPerFrame = 64,
        FramesPerSecond = 128,
        PointsPerSecond = 256,
        LinesPerSecond = 512,
        TrianglesPerSecond = 1024,
        PrimitivesPerSecond = 2048,
        All =
            NumberOfPointsRendered | 
            NumberOfLinesRendered | 
            NumberOfTrianglesRendered | 
            NumberOfPrimitivesRendered | 
            NumberOfDrawCalls | 
            NumberOfClearCalls | 
            MillisecondsPerFrame | 
            FramesPerSecond | 
            PointsPerSecond | 
            LinesPerSecond | 
            TrianglesPerSecond |
            PrimitivesPerSecond
    }
}