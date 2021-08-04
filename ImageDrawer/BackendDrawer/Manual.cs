using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace ImageDrawer
{
	public class Manual : IBackendDrawer
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
				coords.Add(new Point(0, y));
				for (int x = 1; x < origBitmap.Width; x += param.ChunkSize)
				{
					Color pixel = origBitmap.GetPixel(x, y);
					int grayscale = (pixel.R + pixel.G + pixel.B) / 3;
					int factor = param.Factor * (grayscale - 127) / 127;
					coords.Add(new Point(x + (int)(factor * Math.Sin(Math.PI * -param.Angle / 180.0)),
										 y + (int)(factor * Math.Cos(Math.PI * -param.Angle / 180.0))));
				}

				RenderLine(graphics, coords, param, y);
				lineNumber++;
			}
		}

		static void MethodSquiggle(Graphics graphics, Bitmap origBitmap, RenderParams param)
		{
			throw new NotImplementedException();
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
}
