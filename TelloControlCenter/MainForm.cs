using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using TelloControlCenter.Drones;
using TelloControlCenter.Models;
using TelloControlCenter.Sampliers;

namespace TelloControlCenter
{
    public partial class MainForm : Form
    {
        private readonly StickSamplier _stickSamplier;
        private readonly TelemetrySamplier _teleSamplier;
        private readonly Tello _tello;
        private readonly VideoSamplier _vs;
        private bool _remoteControl;
        private bool _keyBoardEnabled;
        private int _yaw;
        private int _roll;
        private int _pitch;
        private int _height;

        public MainForm()
        {
            InitializeComponent();
            _stickSamplier = new StickSamplier(25);
            _teleSamplier = new TelemetrySamplier();
            _vs = new VideoSamplier(25);
            _tello = new Tello();
            _tello.LogMessage += _tello_MessageReceived;
        }

        private void _tello_MessageReceived(object sender, string message)
        {
            MethodInvoker dele = delegate ()
            {
                tbIncoming.AppendText($"{DateTime.Now.ToString("HH:mm:ss.fff")}> {message}\n");
                if (tbIncoming.Lines.Length > 100)
                {
                    tbIncoming.Text = string.Empty;
                }
            };
            Invoke(dele);
        }

        private void TglStickSampling_CheckedChanged(object sender, EventArgs e)
        {
            if (tglStickSampling.Checked)
            {
                _stickSamplier.Start(StickStateUpdater);
            }
            else
            {
                _stickSamplier.Stop();
            }
        }

        private void StickStateUpdater(StickState state)
        {
            stickStateControl.HeightAxis = state.Height;
            stickStateControl.YawAxis = state.Yaw;
            stickStateControl.RollAxis = state.Roll;
            stickStateControl.PitchAxis = state.Pitch;
            if (_remoteControl)
            {
                if (_keyBoardEnabled)
                {
                    var roll = _roll;
                    var pitch = _pitch;
                    var yaw = _yaw;
                    var h = _height;
                    _tello.SendRcCommand(roll, pitch, h, yaw);
                }
                else
                {
                    var roll = Normalize(state.Roll, rollDead.Value);
                    var pitch = -Normalize(state.Pitch, pitchDead.Value);
                    var h = -Normalize(state.Height, heightDead.Value);
                    var yaw = Normalize(state.Yaw, yawDead.Value);
                    _tello.SendRcCommand(roll, pitch, h, yaw);
                }
            }
            MethodInvoker dele = delegate ()
            {
                stickStateControl.Invalidate();
            };
            Invoke(dele);
        }

        private int Normalize(float value, float deadValue)
        {
            var inted = (int)((value - 0.5f) * 200);
            if (Math.Abs(inted) < deadValue)
            {
                return 0;
            }
            if (inted > 100)
            {
                return 100;
            }
            if (inted < -100)
            {
                return -100;
            }
            return inted;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _stickSamplier.Stop();
            _teleSamplier.Stop();
            _vs.Stop();
            _tello.Dispose();
        }

        private void TglTelemetryLink_CheckedChanged(object sender, EventArgs e)
        {
            if (tglTelemetryLink.Checked)
            {
                _teleSamplier.Start(TelemetryStateUpdater, TelemetryHb);
            }
            else
            {
                _teleSamplier.Stop();
            }
        }

        private void TelemetryHb()
        {
            MethodInvoker dele = delegate ()
            {
                var con = _teleSamplier.Connection;
                lblTelemetryConnectionState.Text = con.ToString();
                Color clr;
                switch (con)
                {
                    case ConnectionState.Connected:
                        clr = Color.LightGreen;
                        break;
                    case ConnectionState.Warning:
                        clr = Color.Yellow;
                        break;
                    default:
                        clr = Color.Red;
                        break;
                }
                lblTelemetryConnectionState.ForeColor = clr;
                lblTelemetrySampling.Text = _teleSamplier.SamplingRate.ToString();
            };
            Invoke(dele);
        }

        private void TelemetryStateUpdater(TelemetryState obj)
        {
            MethodInvoker dele = delegate ()
            {
                pfd1.YawAngle = obj.Yaw;
                pfd1.RollAngle = obj.Roll;
                pfd1.PitchAngle = obj.Pitch;
                pfd1.Altitude = obj.Height;
                pfd1.AirSpeed = 100.0f * (float)Math.Sqrt(obj.SpeedY * obj.SpeedY + obj.SpeedX * obj.SpeedX + obj.SpeedZ * obj.SpeedZ);
                pfd1.VerticalSpeed = obj.SpeedZ * 100.0f;
                pfd1.Invalidate();
                lbBar.Text = obj.Barometer.ToString();
                gBattery.Value = obj.Battery;
                gTemp.Value = obj.Temperature;
                gSpeedX.Value = 100.0f * obj.SpeedX;
                gSpeedY.Value = 100.0f * obj.SpeedY;
                gSpeedZ.Value = 100.0f * obj.SpeedZ;
                gAccelX.Value = obj.AccelX;
                gAccelY.Value = obj.AccelY;
                gAccelZ.Value = obj.AccelZ;
                lblTime.Text = obj.FlyTime.ToString();
                lblTof.Text = obj.TOF.ToString();
                lblBatLow.Visible = obj.Battery <= 20;
            };
            Invoke(dele);
        }

        private void BtnIgnite_Click(object sender, EventArgs e)
        {
            _tello.Ignite();
        }

        private void BtnCutOff_CLick(object sender, EventArgs e)
        {
            _tello.CutOff();
        }

        private void BtnVLinkOn_Click(object sender, EventArgs e)
        {
            _tello.VideoOn();
        }

        private void BtnVLinkOff_Click(object sender, EventArgs e)
        {
            _tello.VideoOff();
        }

        private void BtnTakeOff_Click(object sender, EventArgs e)
        {
            _tello.TakeOff();
        }

        private void BtnLand_Click(object sender, EventArgs e)
        {
            _tello.Land();
        }

        private void TglVideoLink_CheckedChanged(object sender, EventArgs e)
        {
            if (tglVideoLink.Checked)
            {
                _vs.Start(VideoApplier, VideHb);
            }
            else
            {
                _vs.Stop();
            }
        }

        private void VideHb()
        {
            MethodInvoker dele = delegate ()
            {
                lblNoLink.Visible = _vs.Connection == ConnectionState.Disconnected;
                lbVlState.Text = _vs.Connection.ToString();
                lbVlState.ForeColor = _vs.Connection == ConnectionState.Disconnected ? Color.Red : Color.Green;
                lbVLFPS.Text = _vs.SamplingRate.ToString();
            };
            Invoke(dele);
        }

        private void VideoApplier(Mat obj)
        {
            MethodInvoker dele = delegate ()
            {
                var size = new OpenCvSharp.Size(pbVideo.Width, pbVideo.Height);
                obj.Resize(size, interpolation: InterpolationFlags.Lanczos4);
                var bmp = BitmapConverter.ToBitmap(obj);
                pbVideo.Image = bmp;
                bmp = null;
            };
            Invoke(dele);
        }

        private void TglRemoteControl_CheckedChanged(object sender, EventArgs e)
        {
            _remoteControl = !_remoteControl;
        }

        private void MacTrackBar1_ValueChanged(object sender, decimal value)
        {
            _stickSamplier.SamplingRate = macTrackBar1.Value;
        }

        private void TglKeyboard_CheckedChanged(object sender, EventArgs e)
        {
            _keyBoardEnabled = tglKeyboard.Checked;
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            var _speed = btSpeed.Value;
            if (_keyBoardEnabled)
            {
                switch (e.KeyCode)
                {
                    case Keys.NumPad8:
                        _pitch = _speed;
                        e.Handled = true;
                        break;
                    case Keys.NumPad2:
                        _pitch = -_speed;
                        e.Handled = true;
                        break;
                    case Keys.NumPad4:
                        _roll = -_speed;
                        e.Handled = true;
                        break;
                    case Keys.NumPad6:
                        _roll = _speed;
                        e.Handled = true;
                        break;
                    case Keys.W:
                        _height = _speed;
                        e.Handled = true;
                        break;
                    case Keys.S:
                        _height = -_speed;
                        e.Handled = true;
                        break;
                    case Keys.NumPad7:
                        _yaw = -_speed;
                        e.Handled = true;
                        break;
                    case Keys.NumPad9:
                        _yaw = _speed;
                        e.Handled = true;
                        break;
                }
                ColorBtns();
            }
        }

        private void ColorBtns()
        {
            var clrFwd = Color.Black;
            var clrBk = Color.Black;
            if(_pitch > 0)
            {
                clrFwd = Color.WhiteSmoke;
            } 
            if(_pitch < 0)
            {
                clrBk = Color.WhiteSmoke;
            }
            var clrLeft = Color.Black;
            var clrRight = Color.Black;
            if(_roll <0)
            {
                clrLeft = Color.WhiteSmoke;
            }
            if(_roll >0)
            {
                clrRight = Color.WhiteSmoke;
            }
            var clrUp = Color.Black;
            var clrDown = Color.Black;
            if(_height >0)
            {
                clrUp = Color.WhiteSmoke;
            }
            if(_height <0)
            {
                clrDown = Color.WhiteSmoke;
            }
            var clrYawLeft = Color.Black;
            var clrYawRight = Color.Black;
            if(_yaw <0 )
            {
                clrYawLeft = Color.WhiteSmoke;
            }
            if(_yaw > 0)
            {
                clrYawRight = Color.WhiteSmoke;
            }
            btnYawLeft.BackColor = clrYawLeft;
            btnYRight.BackColor = clrYawRight;
            btnFwd.BackColor = clrFwd;
            btnBack.BackColor = clrBk;
            btnLeft.BackColor = clrLeft;
            btnRight.BackColor = clrRight;
            btnUp.BackColor = clrUp;
            btnDown.BackColor = clrDown;

            btnUp.Invalidate();
            btnDown.Invalidate();
            btnLeft.Invalidate();
            btnRight.Invalidate();
            btnFwd.Invalidate();
            btnBack.Invalidate();
            btnYawLeft.Invalidate();
            btnYRight.Invalidate();
        }

        private void MainForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (_keyBoardEnabled)
            {
                switch (e.KeyCode)
                {
                    case Keys.NumPad8:
                        _pitch = 0;
                        e.Handled = true;
                        break;
                    case Keys.NumPad2:
                        _pitch = 0;
                        e.Handled = true;
                        break;
                    case Keys.NumPad4:
                        _roll = 0;
                        e.Handled = true;
                        break;
                    case Keys.NumPad6:
                        _roll =0;
                        e.Handled = true;
                        break;
                    case Keys.W:
                        _height = 0;
                        e.Handled = true;
                        break;
                    case Keys.S:
                        _height = 0;
                        e.Handled = true;
                        break;
                    case Keys.NumPad7:
                        _yaw = 0;
                        e.Handled = true;
                        break;
                    case Keys.NumPad9:
                        _yaw = 0;
                        e.Handled = true;
                        break;
                }
                ColorBtns();
            }
        }
    }
}
