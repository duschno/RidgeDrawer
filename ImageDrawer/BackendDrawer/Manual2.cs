using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace ImageDrawer
{
	public class Manual2 : BackendDrawerBase
	{
		protected override void DrawBezier(Point[] coords)
		{
			throw new NotImplementedException();
		}

		protected override void DrawCurve(Point[] coords)
		{
			throw new NotImplementedException();
		}

		protected override void DrawDots(Point[] coords)
		{
			throw new NotImplementedException();
		}

		protected override void DrawLines(Point[] coords)
		{
			for (int i = 0; i < coords.Length - 1; i++)
			{
				Point a = coords[i];
				Point b = coords[i + 1];

				int deltax = Math.Abs(b.X - a.X) + 1;
				int deltay = Math.Abs(b.Y - b.Y) + 1;
				int error = 0;
				int y = a.Y;
				int diry = b.Y - a.Y;
				if (diry > 0)
					diry = 1;
				if (diry < 0)
					diry = -1;
				for (int x = a.X; x <= b.X; x++)
				{
					if (x > 0 && x < newBitmap.Width && y > 0 && y < newBitmap.Height)
						newBitmap.SetPixel(x, y, Color.Black);
					error += deltay;
					if (error >= deltax)
					{
						y += diry;
						error -= deltax;
					}
				}
			}
		}

		protected override void DrawVariableLines(Point[] coords, int y)
		{
			throw new NotImplementedException();
		}
	}
}
