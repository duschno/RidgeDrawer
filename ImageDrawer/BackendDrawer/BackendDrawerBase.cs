﻿using System;
using System.Collections.Generic;
using System.Drawing;

namespace ImageDrawer // TODO: каждая линия со своими параматрами, но это уже в афтере - типа рандом такой
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
			while (lineNumber < param.LinesCount)
			{
				List<Point> coords = new List<Point>();
				int y = GetLineY(lineNumber);

				for (int x = origBitmap.Width / 2 % param.ChunkSize; x < origBitmap.Width; x += param.ChunkSize) // TODO: чанки распределять на оси Х не равномерно, а без сдвига, что бы при некратных значениях (50 и 51 наприм) не было фликеринга, а просто добавлялась новая координата
					coords.Add(CalculatePoint(origBitmap, x, y, param));
				if (param.DrawOnSides)
				{
					coords.Insert(0, CalculatePoint(origBitmap, 0, y, param));
					coords.Add(CalculatePoint(origBitmap, origBitmap.Width - 1, y, param));
				}

				RenderLine(coords, param, y);
				lineNumber++;
			}
		}

		private Point CalculatePoint(Bitmap origBitmap, int x, int y, RenderParams param)
		{
			int greyscaleFactored = (int)Math.Round((CalculateGreyScale(origBitmap, x, y, param) * param.Factor)); // round is used because otherwise angle=0 differs from angle=1 for 127 color
			return CalculateAngle(x, y, greyscaleFactored, greyscaleFactored);
		}

		private double CalculateGreyScale(Bitmap origBitmap, int x, int y, RenderParams param)
		{
			int pixel = origBitmap.GetPixel(x, y).Greyscale();
			if (pixel > param.WhitePoint) pixel = param.WhitePoint;
			if (pixel < param.BlackPoint) pixel = param.BlackPoint;

			int f = param.Invert ? param.WhitePoint - pixel : pixel - param.BlackPoint;
			return f / (double)(param.WhitePoint - param.BlackPoint);
		}

		private Point CalculateAngle(int x, int y, int factorX, int factorY)
		{
			double sin = Math.Sin(Math.PI * -param.Angle / 180.0);
			double cos = Math.Cos(Math.PI * -param.Angle / 180.0);
			int c = (int)((factorY - param.Factor / 2.0) * cos);
			return new Point(x + (int)((factorX - param.Factor / 2.0) * sin),
							 y + (int)((factorY - param.Factor / 2.0) * cos));
		}

		private int GetLineY(int lineNumber)
		{
			//	точное положение линии от нуля							прибавляем половину интервала, чтобы было посередине
			return (origBitmap.Height * lineNumber / param.LinesCount) + (origBitmap.Height / (param.LinesCount * 2));
		}

		private void MethodSquiggle() // TODO: фактор у линии тоже должен быть таким, что чем больше частота - тем больше амплитуда
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
				int accumulator = minChunk;
				for (int x = 1; x < origBitmap.Width; x += accumulator)
				{
					double greyscale = CalculateGreyScale(origBitmap, x, y, param);
					accumulator = (int)(maxChunk - (maxChunk - minChunk) * greyscale); // TODO: добавить грей поинт. который будет центром. по дефолту приращение вниз и вверх одинаковое, но например при грей поинте 10 приращние белого будет намного сильнее, чем черного

					Point point = CalculateAngle(x, y, accumulator, (int)(sign * param.Factor * greyscale));
					point.Y += param.Factor / 2;
					coords.Add(point);
					sign *= -1;
				}

				if (param.DrawOnSides)
				{
					int stepLeft = coords[1].X - coords[0].X;
					int stepRight = coords[coords.Count - 1].X - coords[coords.Count - 2].X;
					coords.Insert(0, new Point(-stepLeft, coords[1].Y));
					coords.Add(new Point(origBitmap.Width - 1 + stepRight, coords[coords.Count - 2].Y));
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