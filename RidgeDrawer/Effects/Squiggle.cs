using System;
using System.Collections.Generic;
using System.Drawing;

namespace RidgeDrawer
{
	public class Squiggle : Common, IEffect
	{
		public void Apply(BackendBase backend, Bitmap newBitmap, Bitmap origBitmap, RenderParams param)
		{
			// TODO: фактор у линии тоже должен быть таким, что чем больше частота - тем больше амплитуда
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
				int y = GetLineY(lineNumber, origBitmap, param);
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

					MyPoint point = CalculateAngle(x, y, accumulator, (int)(sign * param.Factor * greyscale), param);
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

				foreach (List<MyPoint> coordsPart in GetAffectedPoints(coords, y, param))
					RenderLine(backend, coordsPart, param, y);
				lineNumber++;
			}
		}
	}
}
