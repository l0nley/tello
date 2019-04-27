using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TelloControlCenter.Controls
{
    public class LinearControl : UserControl
    {
        private Brush _verticalIndicator;
        private Brush _horizontalRight;
        private Brush _horizontalLeft;

        public LinearControl()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);

            DoubleBuffered = true;
            TickColor = Pens.YellowGreen;
            TickLabelColor = Brushes.YellowGreen;
            BorderColor = Pens.DarkKhaki;
            _verticalIndicator = new LinearGradientBrush(
                  new Point(0, 0),
                  new Point(0, Height),
                  Color.Red,
                  Color.Green);
            _horizontalLeft = new LinearGradientBrush(
                new PointF(47, 0),
                new PointF(Width / 2.0f + 17, 0),
                Color.Red,
                Color.Green);
            _horizontalRight = new LinearGradientBrush(
                new PointF(Width / 2.0f + 17, 0),
                new PointF(Width, 0),
                Color.Green,
                Color.Red);
        }

        public float HeightAxis { get; set; }
        public float PitchAxis { get; set; }
        public float YawAxis { get; set; }
        public float RollAxis { get; set; }
        public Pen TickColor { get; set; }
        public Brush TickLabelColor { get; set; }
        public Pen BorderColor { get; set; }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            _verticalIndicator = new LinearGradientBrush(
                  new Point(0, 0),
                  new Point(0, Height),
                  Color.Red,
                  Color.Green);
            _horizontalLeft = new LinearGradientBrush(
               new PointF(47, 0),
               new PointF(Width / 2.0f + 17, 0),
               Color.Red,
               Color.Green);
            _horizontalRight = new LinearGradientBrush(
                new PointF(Width / 2.0f + 17, 0),
                new PointF(Width, 0),
                Color.Green,
                Color.Red);

        }

        protected override void OnPaint(PaintEventArgs e)
        {
            OnPaintBackground(e);
            DrawHeight(e);
            DrawYaw(e);
            DrawRoll(e);
            DrawPitch(e);
        }

        private void DrawPitch(PaintEventArgs e)
        {
            var top = 40;
            var height = Height - top - 40;
            var realHeight = height * PitchAxis;
            var left = Width / 2.0f + 10;
            e.Graphics.DrawRectangle(BorderColor, left, top, 11, height+1);
            e.Graphics.FillRectangle(_verticalIndicator, left, top + height - realHeight, 10, realHeight);
            var heightStep = height / 5.0f;
            var val = -100;
            for (float i = 0; i <= height; i += heightStep)
            {
                e.Graphics.DrawLine(TickColor, left + 11, top+i, left + 16,top+ i);
                if (val != 0)
                {
                    e.Graphics.DrawString(val.ToString(), Font, TickLabelColor, left + 17, top + i);
                }
                val += 40;
            }
            e.Graphics.DrawString($"P={RollAxis}", Font, TickLabelColor, 240, Height - 20);
        }

        private void DrawRoll(PaintEventArgs e)
        {
            var top = Height / 2.0f - 10;
            e.Graphics.DrawRectangle(BorderColor, 47, top, Width - 60, 11);
            var rollWidth = Width - 60;
            var realWidth = (RollAxis - 0.5f) * (rollWidth);
            e.Graphics.FillPolygon(RollAxis > 0.5 ? _horizontalRight : _horizontalLeft, new PointF[] {
                new PointF(47 + rollWidth / 2.0f, top),
                new PointF(47 + rollWidth / 2.0f, top+10),
                new PointF(47 + rollWidth /2.0f + realWidth, top+10),
                new PointF(47 + rollWidth /2.0f + realWidth, top)
            });
            var rollStep = rollWidth / 10f;
            var val = -100;
            for (float i = 47; i <= rollWidth + 48; i += rollStep)
            {
                e.Graphics.DrawLine(TickColor, i, top + 11, i, top + 16);
                var msg = val.ToString();
                var size = e.Graphics.MeasureString(msg, Font);
                if (val != 0)
                {
                    e.Graphics.DrawString(msg, Font, TickLabelColor, i - size.Width / 2.0f, top + 16);
                }
                val += 20;
            }

            e.Graphics.DrawString($"R={YawAxis}", Font, TickLabelColor, 160, Height - 20);
        }

        private void DrawYaw(PaintEventArgs e)
        {
            e.Graphics.DrawRectangle(BorderColor, 47, 0, Width - 60, 11);
            var yawWidth = Width - 60;
            var realWidth = (YawAxis - 0.5f) * (yawWidth);
            e.Graphics.FillPolygon(YawAxis > 0.5 ? _horizontalRight : _horizontalLeft, new PointF[] {
                new PointF(47 + yawWidth / 2.0f, 1),
                new PointF(47 + yawWidth / 2.0f, 11),
                new PointF(47 + yawWidth /2.0f + realWidth, 11),
                new PointF(47 + yawWidth /2.0f + realWidth, 1)
            });
            var stepYaw = yawWidth / 10f;
            var val = -100;
            for (float i = 47; i <= yawWidth + 48; i += stepYaw)
            {
                e.Graphics.DrawLine(TickColor, i, 11, i, 16);
                var msg = val.ToString();
                var size = e.Graphics.MeasureString(msg, Font);
                e.Graphics.DrawString(msg, Font, TickLabelColor, i - size.Width / 2.0f, 16);
                val += 20;
            }

            e.Graphics.DrawString($"Y={YawAxis}", Font, TickLabelColor, 80, Height - 20);
        }

        private void DrawHeight(PaintEventArgs e)
        {
            var height = Height - 40;
            var topValue = Height * HeightAxis;
            e.Graphics.FillRectangle(_verticalIndicator, 1, topValue, 11, height - topValue - 1);
            e.Graphics.DrawRectangle(BorderColor, 0, 0, 11, height - 1);
            var heightStep = height / 5.0f;
            var val = 100;
            for (float i = 0; i <= height; i += heightStep)
            {
                e.Graphics.DrawLine(TickColor, 11, i, 16, i);
                e.Graphics.DrawString(val.ToString(), Font, TickLabelColor, 17, i);
                val -= 20;
            }
            e.Graphics.DrawString($"H={1 - HeightAxis}", Font, TickLabelColor, 0, Height - 20);
        }
    }
}
