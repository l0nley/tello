using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TelloControlCenter.Drones
{
    public class Tello : IDisposable
    {
        private UdpClient _udp;
        private IPEndPoint _remote = new IPEndPoint(IPAddress.Parse("192.168.10.1"), 8889);
        private readonly Queue<string> _commands = new Queue<string>();
        private readonly Thread _worker;

        public Tello()
        {
            _worker = new Thread(Worker);
            _worker.Start();
        }

        public void Ignite()
        {
            SendUdpCommand("command");
        }

        public void SendRcCommand(int leftRight, int forwardBackward, int upDown, int yaw)
        {
            var command = $"rc {leftRight} {forwardBackward} {upDown} {yaw}";
            SendUdpCommand(command);
        }

        public void TakeOff()
        {
            SendUdpCommand("takeoff");
        }

        public void Land()
        {
            SendUdpCommand("land");
        }

        public void CutOff()
        {
            SendUdpCommand("emergency");
        }

        public void VideoOn()
        {
            SendUdpCommand("streamon");
        }

        public void VideoOff()
        {
            SendUdpCommand("streamoff");
        }

        private void SendUdpCommand(string command)
        {
            lock (_commands)
            {
                _commands.Enqueue(command);
            }
        }

        private void Worker()
        {
            if (_udp == null)
            {
                _udp = new UdpClient();
            }
            try
            {
                while (true)
                {
                    byte[] dgram;
                    if (_udp.Available > 0)
                    {
                        dgram = _udp.Receive(ref _remote);
                        var msg = Encoding.UTF8.GetString(dgram);
                        LogMessage?.Invoke(this, msg);
                    }
                    if (_commands.Count <= 0)
                    {
                        Thread.SpinWait(1);
                        continue;
                    }
                    string command;
                    lock (_commands)
                    {
                        command = _commands.Dequeue();
                    }
                    dgram = Encoding.UTF8.GetBytes(command);
                    LogMessage?.Invoke(this, command);
                    _udp.Send(dgram, dgram.Length, _remote);

                }
            }
            catch (ThreadAbortException)
            {
                return;
            }
        }

        public void Dispose()
        {
            _worker.Abort();
            _worker.Join();
            if (_udp != null)
            {
                _udp.Dispose();
                _udp = null;
            }
        }

        public event LogMessageDelegate LogMessage;
    }

    public delegate void LogMessageDelegate(object sender, string message);
}
