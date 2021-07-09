using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace ImageDrawer
{
	interface IBackendDrawer
	{
		void Draw(Graphics g, Bitmap bmp, RenderParams param);
	}

	public class GDIPlus : IBackendDrawer
	{
		SmoothingMode smoothing;
		MethodType method;
		public void Draw(Graphics graphics, Bitmap bmp, RenderParams param)
		{
			graphics.SmoothingMode = param.Smoothing == SmoothingType.Antialias ?
				SmoothingMode.AntiAlias : SmoothingMode.None;
			switch (method)
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
				List<System.Drawing.Point> coords = new List<System.Drawing.Point>();
				int y = bmp.Height * lineNumber / param.LinesCount + bmp.Height / (param.LinesCount * 2);
				coords.Add(new System.Drawing.Point(0, y));
				for (int x = 1; x < bmp.Width; x += param.ChunkSize)
				{
					System.Drawing.Color pixel = bmp.GetPixel(x, y);
					int grayscale = (pixel.R + pixel.G + pixel.B) / 3;
					int factor = param.Factor * (grayscale - 127) / 127;
					coords.Add(new System.Drawing.Point(x, y + factor));
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
				List<System.Drawing.Point> coords = new List<System.Drawing.Point>();
				int sign = -1;
				int y = bmp.Height * lineNumber / param.LinesCount + bmp.Height / (param.LinesCount * 2);
				coords.Add(new System.Drawing.Point(0, y));
				int accumulator = param.ChunkSize;
				for (int x = 1; x < bmp.Width; x += accumulator)
				{
					System.Drawing.Color pixel = bmp.GetPixel(x, y);
					int grayscale = 255 - (pixel.R + pixel.G + pixel.B) / 3;
					accumulator = (param.ChunkSize * (255 - grayscale) + 10) / 10;
					int factor = param.Factor;

					grayscale = grayscale == 0 ? 1 : grayscale;
					coords.Add(new System.Drawing.Point(
						x + accumulator, y + (sign * factor * grayscale / 80)));
					sign *= -1;
				}

				RenderLine(graphics, coords, param);
				lineNumber++;
			}
		}

		private static void RenderLine(Graphics graphics, List<System.Drawing.Point> coords, RenderParams param)
		{
			if (coords.Count == 1)
				return;
			System.Drawing.Brush brush = new SolidBrush(System.Drawing.Color.Black);
			System.Drawing.Pen pen = new System.Drawing.Pen(brush, param.Width);

			switch (param.LineType)
			{
				case LineType.Line:
					graphics.DrawLines(pen, coords.ToArray());
					break;
				case LineType.Curve:
					graphics.DrawCurve(pen, coords.ToArray());
					break;
				case LineType.Dot:
					foreach (System.Drawing.Point coord in coords)
						graphics.FillRectangle(brush, coord.X, coord.Y, param.Width, param.Width);
					break;
				default:
					break;
			}
		}
	}

	public class Cairo : IBackendDrawer
	{
		[DllImport(@"C:\Users\User\Desktop\ImageDrawer\Debug\PlusPlus.dll", CallingConvention = CallingConvention.Cdecl)]
		private extern static void Draw(IntPtr hdc, int width, int height, int linesCount, int strokeWidth, int factor, int chunkSize);

		public void Draw(Graphics g, int width, int height, int linesCount, int strokeWidth, int factor, int chunkSize)
		{
			IntPtr hdc = g.GetHdc();
			Draw(hdc, width, height, linesCount, strokeWidth, factor, chunkSize);
			g.ReleaseHdc();
		}

		public void Draw(Graphics g, Bitmap bmp, RenderParams param)
		{
			throw new NotImplementedException();
		}
	}
}
