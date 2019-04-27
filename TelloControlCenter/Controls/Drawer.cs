using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace TelloControlCenter.Controls
{
    public class Drawer
    {
        public Drawer(float startX, float startY)
        {
            Points = new List<PointF>
            {
                new PointF(startX, startY)
            };
            CursorX = startX;
            CursorY = startY;
        }
        public List<PointF> Points { get; }
        public float CursorX { get; set; }
        public float CursorY { get; set; }
    }
}
