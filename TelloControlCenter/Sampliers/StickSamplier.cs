using SharpDX.DirectInput;
using System;
using System.Linq;
using System.Threading;
using TelloControlCenter.Models;

namespace TelloControlCenter.Sampliers
{
    public class StickSamplier
    {
        private Thread _thread;

        public int SamplingRate { get; set; }

        public StickSamplier(int rate)
        {
            SamplingRate = rate;
        }


        private void Worker(object para)
        {
            var setter = (Action<StickState>)para;
            var di = new DirectInput();
            var device = di.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AllDevices).First();
            var joystick = new Joystick(di, device.InstanceGuid);
            joystick.Acquire();
            try
            {
                while (true)
                {
                    Thread.Sleep(1000 / SamplingRate);
                    var state = joystick.GetCurrentState();
                    var localState = new StickState
                    {
                        Yaw = state.RotationZ / (float)ushort.MaxValue,
                        Pitch = state.Y / (float)ushort.MaxValue,
                        Roll = state.X / (float)ushort.MaxValue,
                        Height = state.Sliders[0] / (float)ushort.MaxValue
                    };
                    setter(localState);
                }
            }
            catch (ThreadAbortException)
            {
                return;
            }
        }

        public void Start(Action<StickState> setter)
        {
            _thread = new Thread(Worker);
            _thread.Start(setter);
        }

        public void Stop()
        {
            if(_thread == null)
            {
                return;
            }
            _thread.Abort();
            _thread.Join();
            _thread = null;
        }
    }
}
