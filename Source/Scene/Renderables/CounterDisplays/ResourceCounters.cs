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
    public enum ResourceCounters
    {
        NumberOfShaderProgramsCreated = 1,
        NumberOfVertexBuffersCreated = 2,
        NumberOfIndexBuffersCreated = 4,
        NumberOfTexturesCreated = 8,
        NumberOfFencesCreated = 16,
        All =
            NumberOfShaderProgramsCreated | 
            NumberOfVertexBuffersCreated | 
            NumberOfIndexBuffersCreated | 
            NumberOfTexturesCreated | 
            NumberOfFencesCreated
    }
}