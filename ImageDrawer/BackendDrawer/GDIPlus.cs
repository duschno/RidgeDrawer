﻿using System;
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

				RenderLine(graphics, coords, param);
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

				RenderLine(graphics, coords, param);
				lineNumber++;
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
