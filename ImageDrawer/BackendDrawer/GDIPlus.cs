using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace ImageDrawer
{
	public class GDIPlus : BackendDrawerBase
	{
		private Graphics graphics;
		private Pen pen;
		private Brush brush;
		private List<Color> debugFillColors;
		private List<Color> debugStrokeColors;
		private int debugFillColorsIndex;
		private int debugStrokeColorsIndex;
		public override void Construct(Bitmap newBitmap, Bitmap origBitmap, RenderParams param)
		{
			base.Construct(newBitmap, origBitmap, param);
			graphics = Graphics.FromImage(newBitmap);
			brush = new SolidBrush(Color.Black);
			Random random = new Random(1337);
			debugFillColors = new List<Color>(30);
			debugStrokeColors = new List<Color> { Color.Red, Color.Blue, Color.Magenta, Color.Black, Color.LimeGreen };
			for (int i = 0; i < debugFillColors.Capacity; i++)
				debugFillColors.Add(Color.FromArgb(random.Next(200, 256), random.Next(200, 256), random.Next(200, 256)));
			pen = new Pen(brush, param.Stroke);
			graphics.SmoothingMode = param.Smoothing == SmoothingType.Antialias ?
				SmoothingMode.AntiAlias : SmoothingMode.None;
		}

		protected override void DrawBezier(Point[] coords)
		{
			Point[] fin = new Point[(coords.Length - 1) / 3 * 3 + 1];
			for (int i = 0; i < fin.Length; i++)
				fin[i] = coords[i];
			graphics.DrawBeziers(pen, fin);
		}

		protected override void DrawCurve(Point[] coords)
		{
			if (param.FillInside)
			{
				graphics.FillClosedCurve(new SolidBrush(GetColor(true)), GetFillCoordinates(coords));
			}

			if (param.Debug)
				pen.Color = GetColor(false);
			graphics.DrawCurve(pen, coords); // TODO: implement tension to manual too
		}

		private Color GetColor(bool isFill)
		{
			if (isFill)
			{
				if (param.Debug)
					return debugFillColors[debugFillColorsIndex + 1 < debugFillColors.Count ? ++debugFillColorsIndex : debugFillColorsIndex = 0];
				else
					return Color.White;
			}
			else
			{
				if (param.Debug)
					return debugStrokeColors[debugStrokeColorsIndex + 1 < debugStrokeColors.Count ? ++debugStrokeColorsIndex : debugStrokeColorsIndex = 0];
				else
					return Color.Black;
			}
			
		}

		protected override void DrawDots(Point[] coords)
		{
			foreach (Point coord in coords)
				graphics.FillRectangle(brush, coord.X, coord.Y, param.Stroke, param.Stroke);
		}

		private Point[] GetFillCoordinates(Point[] coords)
		{
			int addition = 5; // in Curves mode there are pits always before the peak, thus have to compensate this
			int maxY = coords.Max(p => p.Y);
			List<Point> fillCoords = new List<Point>();
			fillCoords.Add(new Point(coords[0].X, coords[0].Y + (maxY - coords[0].Y) + addition));
			fillCoords.AddRange(coords);
			fillCoords.Add(new Point(coords[coords.Length - 1].X, coords[coords.Length - 1].Y + (maxY - coords[coords.Length - 1].Y) + addition));
			return fillCoords.ToArray();
		}

		protected override void DrawLines(Point[] coords)
		{
			if (param.FillInside)
			{
				graphics.FillPolygon(new SolidBrush(GetColor(true)), GetFillCoordinates(coords));
			}

			if (param.Debug)
				pen.Color = GetColor(false);
			graphics.DrawLines(pen, coords);
			//for (int i = 0; i < coords.Length - 1; i++) // TODO: there are visible breaks if use this way with antialiasing
			//{
			//	pen.Color = colors[i % 3];
			//	Point a = coords[i];
			//	Point b = coords[i + 1];
			//	graphics.DrawLine(pen, a, b);
			//}
		}

		protected override void DrawVariableLines(Point[] coords, int y)
		{
			for (int i = 0; i < coords.Length - 1; i++)
			{
				var coord1 = coords[i];
				var coord2 = coords[i + 1];
				graphics.FillVariableLine(brush, coord1.X, coord1.Y, coord2.X, coord2.Y, Math.Abs(y - coord1.Y), Math.Abs(y - coord2.Y));
			}
		}
	}

	public static class GraphicsExtension
	{
		public static Graphics FillVariableLine(this Graphics g, Brush b, int x1, int y1, int x2, int y2, int w1, int w2)
		{
			var c1 = new { r = (float)w1 / 2, x = x1, y = y1 };
			var c2 = new { r = (float)w2 / 2, x = x2, y = y2 };

			var x = Math.Abs(c1.x - c2.x);
			var y = Math.Abs(c1.y - c2.y);
			var l = (float)Math.Sqrt((x * x) + (y * y));

			var v = new { x = c2.x - c1.x, y = c2.y - c1.y };
			var uv = new { x = v.x / l, y = v.y / l };

			var ca = (c2.r - c1.r) / l;
			var sa = (float)Math.Sqrt(1 - (ca * ca));

			PointF[] points = new PointF[] {
				new PointF(c1.x - (ca * c1.r * uv.x) - (sa * c1.r * uv.y),
						   c1.y + (sa * c1.r * uv.x) - (ca * c1.r * uv.y)),
				new PointF(c1.x - (ca * c1.r * uv.x) + (sa * c1.r * uv.y),
						   c1.y - (sa * c1.r * uv.x) - (ca * c1.r * uv.y)),

				new PointF(c2.x - (ca * c2.r * uv.x) + (sa * c2.r * uv.y),
						   c2.y - (sa * c2.r * uv.x) - (ca * c2.r * uv.y)),
				new PointF(c2.x - (ca * c2.r * uv.x) - (sa * c2.r * uv.y),
						   c2.y + (sa * c2.r * uv.x) - (ca * c2.r * uv.y)),
			};

			g.FillEllipse(b, new Rectangle(x1 - w1 / 2, y1 - w1 / 2, w1, w1));
			g.FillEllipse(b, new Rectangle(x2 - w2 / 2, y2 - w2 / 2, w2, w2));
			g.FillPolygon(b, points);
			return g;
		}
	}

	public static class ColorExtension
	{
		public static int Greyscale(this Color c)
		{
			return (c.R + c.G + c.B) / 3;
		}
	}
}
