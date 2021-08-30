using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace ImageDrawer
{
	public class GDIPlus : IBackendDrawer
	{
		public void Draw(Bitmap newBitmap, Bitmap origBitmap, RenderParams param)
		{
			using (Graphics graphics = Graphics.FromImage(newBitmap))
			{
				graphics.SmoothingMode = param.Smoothing == SmoothingType.Antialias ?
				SmoothingMode.AntiAlias : SmoothingMode.None;
				switch (param.Method)
				{
					case MethodType.Ridge:
						MethodRidge(graphics, origBitmap, param);
						break;
					case MethodType.Squiggle:
						MethodSquiggle(graphics, origBitmap, param);
						break;
					default:
						break;
				}
			}
		}

		static void MethodRidge(Graphics graphics, Bitmap origBitmap, RenderParams param)
		{
			int lineNumber = 0;
			while (lineNumber < param.LinesCount)
			{
				List<Point> coords = new List<Point>();
				int y = origBitmap.Height * lineNumber / param.LinesCount + origBitmap.Height / (param.LinesCount * 2);

				if (param.DrawOnSides)
					coords.Add(CalculatePoint(origBitmap, 0, y, param));
				for (int x = (origBitmap.Width / 2) % param.ChunkSize; x < origBitmap.Width; x += param.ChunkSize)
					coords.Add(CalculatePoint(origBitmap, x, y, param));
				if (param.DrawOnSides)
					coords.Add(CalculatePoint(origBitmap, origBitmap.Width - 1, y, param));

				RenderLine(graphics, coords, param, y);
				lineNumber++;
			}
		}

		static Point CalculatePoint(Bitmap origBitmap, int x, int y, RenderParams param)
		{
			Color pixel = origBitmap.GetPixel(x, y);
			int grayscale = (pixel.R + pixel.G + pixel.B) / 3;
			int factor = param.Factor * (grayscale - 127) / 127;
			return new Point(x + (int)(factor * Math.Sin(Math.PI * -param.Angle / 180.0)),
							 y + (int)(factor * Math.Cos(Math.PI * -param.Angle / 180.0)));
		}

		static void MethodSquiggle(Graphics graphics, Bitmap origBitmap, RenderParams param)
		{
			int lineNumber = 0;
			while (lineNumber < param.LinesCount)
			{
				List<Point> coords = new List<Point>();
				int sign = -1;
				int y = origBitmap.Height * lineNumber / param.LinesCount + origBitmap.Height / (param.LinesCount * 2);
				coords.Add(new Point(0, y));
				int accumulator = param.ChunkSize;
				for (int x = 1; x < origBitmap.Width; x += accumulator)
				{
					Color pixel = origBitmap.GetPixel(x, y);
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
			SolidBrush brush = new SolidBrush(Color.Black);
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
						graphics.FillVariableLine(brush, coord1.X, coord1.Y, coord2.X, coord2.Y, Math.Abs(y - coord1.Y) , Math.Abs(y - coord2.Y));
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
}
