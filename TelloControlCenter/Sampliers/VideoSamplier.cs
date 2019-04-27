using OpenCvSharp;
using System;
using System.Threading;
using TelloControlCenter.Models;

namespace TelloControlCenter.Sampliers
{
    public class VideoSamplier
    {
        private Thread _thread;

        public int SamplingRate { get; set; }

        public ConnectionState Connection { get; set; } = ConnectionState.Disconnected;

        public DateTime LastTimeReceived { get; private set; }

        public VideoSamplier(int rate)
        {
            SamplingRate = rate;
        }

        public void Start(Action<Mat> setter, Action hb)
        {
            _thread = new Thread(Worker);
            _thread.Start(new Tuple<object,object>(setter, hb));
        }

        private void Worker(object p)
        {
            var tpl = (Tuple<object, object>)p;
            var setter = (Action<Mat>)tpl.Item1;
            var hb = (Action)tpl.Item2;
            var lastHb = DateTime.Now;
            var frameCounter = 0;
            try
            {
                var streamAddress = "udp://@0.0.0.0:11111";
                var s = new VideoCapture();

                while (true)
                {
                    var elapsed = DateTime.Now.Subtract(lastHb).TotalSeconds;
                    if (elapsed >= 1)
                    {
                        hb();
                        SamplingRate = frameCounter;
                        lastHb = DateTime.Now;
                        frameCounter = 0;
                    }

                    if (s.IsOpened() == false)
                    {
                        s.Open(streamAddress);
                        Connection = ConnectionState.Disconnected;
                    }
                    else
                    {
                        Connection = ConnectionState.Connected;
                    }
                    var mat = new Mat();
                    try
                    {
                        if (s.Read(mat))
                        {
                            frameCounter++;
                            LastTimeReceived = DateTime.Now;
                            setter(mat);
                            
                        }
                    }
                    catch (ThreadAbortException)
                    {
                        throw;
                    }
                    catch
                    {
                    }
                }
            }
            catch (ThreadAbortException)
            {
                return;
            }
        }

        public void Stop()
        {
            if (_thread == null)
            {
                return;
            }
            _thread.Abort();
            _thread.Join();
            _thread = null;
        }
    }
}
