using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace TelloControlCenter.Controls
{
    public static class Extensions
    {
        public static void DrawRectangleRounded(this Graphics graph, Pen borderPen, float x, float y, float width, float height, float radius)
        {
            graph.DrawRectangleRounded(borderPen, new RectangleF
            {
                X = x,
                Y = y,
                Width = width,
                Height = height
            }, radius);
        }

        public static void DrawRectangleRounded(this Graphics graph, Pen borderPen, RectangleF rect, float radius)
        {
            var path = GetRoundedRectPath(rect, radius);
            graph.DrawPath(borderPen, path);
        }

        public static void FillRectangleRounded(this Graphics graphics, Brush brush, RectangleF rect, float radius)
        {
            var path = GetRoundedRectPath(rect, radius);
            graphics.FillPath(brush, path);
        }

        public static void FillRectangleRounded(this Graphics graphics, Brush brush, float x, float y, float width, float height, float radius)
        {
            graphics.FillRectangleRounded(brush, new RectangleF
            {
                X = x,
                Y = y,
                Width = width,
                Height = height
            }, radius);
        }

        public static Drawer StartDrawing(this PointF from)
        {
            return new Drawer(from.X, from.Y);
        }

        public static Drawer MoveTo(this Drawer d, float x, float y)
        {
            d.Points.Add(new PointF(x, y));
            d.CursorX = x;
            d.CursorY = y;
            return d;
        }
        
        public static Drawer MoveRel(this Drawer d, float dx, float dy)
        {
            d.Points.Add(new PointF(d.CursorX + dx, d.CursorY + dy));
            d.CursorX += dx;
            d.CursorY += dy;
            return d;
        }

        public static GraphicsPath ConvertToPath(this Drawer d)
        {
            var tt = new byte[d.Points.Count];
            tt[0] = (byte)PathPointType.Start;
            for(var i=1;i<tt.Length;i++)
            {
                tt[i] = (byte)PathPointType.Line;
            }
            return new GraphicsPath(d.Points.ToArray(), tt);
        }

        public static GraphicsPath GetRoundedRectPath(this RectangleF rect, float radius)
        {
            float diameter = radius * 2;
            var size = new SizeF(diameter, diameter);
            var arc = new RectangleF(rect.Location, size);
            var path = new GraphicsPath();

            if (radius == 0)
            {
                path.AddRectangle(rect);
            }
            else
            {
                // top left arc  
                path.AddArc(arc, 180, 90);

                // top right arc  
                arc.X = rect.Right - diameter;
                path.AddArc(arc, 270, 90);

                // bottom right arc  
                arc.Y = rect.Bottom - diameter;
                path.AddArc(arc, 0, 90);

                // bottom left arc 
                arc.X = rect.Left;
                path.AddArc(arc, 90, 90);

                path.CloseFigure();
            }

            return path;
        }
    }
}
