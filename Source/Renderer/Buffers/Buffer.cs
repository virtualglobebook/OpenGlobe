#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

namespace OpenGlobe.Renderer
{
    public enum BufferHint
    {
        StreamDraw,
        StreamRead,
        StreamCopy,
        StaticDraw,
        StaticRead,
        StaticCopy,
        DynamicDraw,
        DynamicRead,
        DynamicCopy,
    }

    public enum WritePixelBufferHint
    {
        StreamDraw,
        StaticDraw,
        DynamicDraw,
    }

    public enum ReadPixelBufferHint
    {
        StreamRead,
        StaticRead,
        DynamicRead,
    }
}
