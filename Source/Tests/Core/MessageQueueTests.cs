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
using NUnit.Framework;
using OpenGlobe.Core;
using System.Threading;

namespace OpenGlobe.Core
{
    [TestFixture]
    public class MessageQueueTests
    {
        [Test]
        public void TestPostBeforeStartInCurrentThread()
        {
            using (MessageQueue queue = new MessageQueue())
            {
                object received = null;
                queue.MessageReceived += delegate(object sender, MessageQueueEventArgs e)
                {
                    received = e.Message;
                    queue.Terminate();
                };

                object message = new object();
                queue.Post(message);
                queue.StartInCurrentThread();

                Assert.AreSame(message, received);
            }
        }

        [Test]
        public void TestPostAfterStartInAnotherThread()
        {
            using (MessageQueue queue = new MessageQueue())
            {
                object received = null;
                queue.MessageReceived += delegate(object sender, MessageQueueEventArgs e)
                {
                    received = e.Message;
                };

                object message = new object();
                queue.StartInAnotherThread();
                queue.Post(message);
                queue.TerminateAndWait();

                Assert.AreSame(message, received);
            }
        }

        [Test]
        public void TestSendAfterStartInAnotherThread()
        {
            using (MessageQueue queue = new MessageQueue())
            {
                object received = null;
                queue.MessageReceived += delegate(object sender, MessageQueueEventArgs e)
                {
                    received = e.Message;
                };

                object message = new object();
                queue.StartInAnotherThread();
                queue.Send(message);

                Assert.AreSame(message, received);

                queue.TerminateAndWait();
            }
        }

        [Test]
        public void TestContinueQueueAfterTerminate()
        {
            using (MessageQueue queue = new MessageQueue())
            {
                object received = null;
                queue.MessageReceived += delegate(object sender, MessageQueueEventArgs e)
                {
                    received = e.Message;
                };

                queue.StartInAnotherThread();

                // Keep the queue busy for 100 ms
                queue.Post(x => Thread.Sleep(100), null);

                // Post a message that will be quickly handled as soon as the above sleep finishes
                object message1 = new object();
                queue.Post(message1);

                // Post a termination request to the queue.
                queue.Terminate();

                // Post a second message - because this is after the Terminate it will not be
                // processed until the queue is restarted.
                object message2 = new object();
                queue.Post(message2);

                // Allow time for the messages processes so far (up to the terminate) to be processed.
                Thread.Sleep(500);

                // message1 should have been processed, but not message2.
                Assert.AreSame(message1, received);

                // Once we restart the queue, message2 should be processed.
                queue.StartInAnotherThread();
                queue.TerminateAndWait();
                Assert.AreSame(message2, received);
            }
        }

        [Test]
        public void TestProcessQueue()
        {
            using (MessageQueue queue = new MessageQueue())
            {

                object received = null;
                queue.MessageReceived += delegate(object sender, MessageQueueEventArgs e)
                {
                    received = e.Message;
                };

                object message = new object();
                queue.Post(message);

                queue.ProcessQueue();
                Assert.AreSame(message, received);
            }
        }

        [Test]
        public void TestCantStartQueueTwice()
        {
            using (MessageQueue queue = new MessageQueue())
            {
                queue.StartInAnotherThread();

                try
                {
                    queue.StartInCurrentThread();
                    Assert.Fail();
                }
                catch (InvalidOperationException)
                {
                }

                try
                {
                    queue.ProcessQueue();
                    Assert.Fail();
                }
                catch (InvalidOperationException)
                {
                }

                try
                {
                    queue.StartInAnotherThread();
                    Assert.Fail();
                }
                catch (InvalidOperationException)
                {
                }
            }
        }
    }
}
