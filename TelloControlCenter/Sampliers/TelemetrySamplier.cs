using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TelloControlCenter.Models;

namespace TelloControlCenter.Sampliers
{
    public class TelemetrySamplier
    {
        private Thread _thread;

        private readonly string[] _sepa = new string[] { ";", "\n", "\r" };

        public int SamplingRate { get; private set; } = 0;
        public DateTime LastTimeReceived { get; private set; }
        public ConnectionState Connection { get; private set; } = ConnectionState.Disconnected;

        public void Start(Action<TelemetryState> setter, Action heartBeat)
        {
            _thread = new Thread(Worker);
            _thread.Start(new Tuple<object, object>(setter, heartBeat));
        }

        private void Worker(object para)
        {
            var tp = (Tuple<object, object>)para;
            var setter = (Action<TelemetryState>)tp.Item1;
            var hb = (Action)tp.Item2;
            var LastTimeReceived = DateTime.Now.AddYears(-20);
            var localEP = new IPEndPoint(IPAddress.Any, 8890);
            var remoteEP = new IPEndPoint(IPAddress.Any, 8889);
            var counter = 5000;
            var lastHb = DateTime.Now;
            try
            {
                using (var client = new UdpClient(localEP))
                {
                    while (true)
                    {
                        if (client.Available <= 0)
                        {
                            Thread.Sleep(20);
                            counter++;
                            if (counter > 20 && counter <= 100)
                            {
                                Connection = ConnectionState.Warning;
                            }
                            if (counter > 100)
                            {
                                Connection = ConnectionState.Disconnected;
                            }
                            if (DateTime.Now.Subtract(lastHb).TotalSeconds > 1)
                            {
                                hb();
                                lastHb = DateTime.Now;
                            }
                            continue;
                        }
                        counter = 0;
                        Connection = ConnectionState.Connected;
                        var dgram = client.Receive(ref remoteEP);
                        var ms = (int)DateTime.Now.Subtract(LastTimeReceived).TotalMilliseconds;
                        LastTimeReceived = DateTime.Now;
                        SamplingRate =  ms !=0 ? 1000/ms : 0;
                        var stateString = Encoding.ASCII.GetString(dgram).Split(_sepa, StringSplitOptions.RemoveEmptyEntries);
                        var state = ParseState(stateString);
                        setter(state);
                    }
                }
            }
            catch (ThreadAbortException)
            {
                return;
            }
        }

        private TelemetryState ParseState(string[] pairs)
        {
            var state = new TelemetryState();
            // pitch:-1;roll:0;yaw:-92;vgx:0;vgy:0;vgz:0;templ:47;temph:49;tof:10;h:0;bat:9;baro:93.10;time:0;agx:-23.00;agy:-8.00;agz:-995.00;
            float? temp = null;
            int some;
            foreach (var pair in pairs)
            {
                var tokens = pair.Split(':');
                switch (tokens[0])
                {
                    case "pitch":
                        state.Pitch = DegreeToValue(int.Parse(tokens[1]));
                        break;
                    case "roll":
                        state.Roll = DegreeToValue(int.Parse(tokens[1]));
                        break;
                    case "yaw":
                        state.Yaw = DegreeToValue(int.Parse(tokens[1]));
                        break;
                    case "vgx":
                        state.SpeedX = int.Parse(tokens[1]) / 100.0f;
                        break;
                    case "vgy":
                        state.SpeedY = int.Parse(tokens[1]) / 100.0f;
                        break;
                    case "vgz":
                        state.SpeedZ = int.Parse(tokens[1]) / 100.0f;
                        break;
                    case "templ":
                        some = int.Parse(tokens[1]);
                        if (temp == null)
                        {
                            temp = some;
                        }
                        else
                        {
                            temp = (temp.Value + some) / 2.0f;
                        }
                        break;
                    case "temph":
                        some = int.Parse(tokens[1]);
                        if (temp == null)
                        {
                            temp = some;
                        }
                        else
                        {
                            temp = (temp.Value + some) / 2.0f;
                        }
                        break;
                    case "tof":
                        state.TOF = int.Parse(tokens[1]) / 100.0f;
                        break;
                    case "h":
                        state.Height = int.Parse(tokens[1]) / 100.0f;
                        break;
                    case "bat":
                        state.Battery = int.Parse(tokens[1]);
                        break;
                    case "baro":
                        state.Barometer = float.Parse(tokens[1]);
                        break;
                    case "time":
                        state.FlyTime = int.Parse(tokens[1]);
                        break;
                    case "agx":
                        state.AccelX = float.Parse(tokens[1]) / 100.0f;
                        break;
                    case "agy":
                        state.AccelY = float.Parse(tokens[1]) / 100.0f;
                        break;
                    case "agz":
                        state.AccelZ = float.Parse(tokens[1]) / 100.0f;
                        break;
                    default:
                        break;
                }
            }
            state.Temperature = temp.GetValueOrDefault();
            return state;
        }

        private float DegreeToValue(int degree)
        {
            return (degree + 90.0f) / 180.0f;
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
