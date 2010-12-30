#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using OpenGlobe.Core;

namespace OpenGlobe.Renderer
{
    internal class FrameWorker
    {
        public void Process(object sender, MessageQueueEventArgs e)
        {
            FrameRequest request = (FrameRequest)e.Message;
            request.Bitmap.Save(request.Filename);
            request.Dispose();
        }
    }
}
