#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;
using System.Globalization;
using System.IO;
using System.Threading;
using OpenGlobe.Core;

namespace OpenGlobe.Renderer
{
    public sealed class FrameCapture : IDisposable
    {
        public FrameCapture(GraphicsWindow window)
        {
            _window = window;
            window.PostRenderFrame += PostRenderFrame;
            window.Keyboard.KeyDown += OnKeyDown;

            _frameQueue = new MessageQueue();
            _frameQueue.MessageReceived += new FrameWorker().Process;

            Hotkey = KeyboardKey.F9;
            CancelHotkey = KeyboardKey.F10;
            HotkeysEnabled = true;
            OutputPath = "./";
            _state = FrameCaptureState.Stopped;
        }

        private void PostRenderFrame()
        {
            if (_state == FrameCaptureState.Capturing)
            {
                try
                {
                    if (_fullOutputPath == null)
                    {
                        _fullOutputPath = Path.Combine(OutputPath,
                            DateTime.Now.ToString("MM-dd-yyyy hh-mm-ss"));
                        Directory.CreateDirectory(_fullOutputPath);
                    }

                    string filename = Path.Combine(_fullOutputPath, 
                        "frame" + _frameNumber.ToString(NumberFormatInfo.InvariantInfo) + ".png");
                    ++_frameNumber;

                    using (Texture2D texture = Device.CreateTexture2D(
                        new Texture2DDescription(
                            _window.Width, _window.Height, TextureFormat.RedGreenBlue8)))
                    {
                        //
                        // A bitmap is posted instead of the texture to minimize
                        // to amount of video memory consumed while capturing.
                        // This increases the frame rate hit incurred by asynchronous 
                        // capturing because the rendering thread is responsible for 
                        // the texture to bitmap conversion.
                        //
                        texture.CopyFromFramebuffer();

                        if (Asynchronous)
                        {
                            _frameQueue.Post(new FrameRequest(filename, texture.CopyToBitmap()));
                        }
                        else
                        {
                            _frameQueue.Send(new FrameRequest(filename, texture.CopyToBitmap()));
                        }
                    }
                }
                catch (Exception e)
                {
                    Stop();
                    throw e;
                }
            }
        }

        private void OnKeyDown(object sender, KeyboardKeyEventArgs e)
        {
            if (HotkeysEnabled)
            {
                if (e.Key == Hotkey)
                {
                    if (_state == FrameCaptureState.Stopped)
                    {
                        Start();
                    }
                    else if (_state == FrameCaptureState.Capturing)
                    {
                        Stop();
                    }
                }
                else if (e.Key == CancelHotkey)
                {
                    Cancel();
                }
            }
        }

        public GraphicsWindow Window
        {
            get { return _window; }
        }

        public string OutputPath { get; set; }
        public KeyboardKey Hotkey { get; set; }
        public KeyboardKey CancelHotkey { get; set; }
        public bool HotkeysEnabled { get; set; }
        public bool Asynchronous { get; set; }

        public void Start()
        {
            if (_state == FrameCaptureState.Stopped)
            {
                //
                // A high priority thread is used because otherwise,
                // the consumer will not keep up with the producer,
                // and memory usage will skyrocket.
                //
                _state = FrameCaptureState.Capturing;
                _frameQueue.StartInAnotherThread(ThreadPriority.Highest);
            }
            else if (_state == FrameCaptureState.Paused)
            {
                _state = FrameCaptureState.Capturing;
            }
        }
        
        public void Stop()
        {
            if (_state != FrameCaptureState.Stopped)
            {
                _state = FrameCaptureState.Stopped;
                _frameNumber = 0;
                _fullOutputPath = null;
                _frameQueue.TerminateAndWait();
            }
        }

        public void Pause()
        {
            if (_state == FrameCaptureState.Capturing)
            {
                _state = FrameCaptureState.Paused;
            }
        }

        public void Cancel()
        {
            if (_state != FrameCaptureState.Stopped)
            {
                string outputPath = _fullOutputPath;

                Stop();

                if (outputPath != null)
                {
                    Directory.Delete(outputPath, true);
                }
            }
        }

        public FrameCaptureState State
        {
            get { return _state; }
        }

        #region IDisposable Members

        public void Dispose()
        {
            _frameQueue.TerminateAndWait();

            _window.PostRenderFrame -= PostRenderFrame;
            _window.Keyboard.KeyDown -= OnKeyDown;
            _frameQueue.Dispose();
        }

        #endregion

        private readonly GraphicsWindow _window;
        private readonly MessageQueue _frameQueue;
        private FrameCaptureState _state;
        private long _frameNumber;
        private string _fullOutputPath;
    }
}