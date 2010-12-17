#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using OpenGlobe.Core;

namespace OpenGlobe.Renderer
{
    public enum SynchronizationStatus
    {
        Unsignaled,
        Signaled
    }

    public enum ClientWaitResult
    {
        AlreadySignaled,
        Signaled,
        TimeoutExpired
    }

    public abstract class Fence : Disposable
    {
        public abstract void ServerWait();
        public abstract ClientWaitResult ClientWait();
        public abstract ClientWaitResult ClientWait(int timeoutInNanoseconds);
        public abstract SynchronizationStatus Status();
    }
}
