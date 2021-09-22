using System;
using System.Collections.Generic;
using System.Drawing;

namespace ImageDrawer
{
	public abstract class BackendDrawerBase
	{
		#region Abstract methods

		protected abstract void DrawLines(Point[] coords);
		protected abstract void DrawDots(Point[] coords);
		protected abstract void DrawVariableLines(Point[] coords, int y);
		protected abstract void DrawCurve(Point[] coords);
		protected abstract void DrawBezier(Point[] coords);

		#endregion

		protected Bitmap newBitmap;
		protected Bitmap origBitmap;
		protected RenderParams param;

		public virtual void Construct(Bitmap newBitmap, Bitmap origBitmap, RenderParams param)
		{
			this.newBitmap = newBitmap;
			this.origBitmap = origBitmap;
			this.param = param;
		}

		#region Generic drawing logic

		public void Draw()
		{
			switch (param.Method)
			{
				case MethodType.Ridge:
					MethodRidge();
					break;
				case MethodType.Squiggle:
					MethodSquiggle();
					break;
				default:
					throw new NotImplementedException($"{param.Method} drawing method is not supported");
			}
		}

		private void MethodRidge()
		{
			int lineNumber = 0;
			while (lineNumber < param.LinesCount)
			{
				List<Point> coords = new List<Point>();
				int y = (origBitmap.Height * lineNumber / param.LinesCount) + (origBitmap.Height / (param.LinesCount * 2));

				if (param.DrawOnSides)
					coords.Add(CalculatePoint(origBitmap, 0, y, param));
				for (int x = origBitmap.Width / 2 % param.ChunkSize; x < origBitmap.Width; x += param.ChunkSize)
					coords.Add(CalculatePoint(origBitmap, x, y, param));
				if (param.DrawOnSides)
					coords.Add(CalculatePoint(origBitmap, origBitmap.Width - 1, y, param));

				RenderLine(coords, param, y);
				lineNumber++;
			}
		}

		private Point CalculatePoint(Bitmap origBitmap, int x, int y, RenderParams param)
		{
			Color pixel = origBitmap.GetPixel(x, y);
			int grayscale = (pixel.R + pixel.G + pixel.B) / 3;
			int factor = param.Factor * grayscale / (param.GreyLevel == 0 ? 1 : param.GreyLevel);
			return new Point(x + (int)(factor * Math.Sin(Math.PI * -param.Angle / 180.0)),
							 y + (int)(factor * Math.Cos(Math.PI * -param.Angle / 180.0)));
		}

		private void MethodSquiggle()
		{
			int lineNumber = 0;
			while (lineNumber < param.LinesCount)
			{
				List<Point> coords = new List<Point>();
				int sign = -1;
				int y = (origBitmap.Height * lineNumber / param.LinesCount) + (origBitmap.Height / (param.LinesCount * 2));
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

				RenderLine(coords, param, y);
				lineNumber++;
			}
		}

		private void RenderLine(List<Point> coords, RenderParams param, int y)
		{
			if (coords.Count < 2)
				return;
			switch (param.LineType)
			{
				case LineType.Line:
					DrawLines(coords.ToArray());
					break;
				case LineType.VariableLine:
					DrawVariableLines(coords.ToArray(), y);
					break;
				case LineType.Curve:
					DrawCurve(coords.ToArray());
					break;
				case LineType.Bezier:
					DrawBezier(coords.ToArray());
					break;
				case LineType.Dot:
					DrawDots(coords.ToArray());
					break;
				default:
					throw new NotImplementedException($"{param.LineType} line is not supported");
			}

		}

		#endregion
	}
}