using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace ImageDrawer
{
	//public static class GraphicsExtension
	//{
	//	public static Graphics FillVariableLine(this Graphics g, Brush b, int x1, int y1, int x2, int y2, int w1, int w2)
	//	{
	//		int X = Math.Abs(x1 - x2);
	//		int Y = Math.Abs(y1 - y2);
	//		float gyp = (float)Math.Sqrt(X * X + Y * Y);
	//		float sinA = X / gyp;
	//		float cosA = (float)Math.Sqrt(1 - sinA * sinA);
	//		float half1 = w1 / 2;
	//		float half2 = w2 / 2;
	//		if (x1 > x2 && y1 > y2 || x1 < x2 && y1 < y2)
	//		{
	//			sinA =- 0.70710678118f; // sqrt(2)/2
	//			cosA = (float)Math.Sqrt(1 - sinA * sinA);
	//		}
	//		g.FillEllipse(b, new Rectangle(x1 - w1/2, y1 - w1 / 2, w1, w1));
	//		g.FillEllipse(b, new Rectangle(x2 - w2/2, y2 - w2 / 2, w2, w2));
	//		PointF p1 = new PointF(x1 - w1 / 2 + w1 * cosA / 2 + half1, y1 - w1 / 2 + w1 * sinA / 2 + half1);
	//		PointF p2 = new PointF(x1 - w1 / 2 - w1 * cosA / 2 + half1, y1 - w1 / 2 - w1 * sinA / 2 + half1);
	//		PointF p3 = new PointF(x2 - w2 / 2 - w2 * cosA / 2 + half2, y2 - w2 / 2 - w2 * sinA / 2 + half2);
	//		PointF p4 = new PointF(x2 - w2 / 2 + w2 * cosA / 2 + half2, y2 - w2 / 2 + w2 * sinA / 2 + half2);
	//		PointF[] points = new PointF[] { p1, p2, p3, p4};
	//		g.FillPolygon(b, points);
	//		//g.DrawLine(new Pen(new SolidBrush(Color.Black)), x1, y1, x2, y2);

	//		return g;
	//	}
	//}

	public class Sandbox : IBackendDrawer
	{

		public void Draw(Bitmap newBitmap, Bitmap origBitmap, RenderParams param)
		{
			using (Graphics graphics = Graphics.FromImage(newBitmap))
			{
				graphics.SmoothingMode = param.Smoothing == SmoothingType.Antialias ?
				SmoothingMode.AntiAlias : SmoothingMode.None;
				int w1 = 20;
				int w2 = 80;
				graphics.FillVariableLine(new SolidBrush(Color.Red),			380,	380,	280,	280,	w1,	w2);
				graphics.FillVariableLine(new SolidBrush(Color.Orange),			380,	420,	280,	520,	w1,	w2);
				graphics.FillVariableLine(new SolidBrush(Color.Yellow),			420,	380,	520,	280,	w1,	w2);
				graphics.FillVariableLine(new SolidBrush(Color.DarkOliveGreen),	420,	420,	520,	520,	w1,	w2);

				graphics.FillVariableLine(new SolidBrush(Color.Blue),			400,	380,	400,	280,	w1,	w2);
				graphics.FillVariableLine(new SolidBrush(Color.LightBlue),		380,	400,	280,	400,	w1,	w2);
				graphics.FillVariableLine(new SolidBrush(Color.Violet),			400,	420,	400,	520,	w1,	w2);
				graphics.FillVariableLine(new SolidBrush(Color.Black),			420,	400,	520,	400,	w1,	w2);
			}
		}

		private static void RenderLine(Graphics graphics, List<Point> coords, RenderParams param)
		{
			if (coords.Count == 1)
				return;
			Brush brush = new SolidBrush(Color.Black);
			Pen pen = new Pen(brush, param.Stroke);

			switch (param.LineType)
			{
				case LineType.Line:
					graphics.DrawLines(pen, coords.ToArray());
					break;
				case LineType.Curve:
					graphics.DrawCurve(pen, coords.ToArray());
					break;
				case LineType.Dot:
					foreach (Point coord in coords)
						graphics.FillRectangle(brush, coord.X, coord.Y, param.Stroke, param.Stroke);
					break;
				default:
					break;
			}
		}
	}
}
