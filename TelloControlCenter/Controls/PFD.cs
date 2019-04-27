using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TelloControlCenter.Controls
{
    public class PFD : UserControl
    {

        public PFD()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            BorderColor = Color.GreenYellow;
            AutoSize = false;
            DoubleBuffered = true;
            RollAngle = 0;
            PitchAngle = 15;
            YawAngle = 0;
            NoseColor = Color.Red;
            WingsColor = Color.Red;
            NoseRadius = 5;
            WingsThickness = 5;
            PitchIntervalSize = 5;
            PitchMajorIntervalWidth = 30;
            PitchMinorIntervalWidth = 5;
            PitchMajorIntervalThickness = 2;
            PitchMinorIntervalThickness = 1;
            PitchMajorIntervalColor = Color.White;
            PitchMinorIntervalColor = Color.White;
            GroundColor = Color.SaddleBrown;
            SkyColor = Color.DeepSkyBlue;
            AirSpeedUnits = "cm";
            ScaleFontBig = new Font(Font.FontFamily, Font.Size + 2, FontStyle.Bold, Font.Unit);
            ScaleFontSmall = new Font(Font.FontFamily, Font.Size - 1, FontStyle.Regular, Font.Unit);
            ScaleNormalColor = Color.White;
            AirSpeedColorWarn = Color.OrangeRed;
            AirSpeedSafetyTop = 40;
            AirSpeedSafetyBottom = -1;
            AirSpeedStep = 0.1f;
            AirSpeedFormat = "##0.#";
            Altitude = 0;
            AltitudeStep = 1;
            AltitudeUnits = "m";
            AltitudeFormat = "##0";
            VerticalSpeedFormat = "#0.##";
            VerticalSpeed = 0;
            VerticalSpeedStep = 0.1f;
            VerticalSpeedUnits = "cms";
            YawStep = 10;
            YawAngle = (float)(Math.PI / 2.0f);
            YawFormat = "#0";
            YawUnits = "°";
        }

        public string YawUnits { get; set; }
        public string VerticalSpeedUnits { get; set; }
        public string YawFormat { get; set; }
        public float VerticalSpeed { get; set; }
        public float VerticalSpeedStep { get; set; }
        public Color BorderColor { get; set; }
        public Color NoseColor { get; set; }
        public Color WingsColor { get; set; }
        public float NoseRadius { get; set; }
        public float WingsThickness { get; set; }
        public float RollAngle { get; set; }
        public float PitchAngle { get; set; }
        public float YawAngle { get; set; }
        public float YawStep { get; set; }
        public float PitchIntervalSize { get; set; }
        public float PitchMajorIntervalWidth { get; set; }
        public float PitchMinorIntervalWidth { get; set; }
        public float PitchMajorIntervalThickness { get; set; }
        public float PitchMinorIntervalThickness { get; set; }
        public Color PitchMajorIntervalColor { get; set; }
        public Color PitchMinorIntervalColor { get; set; }
        public Color GroundColor { get; set; }
        public Color SkyColor { get; set; }
        public Color ScaleNormalColor { get; set; }
        public Color AirSpeedColorWarn { get; set; }
        public float AirSpeed { get; set; }
        public string AirSpeedUnits { get; set; }
        public float Altitude { get; set; }
        public string AltitudeUnits { get; set; }
        public float AirSpeedStep { get; set; }
        public float AltitudeStep { get; set; }
        public float AirSpeedSafetyTop { get; set; }
        public float AirSpeedSafetyBottom { get; set; }
        public Font ScaleFontBig { get; set; }
        public Font ScaleFontSmall { get; set; }
        public string AirSpeedFormat { get; set; }
        public string AltitudeFormat { get; set; }
        public string VerticalSpeedFormat { get; set; }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            e.Graphics.FillRectangle(new SolidBrush(BackColor), 0, 0, Width, Height);
            e.Graphics.DrawRectangle(new Pen(BorderColor), 0, 0, Width - 1, Height - 1);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            OnPaintBackground(e);
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            e.Graphics.ScaleTransform(Width /400.0f, Height/400.0f);
            var screenRect = new RectangleF(0, 0, 400, 400);
            var middleX = screenRect.Left + screenRect.Width / 2.0f;
            var middleY = screenRect.Top + screenRect.Height / 2.0f;
            e.Graphics.TranslateTransform(middleX, middleY);
            e.Graphics.RotateTransform(RollAngle);
            e.Graphics.TranslateTransform(-middleX, -middleY);
            DrawHorizon(e.Graphics, screenRect);
            DrawPitch(e.Graphics, screenRect);
            e.Graphics.RotateTransform(-RollAngle);
            DrawDrone(e.Graphics, screenRect);
            DrawLeftScale(e.Graphics, screenRect);
            DrawRightScale(e.Graphics, screenRect);
            DrawYaw(e.Graphics, screenRect);
        }


        private void DrawYaw(Graphics g, RectangleF screenRect)
        {
            var middleX = screenRect.Left + screenRect.Width / 2.0f;
            var diameter = screenRect.Width / 2.0f;
            var normalPen = new Pen(ScaleNormalColor);
            var normalBrush = new SolidBrush(ScaleNormalColor);
            var circleBrush = new SolidBrush(Color.FromArgb(100, 100, 100));
            var ellipseCX = middleX;
            var ellipseCY = screenRect.Bottom - diameter / 2.0f + 30;
            g.FillEllipse(circleBrush, ellipseCX - diameter / 2.0f, ellipseCY, diameter, diameter);
            g.DrawEllipse(normalPen, ellipseCX - diameter / 2.0f, ellipseCY, diameter, diameter);
            var size = g.MeasureString("▽", ScaleFontBig);
            g.DrawString("▽", ScaleFontBig, normalBrush, middleX - size.Width / 2.0f + 1, screenRect.Bottom - diameter / 2.0f - size.Height / 2.0f + 30);
            var value = YawAngle + 90;
            var step = (float)(YawStep * Math.PI / 180.0f) / 2.0f;
            var major = true;
            var originalTransform = g.Transform.Clone();
            for (float alpha = -(float)Math.PI; alpha <= Math.PI; alpha += step)
            {
                var tx = (float)(ellipseCX - Math.Cos(alpha) * diameter / 2.0f);
                var ty = (float)(ellipseCY + diameter / 2.0f - Math.Sin(alpha) * diameter / 2.0f);
                var smallRad = major ? 7 : 4;
                var ttx = (float)(smallRad * Math.Cos(alpha));
                var tty = (float)(smallRad * Math.Sin(alpha));
                g.TranslateTransform(ellipseCX, ellipseCY + diameter / 2.0f);
                g.RotateTransform(value);
                g.TranslateTransform(-ellipseCX, -(ellipseCY + diameter / 2.0f));
                g.DrawLine(normalPen, tx, ty, tx + ttx, ty + tty);
                if (major)
                {
                    var degs = alpha * 180.0f / (float)Math.PI;
                    g.TranslateTransform(tx, ty);
                    g.RotateTransform(degs - 90);
                    var msg = (-degs).ToString(YawFormat);
                    var ss = g.MeasureString(msg, ScaleFontSmall);
                    g.DrawString(msg, ScaleFontSmall, normalBrush, -ss.Width / 2.0f, ss.Height / 2.0f);
                }
                g.Transform = originalTransform;
                major = !major;
            }
            g.Transform = originalTransform;
            var p = new PointF(middleX - 40, screenRect.Bottom - 30);
            g.FillRectangle(Brushes.Black, p.X, p.Y, 80, 60);
            g.DrawRectangle(normalPen, p.X, p.Y, 80, 60);
            var s = YawAngle.ToString(YawFormat) + YawUnits;
            size = g.MeasureString(s, ScaleFontBig);
            g.DrawString(s, ScaleFontBig, normalBrush, p.X + 40 - size.Width / 2.0f, p.Y + 15 - size.Height / 2.0f);
        }

        private void DrawRightScale(Graphics g, RectangleF screenRect)
        {
            var middleX = screenRect.Left + screenRect.Width / 2.0f;
            var middleY = screenRect.Top + screenRect.Height / 2.0f;
            var halfHeight = screenRect.Height / 2.0f;
            var scaleDim = new SolidBrush(Color.FromArgb(100, Color.Gray));
            var borderPen = new Pen(Color.FromArgb(200, Color.Gray));
            var rightScaleLeft = middleX + WingsThickness * 20.0f + 15;
            using (var p = new PointF(rightScaleLeft + 5, middleY)
                .StartDrawing()
                .MoveRel(0, -15)
                .MoveRel(7, 0)
                .MoveRel(0, -halfHeight + 30)
                .MoveRel(32, 0)
                .MoveRel(0, screenRect.Height - 30)
                .MoveRel(-32, 0)
                .MoveRel(0, -halfHeight + 30)
                .MoveRel(-7, 0)
                .MoveRel(0, -15)
                .ConvertToPath())
            {
                p.CloseFigure();
                g.FillPath(scaleDim, p);
                g.DrawPath(borderPen, p);
            }

            using (var p = new PointF(rightScaleLeft + 44, middleY - halfHeight + halfHeight / 5.0f)
                .StartDrawing()
                .MoveRel(30, 0)
                .MoveRel(0, (halfHeight - halfHeight / 5.0f) * 2.0f)
                .MoveRel(-30, 0)
                .ConvertToPath())
            {
                p.CloseFigure();
                g.FillPath(scaleDim, p);
                g.DrawPath(borderPen, p);
            }

            var normalPen = new Pen(ScaleNormalColor);

            DrawVerticalSpeedScale(rightScaleLeft, screenRect, middleY, g);
            DrawScaleTextArea(g, middleY, rightScaleLeft);
            DrawAltitudeScale(rightScaleLeft, screenRect, middleY, g);
            g.DrawRectangle(normalPen, rightScaleLeft + 12, screenRect.Bottom - 33, 32, 18);
            g.FillRectangle(Brushes.Black, rightScaleLeft + 12, screenRect.Bottom - 33, 32, 18);
            var size = g.MeasureString(AltitudeUnits, ScaleFontSmall);
            var normalBursh = new SolidBrush(ScaleNormalColor);
            g.DrawString(AltitudeUnits, ScaleFontSmall, normalBursh, rightScaleLeft + 28 - size.Width / 2.0f, screenRect.Bottom - 33);
            g.DrawRectangle(normalPen, rightScaleLeft + 45, screenRect.Bottom - 51, 29, 18);
            g.FillRectangle(Brushes.Black, rightScaleLeft + 45, screenRect.Bottom - 51, 29, 18);
            size = g.MeasureString(VerticalSpeedUnits, ScaleFontSmall);
            g.DrawString(VerticalSpeedUnits, ScaleFontSmall, normalBursh, rightScaleLeft + 60 - size.Width / 2.0f, screenRect.Bottom - 42 - size.Height / 2.0f);
            g.FillRectangle(Brushes.Black, rightScaleLeft + 12, screenRect.Top + 13, 32, 18);
            size = g.MeasureString("ALT", ScaleFontSmall);
            g.DrawString("ALT", ScaleFontSmall, normalBursh, rightScaleLeft + 29 - size.Width / 2.0f, screenRect.Top + 22 - size.Height / 2.0f);
            g.FillRectangle(Brushes.Black, rightScaleLeft + 44, screenRect.Top + 30, 30, 18);
            size = g.MeasureString("VSI", ScaleFontSmall);
            g.DrawString("VSI", ScaleFontSmall, normalBursh, rightScaleLeft + 59 - size.Width / 2.0f, screenRect.Top + 38 - size.Height / 2.0f);
        }

        private void DrawVerticalSpeedScale(float rightScaleLeft, RectangleF screenRect, float middleY, Graphics g)
        {
            var middleText = rightScaleLeft + 65;
            var normalBrush = new SolidBrush(ScaleNormalColor);
            var counter = 0;
            var normalPen = new Pen(ScaleNormalColor);
            for (float up = middleY, down = middleY; up > screenRect.Top + 40; up -= 35, down += 35, counter++)
            {
                var valUp = VerticalSpeed + counter * VerticalSpeedStep;
                var valDown = VerticalSpeed - counter * VerticalSpeedStep;
                var sUp = valUp.ToString(VerticalSpeedFormat);
                var sDown = valDown.ToString(VerticalSpeedFormat);
                var sizeUp = g.MeasureString(sUp, ScaleFontSmall);
                var sizeDown = g.MeasureString(sDown, ScaleFontSmall);
                g.DrawString(sUp, ScaleFontSmall, normalBrush, middleText - sizeUp.Width / 2.0f, up - sizeUp.Height / 2.0f);
                g.DrawString(sDown, ScaleFontSmall, normalBrush, middleText - sizeDown.Width / 2.0f, down - sizeDown.Height / 2.0f);
                g.DrawLine(normalPen, middleText - 20, up, middleText - 15, up);
                g.DrawLine(normalPen, middleText - 20, down, middleText - 15, down);
            }
        }

        private void DrawAltitudeScale(float rightScaleLeft, RectangleF screenRect, float middleY, Graphics g)
        {
            var middleText = rightScaleLeft + 27;
            var value = Altitude.ToString(AltitudeFormat);
            var size = g.MeasureString(value, ScaleFontBig);
            var normalBrush = new SolidBrush(ScaleNormalColor);
            g.DrawString(value, ScaleFontBig, normalBrush, middleText - size.Width / 2.0f, middleY - size.Height / 2.0f);
            var counter = 1;
            var normalPen = new Pen(ScaleNormalColor);
            for (float up = middleY - 30, down = middleY + 30; up > screenRect.Top + 20; up -= 25, down += 25, counter++)
            {
                var valUp = Altitude + counter * AltitudeStep;
                var valDown = Altitude - counter * AltitudeStep;
                var sUp = valUp.ToString(AltitudeFormat);
                var sDown = valDown.ToString(AltitudeFormat);
                var sizeUp = g.MeasureString(sUp, ScaleFontSmall);
                var sizeDown = g.MeasureString(sDown, ScaleFontSmall);
                g.DrawString(sUp, ScaleFontSmall, normalBrush, middleText - sizeUp.Width / 2.0f + 3, up - sizeUp.Height / 2.0f);
                g.DrawString(sDown, ScaleFontSmall, normalBrush, middleText - sizeDown.Width / 2.0f + 3, down - sizeDown.Height / 2.0f);
                g.DrawLine(normalPen, rightScaleLeft + 12, up, rightScaleLeft + 15, up);
                g.DrawLine(normalPen, rightScaleLeft + 12, down, rightScaleLeft + 15, down);
            }
        }

        private void DrawScaleTextArea(Graphics g, float middleY, float rightScaleLeft)
        {
            using (var p = new PointF(rightScaleLeft, middleY)
                            .StartDrawing()
                            .MoveRel(5, -5)
                            .MoveRel(0, -10)
                            .MoveRel(45, 0)
                            .MoveRel(0, 10)
                            .MoveRel(5, 5)
                            .MoveRel(-5, 5)
                            .MoveRel(0, 10)
                            .MoveRel(-45, 0)
                            .MoveRel(0, -10)
                            .MoveRel(-5, -5)
                            .ConvertToPath())
            {
                p.CloseFigure();
                g.DrawPath(new Pen(ScaleNormalColor), p);
            }
            using (var p = new PointF(rightScaleLeft + 1, middleY)
                 .StartDrawing()
                 .MoveRel(5, -5)
                 .MoveRel(0, -9)
                 .MoveRel(43, 0)
                 .MoveRel(0, 9)
                 .MoveRel(5, 5)
                 .MoveRel(-5, 5)
                 .MoveRel(0, 9)
                 .MoveRel(-43, 0)
                 .MoveRel(0, -9)
                 .MoveRel(-5, -5)
                 .ConvertToPath())
            {
                p.CloseFigure();
                g.FillPath(Brushes.Black, p);
            }
        }

        private void DrawLeftScale(Graphics g, RectangleF screenRect)
        {
            var middleX = screenRect.Left + screenRect.Width / 2.0f;
            var middleY = screenRect.Top + screenRect.Height / 2.0f;
            var halfHeight = screenRect.Height / 2.0f;
            var leftScaleRight = middleX - WingsThickness * 20.0f - 15;
            var scaleDim = new SolidBrush(Color.FromArgb(100, Color.Gray));
            var borderPen = new Pen(Color.FromArgb(200, Color.Gray));
            using (var p = new PointF(leftScaleRight, middleY)
                .StartDrawing()
                .MoveRel(0, -15)
                .MoveRel(-12, 0)
                .MoveRel(0, -halfHeight + 30)
                .MoveRel(-30, 0)
                .MoveRel(0, halfHeight - 30)
                .MoveRel(-12, 0)
                .MoveRel(0, 30)
                .MoveRel(12, 0)
                .MoveRel(0, halfHeight - 30)
                .MoveRel(30, 0)
                .MoveRel(0, -halfHeight + 30)
                .MoveRel(12, 0)
                .MoveRel(0, -15)
                .ConvertToPath())
            {
                p.CloseFigure();
                g.FillPath(scaleDim, p);
                g.DrawPath(borderPen, p);
            }
            var middleText = leftScaleRight - 27;
            var value = AirSpeed.ToString(AirSpeedFormat);
            var airSpeedColor = (AirSpeed > AirSpeedSafetyTop || AirSpeed < AirSpeedSafetyBottom) ? AirSpeedColorWarn : ScaleNormalColor;
            using (var p = new PointF(leftScaleRight + 5, middleY)
                .StartDrawing()
                .MoveRel(-5, -5)
                .MoveRel(0, -10)
                .MoveRel(-54, 0)
                .MoveRel(0, 30)
                .MoveRel(54, 0)
                .MoveRel(0, -10)
                .MoveRel(5, -5)
                .ConvertToPath())
            {
                p.CloseFigure();
                g.DrawPath(new Pen(airSpeedColor), p);
            }

            var counter = 1;
            var normalBrush = new SolidBrush(ScaleNormalColor);
            var safetyBad = new HatchBrush(HatchStyle.WideUpwardDiagonal, Color.Orange, Color.Black);
            var safetyGood = new SolidBrush(Color.Green);
            var normalPen = new Pen(ScaleNormalColor);
            var middleOfScaleLeft = leftScaleRight - 12;
            for (float up = middleY - 30, down = middleY + 30; up > screenRect.Top + 20; up -= 25, down += 25, counter++)
            {
                var valUp = AirSpeed + counter * AirSpeedStep;
                var valDown = AirSpeed - counter * AirSpeedStep;
                var sUp = valUp.ToString(AirSpeedFormat);
                var sDown = valDown.ToString(AirSpeedFormat);
                var sizeUp = g.MeasureString(sUp, ScaleFontSmall);
                var sizeDown = g.MeasureString(sDown, ScaleFontSmall);
                g.DrawString(sUp, ScaleFontSmall, normalBrush, middleText - sizeUp.Width / 2.0f, up - sizeUp.Height / 2.0f);
                g.DrawString(sDown, ScaleFontSmall, normalBrush, middleText - sizeDown.Width / 2.0f, down - sizeDown.Height / 2.0f);
                g.DrawLine(normalPen, middleOfScaleLeft, up, middleOfScaleLeft - 3, up);
                g.DrawLine(normalPen, middleOfScaleLeft, down, middleOfScaleLeft - 3, down);
                var bUp = (valUp > AirSpeedSafetyTop || valUp < AirSpeedSafetyBottom) ? (Brush)safetyBad : safetyGood;
                var bDown = (valDown > AirSpeedSafetyTop || valDown < AirSpeedSafetyBottom) ? (Brush)safetyBad : safetyGood;
                g.FillRectangle(bUp, middleOfScaleLeft, up - 15, 5, 30);
                g.FillRectangle(bDown, middleOfScaleLeft, down - 15, 5, 30);
            }

            g.FillRectangle(Brushes.Black, middleOfScaleLeft - 29, middleY + halfHeight - 35, 29, 19);
            g.FillRectangle(Brushes.Black, middleOfScaleLeft - 29, middleY - halfHeight + 15, 29, 19);
            var sizeU = g.MeasureString(AirSpeedUnits, ScaleFontSmall);
            g.DrawString(AirSpeedUnits, ScaleFontSmall, Brushes.White, middleText - sizeU.Width / 2.0f, middleY + halfHeight - 27 - sizeU.Height / 2.0f);
            sizeU = g.MeasureString("TAS", ScaleFontSmall);
            g.DrawString("TAS", ScaleFontSmall, Brushes.White, middleText - sizeU.Width / 2.0f, middleY - halfHeight + 24 - sizeU.Height / 2.0f);

            var size = g.MeasureString(value, ScaleFontBig);
            using (var p = new PointF(leftScaleRight + 4, middleY)
                .StartDrawing()
                .MoveRel(-5, -5)
                .MoveRel(0, -9)
                .MoveRel(-52, 0)
                .MoveRel(0, 29)
                .MoveRel(52, 0)
                .MoveRel(0, -9)
                .MoveRel(4, -4)
                .ConvertToPath())
            {
                p.CloseFigure();
                g.FillPath(Brushes.Black, p);
            }

            g.DrawString(value, ScaleFontBig, new SolidBrush(airSpeedColor), middleText - size.Width / 2.0f, middleY - size.Height / 2.0f);
        }

        private void DrawHorizon(Graphics graphics, RectangleF screenRect)
        {
            var pixelsPerPitchDegree = screenRect.Height / 90.0f;
            var pixelsToAdd = pixelsPerPitchDegree * (-PitchAngle);
            var middleX = screenRect.Left + screenRect.Width / 2.0f;
            var middleY = screenRect.Top + screenRect.Height / 2.0f;
            var rectTop = new RectangleF(middleX - Width * 2.0f, middleY - Height * 2 + pixelsToAdd, Width * 4, Height * 2);
            var rectBot = new RectangleF(middleX - Width * 2.0f, middleY + pixelsToAdd, Width * 4, Height * 2);
            graphics.FillRectangle(new SolidBrush(SkyColor), rectTop);
            graphics.FillRectangle(new SolidBrush(GroundColor), rectBot);
        }

        private void DrawDrone(Graphics g, RectangleF screenRect)
        {
            var middleX = screenRect.Left + screenRect.Width / 2.0f;
            var middleY = screenRect.Top + screenRect.Height / 2.0f;
            g.FillEllipse(new SolidBrush(NoseColor), middleX - NoseRadius, middleY - NoseRadius, NoseRadius * 2.0f, NoseRadius * 2.0f);
            var wingsBursh = new SolidBrush(WingsColor);
            g.FillRectangle(wingsBursh, middleX - WingsThickness * 12.0f, middleY - WingsThickness, WingsThickness, WingsThickness * 2.0f);
            g.FillRectangle(wingsBursh, middleX + WingsThickness * 11.0f, middleY - WingsThickness, WingsThickness, WingsThickness * 2.0f);
            g.FillRectangle(wingsBursh, middleX - WingsThickness * 20.0f, middleY - WingsThickness, WingsThickness * 9.0f, WingsThickness);
            g.FillRectangle(wingsBursh, middleX + WingsThickness * 12.0f - 1, middleY - WingsThickness, WingsThickness * 9.0f, WingsThickness);
        }

        private void DrawPitch(Graphics g, RectangleF screenRect)
        {
            var pixelsPerPitchDegree = screenRect.Height / 25.0f;
            var middleY = screenRect.Top + screenRect.Height / 2.0f;
            var middleX = screenRect.Left + screenRect.Width / 2.0f;

            var startCoord = (-90 + PitchAngle) * pixelsPerPitchDegree + middleY;
            var endCoord = (90 + PitchAngle) * pixelsPerPitchDegree + middleY;

            var major = true;
            var pitchStep = PitchIntervalSize / 2.0f;
            var majorPen = new Pen(PitchMajorIntervalColor, PitchMajorIntervalThickness);
            var minorPen = new Pen(PitchMinorIntervalColor, PitchMinorIntervalThickness);
            var halfMinorSize = PitchMinorIntervalWidth / 2.0f;
            var halfMajorSize = PitchMajorIntervalWidth / 2.0f;
            var textBrush = new SolidBrush(PitchMajorIntervalColor);
            float w;
            for (float coord = startCoord, value = -90.0f;
                coord <= endCoord;
                coord += pitchStep * pixelsPerPitchDegree, value += pitchStep, major = !major)
            {
                w = major ? halfMajorSize : halfMinorSize;
                g.DrawLine(major ? majorPen : minorPen, middleX - w, coord, middleX + w, coord);
                if (major)
                {
                    var val = Math.Abs(value).ToString("0.###");
                    var sign = value == 0 ? "" : (value < 0 ? "▽" : "△");
                    var msgLeft = $"{val} {sign}";
                    var msgRight = $"{sign} {val}";
                    var sizeLeft = g.MeasureString(msgLeft, Font);
                    var sizeRight = g.MeasureString(msgRight, Font);
                    g.DrawString(msgLeft, Font, textBrush, middleX - w - sizeLeft.Width - 2, coord - sizeLeft.Height / 2.0f);
                    g.DrawString(msgRight, Font, textBrush, middleX + w + 2, coord - sizeRight.Height / 2.0f);
                }
            }

        }
    }
}
