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
				int y = GetLineY(lineNumber);

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
			int greyscale = origBitmap.GetPixel(x, y).Greyscale();
			int factor = param.Factor * greyscale / param.GreyPoint;
			return new Point(x + (int)(factor * Math.Sin(Math.PI * -param.Angle / 180.0)),
							 y + (int)(factor * Math.Cos(Math.PI * -param.Angle / 180.0)));
		}

		private int GetLineY(int lineNumber)
		{
			//	точное положение линии от нуля							прибавляем половину интервала, чтобы было посередине
			return (origBitmap.Height * lineNumber / param.LinesCount) + (origBitmap.Height / (param.LinesCount * 2));
		}

		private void MethodSquiggle()
		{
			if (param.WhitePoint == param.BlackPoint)
				throw new NotImplementedException($"White point is equal to black point");

			int maxLevel = Math.Max(param.WhitePoint, param.BlackPoint);
			int minLevel = Math.Min(param.WhitePoint, param.BlackPoint);

			int lineNumber = 0;
			while (lineNumber < param.LinesCount)
			{
				List<Point> coords = new List<Point>();
				int sign = -1;
				int y = GetLineY(lineNumber);
				coords.Add(new Point(0, y));
				int accumulator = param.ChunkSize;
				for (int x = 1; x < origBitmap.Width; x += accumulator)
				{
					int p = origBitmap.GetPixel(x, y).Greyscale();
					if (p > maxLevel)
						p = maxLevel;
					if (p < minLevel)
						p = minLevel;

					int f = param.WhitePoint > param.BlackPoint ? p - minLevel : maxLevel - p;
					double greyscale = f / (double)(maxLevel - minLevel);
					//accumulator = (int)(param.ChunkSize * greyscale);
					//accumulator = (param.ChunkSize * (255 - greyscale) + 10) / 10;
					//int factor = param.Factor;

					coords.Add(new Point(x + accumulator, y + (int)(sign * param.Factor * greyscale)));
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