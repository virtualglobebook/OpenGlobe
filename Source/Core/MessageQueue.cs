#region License
//
// (C) Copyright 2010 Patrick Cozzi, Deron Ohlarik, and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Collections.Generic;
using System.Threading;

namespace OpenGlobe.Core
{
    /// <summary>
    /// A message queue for passing messages between threads.  Messages can be simple objects that are processed by
    /// the <see cref="MessageReceived"/> event, or they can be delegates which are invoked in
    /// the thread that is processing the queue.  The queue can be processed in a dedicated thread by calling
    /// <see cref="StartInAnotherThread"/>, or in an existing thread by calling <see cref="StartInCurrentThread"/>.
    /// Finally, the current contents of the queue can be executed just once by calling <see cref="ProcessQueue"/>.
    /// </summary>
    public class MessageQueue : IDisposable
    {
        /// <summary>
        /// Raised when a message is received by the message queue.  This event is raised in the thread that
        /// is handling messages for the queue.
        /// </summary>
        public event EventHandler<MessageQueueEventArgs> MessageReceived;

        /// <summary>
        /// Starts processing the queue in the current thread.  This method does not return until
        /// <see cref="Terminate"/> is called.
        /// </summary>
        public void StartInCurrentThread()
        {
            lock (_queue)
            {
                if (_state != State.Stopped)
                    throw new InvalidOperationException("The MessageQueue is already running.");
                _state = State.Running;
            }

            Run();
        }

        /// <summary>
        /// Starts processing the queue in a separate thread.  This method returns immediately.
        /// The thread created by this method will continue running until <see cref="Terminate"/>
        /// is called.
        /// </summary>
        public void StartInAnotherThread()
        {
            lock (_queue)
            {
                if (_state != State.Stopped)
                    throw new InvalidOperationException("The MessageQueue is already running.");
                _state = State.Running;
            }

            Thread thread = new Thread(Run);
            thread.IsBackground = true;
            thread.Start();
        }

        /// <summary>
        /// Processes messages currently in the queue using the calling thread.  This method returns as soon as all
        /// messages currently in the queue have been processed.
        /// </summary>
        public void ProcessQueue()
        {
            List<MessageInfo> current = null;

            lock (_queue)
            {
                if (_state != State.Stopped)
                    throw new InvalidOperationException("The MessageQueue is already running.");

                if (_queue.Count > 0)
                {
                    _state = State.Running;
                    current = new List<MessageInfo>(_queue);
                    _queue.Clear();
                }
            }

            if (current != null)
            {
                try
                {
                    ProcessCurrentQueue(current);
                }
                finally
                {
                    lock (_queue)
                    {
                        _state = State.Stopped;
                    }
                }
            }
        }

        /// <summary>
        /// Stops queue processing started by <see cref="StartInCurrentThread"/>, <see cref="StartInAnotherThread"/>,
        /// or <see cref="ProcessQueue"/>.  This method returns immediately without waiting for the message queue
        /// to stop.  To wait for the message queue to stop, call <see cref="TerminateAndWait"/> instead.  If the message
        /// queue is not running when this method is called, a "stop" message will be queued such that the message
        /// queue will be stopped when it starts processing messages again.  
        /// </summary>
        public void Terminate()
        {
            Post(StopQueue, null);
        }

        /// <summary>
        /// Stops queue processing started by <see cref="StartInCurrentThread"/>, <see cref="StartInAnotherThread"/>,
        /// or <see cref="ProcessQueue"/>.  This method does not return until the message queue has stopped.
        /// To signal the message queue to terminate without waiting, call <see cref="Terminate"/> instead.  If the message
        /// queue is not running when this method is called, a "stop" message will be queued such that the message
        /// queue will be stopped when it starts processing messages again, and the calling thread will be blocked
        /// until that happens.
        /// </summary>
        public void TerminateAndWait()
        {
            Send(StopQueue, null);
        }

        /// <summary>
        /// Posts a delegate to the queue.  This method returns immediately without waiting for the delegate to be invoked.
        /// </summary>
        /// <param name="callback">The callback to invoke when the message is processed.</param>
        /// <param name="userData">Optional data to pass to the <paramref name="callback"/> when it is invoked.</param>
        public void Post(Action<object> callback, object userData)
        {
            lock (_queue)
            {
                _queue.Add(new MessageInfo(callback, userData, null));
                Monitor.Pulse(_queue);
            }
        }

        /// <summary>
        /// Posts a message to the queue.  This method returns immediately without waiting for the message to be processed.
        /// </summary>
        /// <param name="message">The message to post to the queue.</param>
        public void Post(object message)
        {
            lock (_queue)
            {
                _queue.Add(new MessageInfo(null, message, null));
                Monitor.Pulse(_queue);
            }
        }

        /// <summary>
        /// Sends a delegate to the queue.  This method waits for the delegate to be invoked in the queue thread
        /// before returning.  Calling this message from the queue thread itself will result in a deadlock.
        /// </summary>
        /// <param name="callback">The callback to invoke when the message is processed.</param>
        /// <param name="userData">Optional data to pass to the <paramref name="callback"/> when it is invoked.</param>
        public void Send(Action<object> callback, object userData)
        {
            MessageInfo messageInfo = new MessageInfo(callback, userData, new object());
            lock (messageInfo.Done)
            {
                lock (_queue)
                {
                    _queue.Add(messageInfo);
                    Monitor.Pulse(_queue);
                }
                Monitor.Wait(messageInfo.Done);
            }
        }

        /// <summary>
        /// Sends a message to the queue.  This method waits for the delegate to be invoked in the queue thread
        /// before returning.  Calling this message from the queue thread itself will result in a deadlock.
        /// </summary>
        /// <param name="message">The message to post to the queue.</param>
        public void Send(object message)
        {
            MessageInfo messageInfo = new MessageInfo(null, message, new object());
            lock (messageInfo.Done)
            {
                lock (_queue)
                {
                    _queue.Add(messageInfo);
                    Monitor.Pulse(_queue);
                }
                Monitor.Wait(messageInfo.Done);
            }
        }

        /// <summary>
        /// Blocks the calling thread until a message is waiting in the queue.
        /// This message should only be called on queues for which messages are processed
        /// explicitly with a call to <see cref="ProcessQueue"/>.
        /// </summary>
        public void WaitForMessage()
        {
            lock (_queue)
            {
                while (_queue.Count == 0)
                {
                    Monitor.Wait(_queue);
                }
            }
        }

        /// <summary>
        /// Calls <see cref="Terminate"/>.
        /// </summary>
        public void Dispose()
        {
            Terminate();
        }

        private void Run()
        {
            try
            {
                List<MessageInfo> current = new List<MessageInfo>();

                do
                {
                    lock (_queue)
                    {
                        if (_queue.Count > 0)
                        {
                            current.AddRange(_queue);
                            _queue.Clear();
                        }
                        else
                        {
                            Monitor.Wait(_queue);

                            current.AddRange(_queue);
                            _queue.Clear();
                        }
                    }

                    ProcessCurrentQueue(current);
                    current.Clear();
                } while (_state == State.Running);
            }
            finally
            {
                lock (_queue)
                {
                    _state = State.Stopped;
                }
            }
        }

        private void ProcessCurrentQueue(List<MessageInfo> currentQueue)
        {
            for (int i = 0; i < currentQueue.Count; ++i)
            {
                if (_state == State.Stopping)
                {
                    // Push the remainder of 'current' back into '_queue'.
                    lock (_queue)
                    {
                        currentQueue.RemoveRange(0, i);
                        _queue.InsertRange(0, currentQueue);
                    }
                    break;
                }
                ProcessMessage(currentQueue[i]);
            }
        }

        private void ProcessMessage(MessageInfo message)
        {
            if (message.Callback != null)
            {
                message.Callback(message.Message);
            }
            else
            {
                EventHandler<MessageQueueEventArgs> e = MessageReceived;
                if (e != null)
                {
                    e(this, new MessageQueueEventArgs(message.Message));
                }
            }

            if (message.Done != null)
            {
                lock (message.Done)
                {
                    Monitor.Pulse(message.Done);
                }
            }
        }

        private void StopQueue(object userData)
        {
            _state = State.Stopping;
        }

        private struct MessageInfo
        {
            public MessageInfo(Action<object> callback, object message, object done)
            {
                Callback = callback;
                Message = message;
                Done = done;
            }
            public Action<object> Callback;
            public object Message;
            public object Done;
        }

        private enum State
        {
            Stopped,
            Running,
            Stopping,
        }

        private List<MessageInfo> _queue = new List<MessageInfo>();
        private State _state;
    }
}
