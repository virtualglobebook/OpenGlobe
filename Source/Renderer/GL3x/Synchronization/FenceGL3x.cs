#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using OpenGlobe.Renderer;
using OpenTK.Graphics.OpenGL;

namespace OpenGlobe.Renderer.GL3x
{
    public class FenceGL3x : Fence
    {
        public FenceGL3x()
        {
            _handle = new FenceHandleGL3x();
            //TODO:  When/where to GL.Fush?  If at all.  Or use SYNC_FLUSH_COMMANDS_BIT with ClientWait?
        }

        public override void ServerWait()
        {
            GL.WaitSync(_handle.Value, 0, (long)ArbSync.TimeoutIgnored);
        }

        public override ClientWaitResult ClientWait()
        {
            return ClientWait((int)ArbSync.TimeoutIgnored);
        }

        public override ClientWaitResult ClientWait(int timeoutInNanoseconds)
        {
            if ((timeoutInNanoseconds < 0) && (timeoutInNanoseconds != (int)ArbSync.TimeoutIgnored))
            {
                throw new ArgumentOutOfRangeException("timeoutInNanoseconds");
            }

            ArbSync result = GL.ClientWaitSync(_handle.Value, 0, timeoutInNanoseconds);

            switch (result)
            {
                case ArbSync.AlreadySignaled:
                    return ClientWaitResult.AlreadySignaled;
                case ArbSync.ConditionSatisfied:
                    return ClientWaitResult.Signaled;
                case ArbSync.TimeoutExpired:
                    return ClientWaitResult.TimeoutExpired;
            }

            return ClientWaitResult.TimeoutExpired;     // ArbSync.WaitFailed
        }

        public override SynchronizationStatus Status()
        {
            int length;
            int status;

            GL.GetSync(_handle.Value, ArbSync.SyncStatus, 1, out length, out status);

            if (status == (int)ArbSync.Unsignaled)
            {
                return SynchronizationStatus.Unsignaled;
            }
            else
            {
                return SynchronizationStatus.Signaled;
            }
        }

        #region Disposable Members

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _handle.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion

        private FenceHandleGL3x _handle;
    }
}
