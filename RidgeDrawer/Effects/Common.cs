using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RidgeDrawer
{
	public class Common
	{
		protected List<List<MyPoint>> GetAffectedPoints(List<MyPoint> coords, int zeroLevel, RenderParams param)
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
		protected double Distance(MyPoint p1, MyPoint p2)
		{
			int x = p1.X - p2.X;
			int y = p1.Y - p2.Y;
			return Math.Sqrt(x * x + y * y);
		}

		protected MyPoint PullToPoint(MyPoint point, double force, RenderParams param)
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

		protected MyPoint CalculatePoint(Bitmap origBitmap, int x, int y, RenderParams param)
		{
			int greyscaleFactored = (int)Math.Round(CalculateGreyScale(origBitmap, x, y, param) * param.Factor); // round is used because otherwise angle=0 differs from angle=1 for 127 color. ебаное решение, переделывай
			return CalculateAngle(x, y, greyscaleFactored, greyscaleFactored, param);
		}

		protected double CalculateGreyScale(Bitmap origBitmap, int x, int y, RenderParams param)
		{
			int pixel = origBitmap.GetPixel(x, y).Greyscale();
			if (pixel > param.WhitePoint) pixel = param.WhitePoint;
			if (pixel < param.BlackPoint) pixel = param.BlackPoint;

			int f = param.Invert ? param.WhitePoint - pixel : pixel - param.BlackPoint;
			return f / (double)(param.WhitePoint - param.BlackPoint);
		}

		protected MyPoint CalculateAngle(int x, int y, int factorX, int factorY, RenderParams param)
		{
			double sin = Math.Sin(Math.PI * -param.Angle / 180.0); // param.Angle is negative to rotate it clockwise
			double cos = Math.Cos(Math.PI * -param.Angle / 180.0);
			int xAddition = (int)((factorX - param.Factor / 2.0) * sin); // вычитаем param.Factor / 2.0, чтобы линии построенные по серому не сдвигались. но с черным и белым это не работает. мб все таки ввести точку серого?
			int yAddition = (int)((factorY - param.Factor / 2.0) * cos);
			double len = Distance(new MyPoint(x, y), new MyPoint(x + xAddition, y + yAddition));
			return PullToPoint(new MyPoint(x + xAddition, y + yAddition), len, param);
		}

		protected int GetLineY(int lineNumber, Bitmap origBitmap, RenderParams param)
		{
			//	точное положение линии от нуля							прибавляем половину интервала, чтобы было посередине
			return (origBitmap.Height * lineNumber / param.LinesCount) + (origBitmap.Height / (param.LinesCount * 2));
		}

		protected void RenderLine(BackendBase backend, List<MyPoint> coords, RenderParams param, int y)
		{
			if (coords.Count < 2)
				return;
			switch (param.LineType)
			{
				case LineType.Line:
					backend.DrawLines(coords.ToArray());
					break;
				case LineType.VariableLine:
					backend.DrawVariableLines(coords.ToArray(), y);
					break;
				case LineType.Curve:
					backend.DrawCurve(coords.ToArray());
					break;
				case LineType.Bezier:
					backend.DrawBezier(coords.ToArray());
					break;
				case LineType.Dot:
					backend.DrawDots(coords.ToArray());
					break;
				default:
					throw new NotImplementedException($"{param.LineType} line is not supported");
			}

		}
	}
}
