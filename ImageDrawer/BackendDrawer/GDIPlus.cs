﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace RidgeDrawer
{
	public class GDIPlus : BackendDrawerBase
	{
		private Graphics graphics;
		private Pen pen;
		private Brush brush;
		private IEnumerator debugFillColorsEnumerator;
		private IEnumerator debugStrokeColorsEnumerator;
		public override void Construct(Bitmap newBitmap, Bitmap origBitmap, RenderParams param)
		{
			base.Construct(newBitmap, origBitmap, param);
			graphics = Graphics.FromImage(newBitmap);
			brush = new SolidBrush(Color.Black);
			Random random = new Random(1337);
			debugStrokeColorsEnumerator = new List<Color> { Color.Blue, Color.Magenta, Color.Black, Color.LimeGreen }.GetEnumerator();
			List<Color> debugFillColors = new List<Color>(30);
			for (int i = 0; i < debugFillColors.Capacity; i++)
				debugFillColors.Add(Color.FromArgb(random.Next(200, 256), random.Next(200, 256), random.Next(200, 256)));
			debugFillColorsEnumerator = debugFillColors.GetEnumerator();
			pen = new Pen(brush, param.Stroke);
			graphics.SmoothingMode = param.Smoothing == SmoothingType.Antialias ?
				SmoothingMode.AntiAlias : SmoothingMode.None;
		}

		protected override void DrawBezier(MyPoint[] coords)
		{
			MyPoint[] fin = new MyPoint[(coords.Length - 1) / 3 * 3 + 1];
			for (int i = 0; i < fin.Length; i++)
				fin[i] = coords[i];
			graphics.DrawBeziers(pen, MyPoint.ToPoint(fin));
		}

		protected override void DrawCurve(MyPoint[] coords)
		{
			if (param.FillInside)
			{
				graphics.FillClosedCurve(new SolidBrush(GetColor(true)), MyPoint.ToPoint(GetFillCoordinates(coords)));
			}

			pen.Color = GetColor(false);
			graphics.DrawCurve(pen, MyPoint.ToPoint(coords), .5f); // TODO: implement tension to manual too
		}

		private Color GetColor(bool isFill)
		{
			if (param.Debug)
			{
				IEnumerator enumerator = isFill ? debugFillColorsEnumerator : debugStrokeColorsEnumerator;
				if (!enumerator.MoveNext())
				{
					enumerator.Reset();
					enumerator.MoveNext();
				}

				return (Color)enumerator.Current;
			}

			return isFill ? Color.White : Color.Black;
		}

		protected override void DrawDots(MyPoint[] coords)
		{
			int strokeAmount = param.Stroke;
			if (param.Debug && param.LineType != LineType.Dot)
			{
				brush = new SolidBrush(Color.Red);
				strokeAmount = 2;
			}
			foreach (MyPoint coord in coords)
				graphics.FillRectangle(brush, coord.X, coord.Y, strokeAmount, strokeAmount);
		}

		private MyPoint[] GetFillCoordinates(MyPoint[] coords)
		{
			List<MyPoint> fillCoords = new List<MyPoint>();
			fillCoords.Add(new MyPoint(coords[0].X, newBitmap.Height));
			fillCoords.AddRange(coords);
			fillCoords.Add(new MyPoint(coords[coords.Length - 1].X, newBitmap.Height));
			return fillCoords.ToArray();
		}

		protected override void DrawLines(MyPoint[] coords)
		{
			if (param.FillInside)
			{
				graphics.FillPolygon(new SolidBrush(GetColor(true)), MyPoint.ToPoint(GetFillCoordinates(coords)));
			}

			if (param.Debug)
				pen.Color = GetColor(false);
			graphics.DrawLines(pen, MyPoint.ToPoint(coords));
			if (param.Debug)
				DrawDots(coords);
			//for (int i = 0; i < coords.Length - 1; i++) // TODO: there are visible breaks if use this way with antialiasing
			//{
			//	pen.Color = colors[i % 3];
			//	MyPoint a = coords[i];
			//	MyPoint b = coords[i + 1];
			//	graphics.DrawLine(pen, a, b);
			//}
		}

		protected override void DrawVariableLines(MyPoint[] coords, int y)
		{
			for (int i = 0; i < coords.Length - 1; i++)
			{
				var coord1 = coords[i];
				var coord2 = coords[i + 1];
				graphics.FillVariableLine(brush, coord1.X, coord1.Y, coord2.X, coord2.Y, Math.Abs(y - coord1.Y), Math.Abs(y - coord2.Y));
			}
		}

		protected override void DrawDebugInfo()
		{
			brush = new SolidBrush(Color.Orange);
			int width = 20;
			graphics.FillRectangle(brush, param.PullPointX - 1, param.PullPointY - width / 2, 2, width);
			graphics.FillRectangle(brush, param.PullPointX - width / 2, param.PullPointY - 1, width, 2);
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
