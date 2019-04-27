using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TelloControlCenter.Controls
{
    class ToggleCheckBox : CheckBox
    {
        public ToggleCheckBox()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            Padding = new Padding(5);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            OnPaintBackground(e);
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            
            using (var path = new GraphicsPath())
            {
                var d = Padding.All;
                var r = Height - 2 * d;
                path.AddArc(d, d, r, r, 90, 180);
                path.AddArc(Width - r - d, d, r, r, -90, 180);
                path.CloseFigure();
                e.Graphics.FillPath(Checked ? Brushes.DarkGray : Brushes.LightGray, path);
                r = Height - 1;
                var rect = Checked ? new Rectangle(Width - r - 1, 0, r, r)
                                   : new Rectangle(0, 0, r, r);
                e.Graphics.FillEllipse(Checked ? Brushes.Red : Brushes.WhiteSmoke, rect);
                var msg = Checked ? "ON" : "OFF";
                var len = e.Graphics.MeasureString(msg, Font);
                e.Graphics.DrawString(msg, Font, new SolidBrush(ForeColor), Width / 2.0f - len.Width / 2.0f, Height / 2.0f - len.Height / 2.0f);
            }

        }


    }
}
