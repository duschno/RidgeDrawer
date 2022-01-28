using System;
using System.Collections.Generic;
using System.Drawing;

namespace ImageDrawer // TODO: каждая линия со своими параматрами, но это уже в афтере - типа рандом такой
{
	public abstract class BackendDrawerBase
	{
		#region Abstract methods

		protected abstract void DrawLines(MyPoint[] coords);
		protected abstract void DrawDots(MyPoint[] coords);
		protected abstract void DrawVariableLines(MyPoint[] coords, int y);
		protected abstract void DrawCurve(MyPoint[] coords);
		protected abstract void DrawBezier(MyPoint[] coords);
		protected abstract void DrawDebugInfo();

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
			switch (param.Method) // TODO: не рисовать линии без приращения
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

			if (param.Debug)
				DrawDebugInfo();
		}

		private void MethodRidge() // TODO: чекни скрин на телефоне с женщиной, там линии объемно смещаются от центра
		{
			int lineNumber = 0; // сейчас он считает так: насколько относительно серого цвета сместить вверх или вниз линию. надо переделать от белого
			while (lineNumber < param.LinesCount)
			{
				List<MyPoint> coords = new List<MyPoint>();
				int y = GetLineY(lineNumber);

				for (int x = origBitmap.Width / 2 % param.ChunkSize; x < origBitmap.Width; x += param.ChunkSize) // TODO: чанки распределять на оси Х не равномерно, а без сдвига, что бы при некратных значениях (50 и 51 наприм) не было фликеринга, а просто добавлялась новая координата
					coords.Add(CalculatePoint(origBitmap, x, y, param));
				if (param.DrawOnSides)
				{
					coords.Insert(0, CalculatePoint(origBitmap, 0, y, param));
					coords.Add(CalculatePoint(origBitmap, origBitmap.Width - 1, y, param));
				}

				foreach (List<MyPoint> coordsPart in GetAffectedPoints(coords, y))
					RenderLine(coordsPart, param, y);
				lineNumber++;
			}
		}

		private List<List<MyPoint>> GetAffectedPoints(List<MyPoint> coords, int zeroLevel)
		{
			if (param.PointsAroundPeak == -1) // если -1 - рисовать все, если 0 - не рисовать ничего, если 1 - осавлять 1 грей поинт и т.д.
				return new List<List<MyPoint>>() { coords };

			List<List<MyPoint>> coordsParts = new List<List<MyPoint>>();
			int endIndex = 0;
			int startIndex = 0;
			while (startIndex != -1 && endIndex != -1)
			{
				startIndex = coords.FindIndex(endIndex, p => p.Y != zeroLevel);
				if (startIndex != -1)
				{
					endIndex = coords.FindIndex(startIndex, p => p.Y == zeroLevel /* либо достигли конца*/);
					if (endIndex != -1)
					{
						coordsParts.Add(coords.GetRange(startIndex - (startIndex > 0 ? 1 : 0), endIndex - startIndex + 1 + (endIndex < coords.Count - 1 ? 1 : 0)));
					}
				}
			}


			//int shift = 0;
			//while (shift < coords.Count)
			//{
			//	startIndex = coords.FindIndex(shift, p => p.Y != zeroLevel);
			//	if (startIndex == -1)
			//		return coordsParts;
			//	endIndex = coords.FindIndex(startIndex, p => p.Y == zeroLevel);
			//	coordsParts.Add(coords.GetRange(startIndex, endIndex - startIndex));
			//	shift += startIndex;
			//}

			return coordsParts;
		}

		/// <summary>
		/// Calculates a distance between two points
		/// </summary>
		/// <param name="p1">First point</param>
		/// <param name="p2">Second point</param>
		/// <returns></returns>
		private double Distance(MyPoint p1, MyPoint p2)
		{
			int x = p1.X - p2.X;
			int y = p1.Y - p2.Y;
			return Math.Sqrt(x * x + y * y);
		}

		private MyPoint PullToPoint(MyPoint point, double force)
		{
			return point;

			if (force == 0)
			{
				return point;
			}
			int centerX = param.PullPointX;
			int centerY = param.PullPointY;

			bool mustNotBeLessThanCenterX = point.X > centerX;
			bool mustNotBeLessThanCenterY = point.Y > centerY;

			double len = Distance(new MyPoint(centerX, centerY), point);

			point.X += (int)((centerX - point.X)/* * force*/ * len * 0.0002);
			point.Y += (int)((centerY - point.Y)/* * force*/ * len * 0.0002);
			if (mustNotBeLessThanCenterX)
			{
				if (point.X < centerX)
					point.X = centerX;
			}
			else
			{
				if (point.X > centerX)
					point.X = centerX;
			}

			if (mustNotBeLessThanCenterY)
			{
				if (point.Y < centerY)
					point.Y = centerY;
			}
			else
			{
				if (point.Y > centerY)
					point.Y = centerY;
			}

			return point;
		}

		private MyPoint CalculatePoint(Bitmap origBitmap, int x, int y, RenderParams param)
		{
			int greyscaleFactored = (int)Math.Round(CalculateGreyScale(origBitmap, x, y, param) * param.Factor); // round is used because otherwise angle=0 differs from angle=1 for 127 color. ебаное решение, переделывай
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

		private MyPoint CalculateAngle(int x, int y, int factorX, int factorY)
		{
			double sin = Math.Sin(Math.PI * -param.Angle / 180.0); // param.Angle is negative to rotate it clockwise
			double cos = Math.Cos(Math.PI * -param.Angle / 180.0);
			int xAddition = (int)((factorX - param.Factor / 2.0) * sin); // вычитаем param.Factor / 2.0, чтобы линии построенные по серому не сдвигались. но с черным и белым это не работает. мб все таки ввести точку серого?
			int yAddition = (int)((factorY - param.Factor / 2.0) * cos);
			double len = Distance(new MyPoint(x, y), new MyPoint(x + xAddition, y + yAddition));
			return PullToPoint(new MyPoint(x + xAddition, y + yAddition), len);
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
			double greyGreyscale = 127 / 255.0;
			int greyAccumulator = (int)(maxChunk - (maxChunk - minChunk) * greyGreyscale);

			int lineNumber = 0;
			while (lineNumber < param.LinesCount)
			{
				List<MyPoint> coords = new List<MyPoint>();
				int sign = -1;
				int y = GetLineY(lineNumber);
				int accumulator = minChunk;
				bool prevStepCorrected = false;
				int xStart = 1;
				for (int x = xStart; x < origBitmap.Width; x += accumulator)
				{
					double greyscale = CalculateGreyScale(origBitmap, x, y, param);
					int oldAccumulator = accumulator;
					accumulator = (int)(maxChunk - (maxChunk - minChunk) * greyscale); // TODO: добавить грей поинт. который будет центром. по дефолту приращение вниз и вверх одинаковое, но например при грей поинте 10 приращние белого будет намного сильнее, чем черного

					if (!prevStepCorrected) // point correction for grey color TODO: checkbox "syncronize"
					{
						bool cond0 = greyscale == greyGreyscale;
						bool cond1 = x + greyAccumulator < origBitmap.Width && CalculateGreyScale(origBitmap, x + greyAccumulator, y, param) == greyGreyscale;
						bool cond2 = x + 2 * greyAccumulator < origBitmap.Width && CalculateGreyScale(origBitmap, x + 2 * greyAccumulator, y, param) == greyGreyscale;
						bool cond3 = x - oldAccumulator > 0 && CalculateGreyScale(origBitmap, x - oldAccumulator, y, param) != greyGreyscale; // стоит ли фиксить случай когда 
						if (cond0 && cond1 && cond2 && cond3)
						{
							var leftValue = xStart + (x - xStart) / greyAccumulator * greyAccumulator;
							var rightValue = leftValue + greyAccumulator;

							if (coords.Count > 0 && coords[coords.Count - 1].X > leftValue)
								x = rightValue;
							else
							{
								if (x - leftValue < rightValue - x)
									x = leftValue;
								else
									x = rightValue;
							}

							sign = (x / greyAccumulator) % 2 == 0 ? -1 : 1;
							accumulator = greyAccumulator;
							prevStepCorrected = true;
						}
					}
					else
					{
						prevStepCorrected = false;
					}

					MyPoint point = CalculateAngle(x, y, accumulator, (int)(sign * param.Factor * greyscale));
					point.Y += param.Factor / 2;
					coords.Add(point);
					sign *= -1;
				}

				if (param.DrawOnSides)
				{
					MyPoint p1 = coords[0];
					MyPoint p2 = coords[1];
					MyPoint pN1 = coords[coords.Count - 1]; // p[N-1]
					MyPoint pN2 = coords[coords.Count - 2]; // p[N-2]
					int stepLeft = p2.X - p1.X;
					int stepRight = pN1.X - pN2.X;

					coords.Insert(0, new MyPoint(xStart - stepLeft, p2.Y)); // не нужно считать угол, потому что он уже был посчитан для точек, которыми тут оперируем
					coords.Add(new MyPoint(pN1.X + stepRight, pN2.Y)); // TODO: одной новой точки иногда не хватает, все равно остается пустота
				}

				foreach (List<MyPoint> coordsPart in GetAffectedPoints(coords, y))
					RenderLine(coordsPart, param, y);
				lineNumber++;
			}
		}

		private void RenderLine(List<MyPoint> coords, RenderParams param, int y)
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