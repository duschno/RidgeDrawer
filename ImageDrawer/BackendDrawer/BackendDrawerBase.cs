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

		private void MethodRidge() // TODO: чекни скрин на телефоне с женщиной, там линии объемно смещаются от центра
		{
			int lineNumber = 0; // сейчас он считает так: насколько относительно серого цвета сместить вверх или вниз линию. надо переделать от белого
			while (lineNumber < param.LinesCount) // TODO: delete grey point and use white an dblack instead
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
			if (greyscale > param.WhitePoint) greyscale = param.WhitePoint;
			if (greyscale < param.BlackPoint) greyscale = param.BlackPoint;
			int factor = param.Factor * (greyscale - param.BlackPoint) / (param.WhitePoint - param.BlackPoint);
			return CalculateAngle(x, y, factor, factor);
		}

		private Point CalculateAngle(int x, int y, int factorX, int factorY)
		{
			return new Point(x + (int)(factorX * Math.Sin(Math.PI * -param.Angle / 180.0)),
							 y + (int)(factorY * Math.Cos(Math.PI * -param.Angle / 180.0)));
		}

		private int GetLineY(int lineNumber)
		{
			//	точное положение линии от нуля							прибавляем половину интервала, чтобы было посередине
			return (origBitmap.Height * lineNumber / param.LinesCount) + (origBitmap.Height / (param.LinesCount * 2));
		}

		private void MethodSquiggle()
		{
			if (param.WhitePoint <= param.BlackPoint)
				throw new NotImplementedException($"White point is less or equal to black point");

			int maxChunk = 20;
			int minChunk = 3;

			int lineNumber = 0;
			while (lineNumber < param.LinesCount)
			{
				List<Point> coords = new List<Point>();
				int sign = -1;
				int y = GetLineY(lineNumber);
				coords.Add(new Point(0, y));
				int accumulator = minChunk;
				for (int x = 1; x < origBitmap.Width; x += accumulator)
				{
					int pixel = origBitmap.GetPixel(x, y).Greyscale();
					if (pixel > param.WhitePoint) pixel = param.WhitePoint;
					if (pixel < param.BlackPoint) pixel = param.BlackPoint;

					int f = param.Invert ? param.WhitePoint - pixel : pixel - param.BlackPoint;
					double greyscale = f / (double)(param.WhitePoint - param.BlackPoint);
					accumulator = (int)(maxChunk - (maxChunk - minChunk) * greyscale);

					coords.Add(CalculateAngle(x, y, accumulator, (int)(sign * param.Factor * greyscale)));
					//coords.Add(new Point(x + accumulator,
					//					 y + (int)(sign * param.Factor * greyscale))); // TODO: angle support
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