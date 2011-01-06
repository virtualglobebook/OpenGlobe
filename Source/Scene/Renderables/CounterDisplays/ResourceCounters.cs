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
        NumberOfQueriesCreated = 32,
        NumberOfVertexArraysCreated = 64,
        NumberOfFramebuffersCreated = 128,
        VertexBufferMemoryUsedInBytes = 256,
        IndexBufferMemoryUsedInBytes = 512,
        TextureMemoryUsedInBytes = 1024,
        All =
            NumberOfShaderProgramsCreated | 
            NumberOfVertexBuffersCreated | 
            NumberOfIndexBuffersCreated | 
            NumberOfTexturesCreated | 
            NumberOfFencesCreated |
            NumberOfQueriesCreated | 
            NumberOfVertexArraysCreated | 
            NumberOfFramebuffersCreated | 
            VertexBufferMemoryUsedInBytes |
            IndexBufferMemoryUsedInBytes |
            TextureMemoryUsedInBytes
    }
}