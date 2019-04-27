using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TelloControlCenter.Controls
{
    public partial class KeyButton : UserControl
    {
        public KeyButton()
        {
            BorderColor = Color.Red;
            InitializeComponent();
        }

        public Color BorderColor { get; set; }

        public string ButtonText { get; set; }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
            e.Graphics.DrawRectangle(new Pen(BorderColor), 0, 0, Width-1, Height-1);
            var size = e.Graphics.MeasureString(ButtonText, Font);
            e.Graphics.DrawString(ButtonText, Font, new SolidBrush(ForeColor), Width / 2.0f - size.Width / 2.0f, Height / 2.0f - size.Height / 2.0f);
        }
    }
}
