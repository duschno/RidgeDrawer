using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace ImageDrawer
{
	public class Manual2 : BackendDrawerBase
	{
		private Graphics graphics;
		public override void Draw(Bitmap newBitmap, Bitmap origBitmap, RenderParams param)
		{
			graphics = Graphics.FromImage(newBitmap);
			graphics.SmoothingMode = param.Smoothing == SmoothingType.Antialias ?
			SmoothingMode.AntiAlias : SmoothingMode.None;
			switch (param.Method)
			{
				case MethodType.Ridge:
					MethodRidge(newBitmap, origBitmap, param);
					break;
				case MethodType.Squiggle:
					MethodSquiggle(newBitmap, origBitmap, param);
					break;
				default:
					break;
			}
		}

		public void MethodRidge(Bitmap newBitmap, Bitmap origBitmap, RenderParams param)
		{
			int lineNumber = 0;
			while (lineNumber < param.LinesCount)
			{
				List<Point> coords = new List<Point>();
				int y = origBitmap.Height * lineNumber / param.LinesCount + origBitmap.Height / (param.LinesCount * 2);
				coords.Add(new Point(0, y));
				for (int x = 1; x < origBitmap.Width; x += param.ChunkSize)
				{
					Color pixel = origBitmap.GetPixel(x, y);
					int grayscale = (pixel.R + pixel.G + pixel.B) / 3;
					int factor = param.Factor * (grayscale - 127) / 127;
					coords.Add(new Point(x + (int)(factor * Math.Sin(Math.PI * -param.Angle / 180.0)),
										 y + (int)(factor * Math.Cos(Math.PI * -param.Angle / 180.0))));
				}

				RenderLine(newBitmap, coords, param, y);
				lineNumber++;
			}
		}

		public void MethodSquiggle(Bitmap newBitmap, Bitmap origBitmap, RenderParams param)
		{
			throw new NotImplementedException();
		}

		private void RenderLine(Bitmap newBitmap, List<Point> coords, RenderParams param, int y)
		{
			if (coords.Count == 1)
				return;
			Brush brush = new SolidBrush(Color.Black);
			Pen pen = new Pen(brush, param.Stroke);

			switch (param.LineType)
			{
				case LineType.Line:
					for (int i = 0; i < coords.Count - 1; i++)
						DrawLine(newBitmap, pen.Color, coords[i], coords[i + 1]);
					break;
				case LineType.VariableLine:
					throw new NotImplementedException();
				case LineType.Curve:
					throw new NotImplementedException();
				case LineType.Dot:
					throw new NotImplementedException();
				default:
					break;
			}
		}

		private void DrawLine(Bitmap newBitmap, Color color, Point a, Point b)
		{
			int deltax = Math.Abs(b.X - a.X) + 1;
			int deltay = Math.Abs(b.Y - b.Y) + 1;
			int error = 0;
			int y = a.Y;
			int diry = b.Y - a.Y;
			if (diry > 0)
				diry = 1;
			if (diry < 0)
				diry = -1;
			for (int x = a.X; x <= b.X; x++)
			{
				if (x > 0 && x < newBitmap.Width && y > 0 && y < newBitmap.Height)
					newBitmap.SetPixel(x, y, color);
				error += deltay;
				if (error >= deltax)
				{
					y += diry;
					error -= deltax;
				}
			}
		}
	}
}
