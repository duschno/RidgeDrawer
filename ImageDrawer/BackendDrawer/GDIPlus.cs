using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace ImageDrawer
{
	public class GDIPlus : IBackendDrawer
	{
		public void Draw(Graphics graphics, Bitmap bmp, RenderParams param)
		{
			graphics.SmoothingMode = param.Smoothing == SmoothingType.Antialias ?
				SmoothingMode.AntiAlias : SmoothingMode.None;
			switch (param.Method)
			{
				case MethodType.Ridge:
					MethodRidge(graphics, bmp, param);
					break;
				case MethodType.Squiggle:
					MethodSquiggle(graphics, bmp, param);
					break;
				default:
					break;
			}
		}

		static void MethodRidge(Graphics graphics, Bitmap bmp, RenderParams param)
		{
			int lineNumber = 0;
			while (lineNumber < param.LinesCount)
			{
				List<Point> coords = new List<Point>();
				int y = bmp.Height * lineNumber / param.LinesCount + bmp.Height / (param.LinesCount * 2);
				coords.Add(new Point(0, y));
				for (int x = 1; x < bmp.Width; x += param.ChunkSize)
				{
					Color pixel = bmp.GetPixel(x, y);
					int grayscale = (pixel.R + pixel.G + pixel.B) / 3;
					int factor = param.Factor * (grayscale - 127) / 127;
					coords.Add(new Point(x + (int)(factor * Math.Sin(Math.PI * -param.Angle / 180.0)),
										 y + (int)(factor * Math.Cos(Math.PI * -param.Angle / 180.0))));
				}

				RenderLine(graphics, coords, param, y);
				lineNumber++;
			}
		}

		static void MethodSquiggle(Graphics graphics, Bitmap bmp, RenderParams param)
		{
			int lineNumber = 0;
			while (lineNumber < param.LinesCount)
			{
				List<Point> coords = new List<Point>();
				int sign = -1;
				int y = bmp.Height * lineNumber / param.LinesCount + bmp.Height / (param.LinesCount * 2);
				coords.Add(new Point(0, y));
				int accumulator = param.ChunkSize;
				for (int x = 1; x < bmp.Width; x += accumulator)
				{
					Color pixel = bmp.GetPixel(x, y);
					int grayscale = 255 - (pixel.R + pixel.G + pixel.B) / 3;
					accumulator = (param.ChunkSize * (255 - grayscale) + 10) / 10;
					int factor = param.Factor;

					grayscale = grayscale == 0 ? 1 : grayscale;
					coords.Add(new Point(
						x + accumulator, y + (sign * factor * grayscale / 80)));
					sign *= -1;
				}

				RenderLine(graphics, coords, param, y);
				lineNumber++;
			}
		}

		private static void RenderLine(Graphics graphics, List<Point> coords, RenderParams param, int y)
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
				case LineType.VariableLine:
					for (int i = 0; i < coords.Count - 1; i++)
					{
						var coord1 = coords[i];
						var coord2 = coords[i + 1];
						graphics.FillVariableLine(brush, coord1.X, coord1.Y, coord2.X, coord2.Y, 1/*Math.Abs(y - coord1.Y) / 2*/, Math.Abs(y - coord2.Y));
					}
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

	public static class GraphicsExtension
	{
		public static Graphics FillVariableLine(this Graphics g, Brush b, int x1, int y1, int x2, int y2, int w1, int w2)
		{
			int X = Math.Abs(x1 - x2);
			int Y = Math.Abs(y1 - y2);
			float gyp = (float)Math.Sqrt(X * X + Y * Y);
			float sinA = X / gyp;
			float cosA = (float)Math.Sqrt(1 - sinA * sinA);
			float half1 = w1 / 2;
			float half2 = w2 / 2;
			if (x1 > x2 && y1 > y2 || x1 < x2 && y1 < y2)
			{
				sinA = -0.70710678118f; // sqrt(2)/2
				cosA = (float)Math.Sqrt(1 - sinA * sinA);
			}
			g.FillEllipse(b, new Rectangle(x1 - w1 / 2, y1 - w1 / 2, w1, w1));
			g.FillEllipse(b, new Rectangle(x2 - w2 / 2, y2 - w2 / 2, w2, w2));
			PointF p1 = new PointF(x1 - w1 / 2 + w1 * cosA / 2 + half1, y1 - w1 / 2 + w1 * sinA / 2 + half1);
			PointF p2 = new PointF(x1 - w1 / 2 - w1 * cosA / 2 + half1, y1 - w1 / 2 - w1 * sinA / 2 + half1);
			PointF p3 = new PointF(x2 - w2 / 2 - w2 * cosA / 2 + half2, y2 - w2 / 2 - w2 * sinA / 2 + half2);
			PointF p4 = new PointF(x2 - w2 / 2 + w2 * cosA / 2 + half2, y2 - w2 / 2 + w2 * sinA / 2 + half2);
			PointF[] points = new PointF[] { p1, p2, p3, p4 };
			g.FillPolygon(b, points);
			//g.DrawLine(new Pen(new SolidBrush(Color.Black)), x1, y1, x2, y2);

			return g;
		}
	}
}
